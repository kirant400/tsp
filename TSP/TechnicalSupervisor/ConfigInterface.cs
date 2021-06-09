using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.IO;
using System.Net;
using System.Data.SQLite;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Collections;
using System.Globalization;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ComponentModel;


namespace TechnicalSupervisor
{
    public delegate void XmlChangedEventHandler(object sender, EventArgs e);

    public struct User_action
    {
        public bool Enabled; // Is tsp node enabled?
        public IPEndPoint ep;
        public string raw_sql;
    }

    public struct WebPage_action
    {
        public bool Enabled; // Is tsp node enabled?
        public string URL; // Webpage
        public string desc; // description
    }

    public enum Tsp_hw_target_match_type
    {
        MatchAny = 0, /* Wild card match - e.g. top-level node */
        MatchSource = 1, /* All events from a given source */
        MatchHwID = 2, /* All events from a given hardware ID */
        MatchSourceAndHwID = 3, /* All events on a source with a given hardware ID */
        MatchChannel = 4, /* All events from a given channel */
        MatchSourceAndChannel = 5, /* All events on a source with a given channel */
        MatchHwIDAndChannel = 6, /* All events with a given hardware ID and channel from any source */
        MatchAll = 7, /* Exact match e.g. src + hw_id + channel must match */
    };

    public struct Tsp_hw_target_match
    {
        public string hw_src;
        public string hw_id;
        public int channel;
        public Tsp_hw_target_match_type type;
    }

    public struct Tsp_hw_target
    {
        public int pos;
        public string name;
        public List<Tsp_hw_target_match> matchList;
    }

    public struct Tsp_hw_node
    {
        public bool Enabled; // Is tsp node enabled?
        public int id;
        public Rectangle rect;
        public string text;
        public string prompt; // Message to prompt user before button is actioned..
        public List<WebPage_action> actions;
        public List<User_action> UserActions;
        public bool isActive;
        public bool isSystem; // true if this is the main system button
        public Image img_main;
        public string img_main_fname;
        public List<Tsp_hw_target> targets;
    }

    public delegate void XmlItemChangedEventHandler(object sender, TspItemEventArgs e);
    public class TspItemEventArgs : EventArgs
    {
        int index;
        public int Index
        {
            get { return index; }
        }
        public TspItemEventArgs(int idx) : base()
        {
            index = idx;
        }
    }

    // make as singleton
    public class XmlDatabaseInterface
    {
        public string GetNodeName(int node_id)
        {
            SqliteDatabaseInterface.Sw_id sw_id = LookupNodeId(node_id);
            // Invalid ID
            if (sw_id.id < 0) return "Unknown node";

            // Return hardware node name
            if (sw_id.id == sw_id.hw_id) return db.m_hw_list.Find(x => x.id == sw_id.hw_id).hw_name;

            // Otherwise return software type name
            return db.m_sw_types[sw_id.sw_type];
        }

        public SqliteDatabaseInterface.Sw_id LookupNodeId(int node_id, int sw_type = -1)
        {
            int idx;

            // Try to match hardware ID and SW Type
            if (sw_type >= 0)
            {
                idx = db.m_sw_list.FindIndex(x => (x.hw_id == node_id) && (x.sw_type == sw_type));
                if (idx >= 0)
                {
                    return db.m_sw_list[idx];
                }
            }

            // Try to lookup the hardware node in the software list
            idx = db.m_sw_list.FindIndex(x => x.id == node_id);
            if (idx >= 0)
            {
                return db.m_sw_list[idx];
            }

            
            // Next try to match just the hardware ID
            idx = db.m_hw_list.FindIndex(x => x.id == node_id);
            if (idx >= 0)
            {
                SqliteDatabaseInterface.Sw_id sw_id = new SqliteDatabaseInterface.Sw_id()
                {
                    hw_id = node_id,
                    id = node_id,
                    sw_type = 0,
                };
                return sw_id;
            }

            // Otherwise give up - it does not exist
            SqliteDatabaseInterface.Sw_id lookup_failed = new SqliteDatabaseInterface.Sw_id()
            {
                hw_id = -1,
                id = -1,
                sw_type = 0,
            };
            return lookup_failed;
        }

        public SqliteDatabaseInterface.Event_item GetEventItem(int event_id)
        {
            SqliteDatabaseInterface.Event_item item = db.m_events.Find(x => x.event_id == event_id);
            if (item.name != null) return item;
            item.name = string.Format("Unknown Event {0}", event_id);
            item.desc = "The event ID from logs was not found in the config database.";
            item.action = "Check event and/or config database.";
            return item;
        }

        public bool ExtractXmlResouces(string xml_file, string export_folder)
        {
            try
            {
                // Create folder to store resources (if necessary)
                DirectoryInfo cfg = Directory.CreateDirectory(export_folder);
                if (!cfg.Exists)
                {
                    last_err = "Unable to create folder: " + cfg.FullName;
                    return false;
                }

                // Open the local XML config file
                XmlDocument doc = new XmlDocument();
                doc.Load(xml_file);
                XmlNode root = doc.DocumentElement;

                XmlNodeList resources = root.SelectNodes("resources/resource");
                foreach (XmlNode resource in resources)
                {
                    string fname = GetAttribute(resource, "id", "");
                    if (fname == "") continue;

                    string res_file = GetTextValue(resource, "");
                    if (res_file.Length == 0) return false;
                    byte[] raw_res = Convert.FromBase64String(res_file);
                    if (raw_res.Length == 0) return false;

                    FileStream fd = File.OpenWrite(Path.Combine(cfg.FullName, Path.GetFileName(fname)));
                    BinaryWriter bin = new BinaryWriter(fd);
                    bin.Write(raw_res);
                    bin.Close();
                    fd.Close();
                }
            }
            catch (Exception e)
            {
                // There was a problem expanding the data..
                last_err = e.Message;
                return false;
            }
            return true;

        }

        public void RefreshXmlData(string xml_file, string app_data)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(xml_file);
            XmlNode root = doc.DocumentElement;

            // Get config database + bitmap ID
            XmlNode head = root.SelectSingleNode("head");
            string bg_name = Path.GetFileName(GetAttribute(head, "bg", ""));
            string db_name = Path.GetFileName(GetAttribute(head, "db", ""));

            if (Path.IsPathRooted(bg_name))
            {
                safe_file_copy(bg_name, Path.Combine(app_data, "config"));
            }
            if (Path.IsPathRooted(db_name))
            {
                safe_file_copy(db_name, Path.Combine(app_data, "config"));
            }
        }

        public bool safe_file_copy(string src_file, string dst_folder, string dst_name = null)
        {
            // Will copy file, creating dst_folder if necessary
            // All errors will be silently handled.
            // Returns true if successful.
            if (!File.Exists(src_file)) return false;
            try
            {
                if (dst_name == null) dst_name = Path.GetFileName(src_file);
                if (!Directory.Exists(dst_folder)) Directory.CreateDirectory(dst_folder);
                File.Copy(src_file, Path.Combine(dst_folder, dst_name), true);
                return true;
            } catch (Exception e)
            {
                last_err = e.Message;   
            }
            return false;
        }

        private static XmlDatabaseInterface instance;

        public static XmlDatabaseInterface Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new XmlDatabaseInterface();
                }
                return instance;
            }
        }

        private bool isSaved = true;
        public bool IsSaved
        {
            get { return isSaved; }
        }
        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                isSaved = false;
            }
        }
        private string config;
        public string Config
        {
            get { return config; }
            set
            {
                config = value;
                isSaved = false;
            }
        }
        private Image bg_image;
        public Image Bg_image
        {
            get { return bg_image; }
            set
            {
                bg_image = value;
                isSaved = false;
            }
        }
        private Color background;
        public Color Background
        {
            get { return background; }
            set
            {
                background = value;
                isSaved = false;
            }
        }
        private SqliteDatabaseInterface db;
        public SqliteDatabaseInterface Db
        {
            get { return db; }
            set
            {
                db = value;
                isSaved = false;
            }
        }
        private LogClients logClients;
        public LogClients LogClients
        {
            get { return logClients; }
            set
            {
                logClients = value;
                isSaved = false;
            }
        }
        private List<Tsp_hw_node> tsp_hw_nodes;
        public List<Tsp_hw_node> Tsp_hw_nodes
        {
            get { return tsp_hw_nodes; }
            set
            {
                tsp_hw_nodes = value;
                isSaved = false;
            }
        }

        List<int> hw_list;
        public int[] GetHardwareNodeList()
        {
            return hw_list.ToArray();
        }


        /*
        public XmlDatabaseInterface(string xml_config) : this()
        {
            Config = xml_config;
        }
        */

        private XmlDatabaseInterface()
        {
            logClients = new LogClients();
            tsp_hw_nodes = new List<Tsp_hw_node>();
        }

        public bool SaveXml(string fname)
        {
            try
            {
                return ISaveXml(fname);
            }
            catch (Exception e)
            {
                last_err = e.Message;
            }
            return false;
        }

        public bool ISaveXml(string fname)
        {
            XmlDocument doc = new XmlDocument();
            XmlNode tsp, nodes, node, button;
            XmlAttribute attr;
            XmlElement element;

            // Create tsp/*
            doc.AppendChild(doc.CreateXmlDeclaration("1.0", "UTF-8", "yes"));
            tsp = doc.CreateElement("tsp");
            doc.AppendChild(tsp);


            // Create head node
            node = doc.CreateElement("head");
            element = node as XmlElement;

            attr = doc.CreateAttribute("bg");
            attr.Value = Path.GetFileName(Img_fname);
            element.SetAttributeNode(attr);

            attr = doc.CreateAttribute("db");
            attr.Value = Path.GetFileName(Db_fname);
            element.SetAttributeNode(attr);

            attr = doc.CreateAttribute("font_size");
            if (font_size > 0)
            {
                attr.Value = font_size.ToString();
            }
            else
            {
                attr.Value = Db_fname;
            }
            element.SetAttributeNode(attr);

            node.AppendChild(doc.CreateTextNode(Name));
            tsp.AppendChild(node);


            // Create servers/node
            nodes = doc.CreateElement("servers");

            foreach (LogClient lc in LogClients)
            {
                node = doc.CreateElement("node");
                element = node as XmlElement;

                attr = doc.CreateAttribute("Enabled");
                attr.Value = lc.Enabled.ToString();
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("hwId");
                attr.Value = Db.GetHwId(lc.Hw_id).hw_name;
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("class");
                attr.Value = "logclient";
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("ip");
                attr.Value = lc.Ep.Address.ToString();
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("port");
                attr.Value = lc.Ep.Port.ToString();
                element.SetAttributeNode(attr);

                nodes.AppendChild(node);
            }
            tsp.AppendChild(nodes);


            // Create nodes/node
            nodes = doc.CreateElement("nodes");

            
            foreach (Tsp_hw_node tsp_node in Tsp_hw_nodes)
            {
                node = doc.CreateElement("node");
                element = node as XmlElement;

                
                if (tsp_node.isActive)
                {
                    attr = doc.CreateAttribute("hwId");
                    attr.Value = Db.GetHwId(tsp_node.id).hw_name;
                    element.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("class");
                    attr.Value = "logclient";
                    element.SetAttributeNode(attr);
                }
                else if (tsp_node.isSystem)
                {
                    attr = doc.CreateAttribute("hwId");
                    attr.Value = "*";
                    element.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("class");
                    attr.Value = "system";
                    element.SetAttributeNode(attr);
                }
                else // This is a user node
                {
                    attr = doc.CreateAttribute("hwId");
                    attr.Value = "*";
                    element.SetAttributeNode(attr);

                    attr = doc.CreateAttribute("class");
                    attr.Value = "user";
                    element.SetAttributeNode(attr);
                }

                attr = doc.CreateAttribute("enabled");
                attr.Value = tsp_node.Enabled.ToString();
                element.SetAttributeNode(attr);

                if (tsp_node.prompt != "")
                {
                    attr = doc.CreateAttribute("prompt");
                    attr.Value = tsp_node.prompt; // Prompt is in fact a message to prompt to display to the user with OK/Cancel options
                    element.SetAttributeNode(attr);
                }

                button = doc.CreateElement("button");
                element = button as XmlElement;

                attr = doc.CreateAttribute("x");
                attr.Value = tsp_node.rect.Left.ToString();
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("y");
                attr.Value = tsp_node.rect.Top.ToString();
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("sx");
                attr.Value = tsp_node.rect.Width.ToString();
                element.SetAttributeNode(attr);

                attr = doc.CreateAttribute("sy");
                attr.Value = tsp_node.rect.Height.ToString();
                element.SetAttributeNode(attr);

                // Store image filename (if used)
                if (tsp_node.img_main != null)
                {
                    attr = doc.CreateAttribute("image");
                    attr.Value = Path.GetFileName(tsp_node.img_main_fname);
                    element.SetAttributeNode(attr);
                }

                button.AppendChild(doc.CreateTextNode(tsp_node.text));
                node.AppendChild(button);

                if (tsp_node.actions != null)
                {
                    foreach (WebPage_action uri in tsp_node.actions)
                    {
                        button = doc.CreateElement("menu");
                        element = button as XmlElement;

                        attr = doc.CreateAttribute("Enabled");
                        attr.Value = uri.Enabled.ToString();
                        element.SetAttributeNode(attr);

                        attr = doc.CreateAttribute("src");
                        attr.Value = uri.URL;
                        element.SetAttributeNode(attr);

                        button.AppendChild(doc.CreateTextNode(uri.desc));
                        node.AppendChild(button);
                    }
                }

                if (tsp_node.UserActions != null)
                {
                    foreach (User_action action in tsp_node.UserActions)
                    {
                        button = doc.CreateElement("action");
                        element = button as XmlElement;

                        attr = doc.CreateAttribute("class");
                        attr.Value = "logClient";
                        element.SetAttributeNode(attr);

                        attr = doc.CreateAttribute("ip");
                        attr.Value = action.ep.Address.ToString();
                        element.SetAttributeNode(attr);

                        attr = doc.CreateAttribute("port");
                        attr.Value = action.ep.Port.ToString();
                        element.SetAttributeNode(attr);

                        button.AppendChild(doc.CreateTextNode(action.raw_sql));
                        node.AppendChild(button);
                    }
                }

                nodes.AppendChild(node);
            }
            tsp.AppendChild(nodes);

            // Embed PNG into the file?
            nodes = doc.CreateElement("resources");

            if (Img_fname != null)
            {
                node = doc.CreateElement("resource");
                element = node as XmlElement;

                attr = doc.CreateAttribute("id");
                attr.Value = Path.GetFileName(Img_fname);
                element.SetAttributeNode(attr);

                byte[] png = File.ReadAllBytes(Img_fname);
                string raw_png = Convert.ToBase64String(png);
                node.AppendChild(doc.CreateTextNode(raw_png));
                nodes.AppendChild(node);
            }

            if (Db_fname != null)
            {
                node = doc.CreateElement("resource");
                element = node as XmlElement;

                attr = doc.CreateAttribute("id");
                attr.Value = Path.GetFileName(Db_fname);
                element.SetAttributeNode(attr);

                byte[] db = File.ReadAllBytes(Db_fname);
                string raw_db = Convert.ToBase64String(db);
                node.AppendChild(doc.CreateTextNode(raw_db));
                nodes.AppendChild(node);
            }

            // Store all of the embedded resources as well
            foreach (Tsp_hw_node tsp_node in tsp_hw_nodes)
            {
                if (tsp_node.img_main != null)
                {
                    node = doc.CreateElement("resource");
                    element = node as XmlElement;

                    attr = doc.CreateAttribute("id");
                    attr.Value = Path.GetFileName(tsp_node.img_main_fname);
                    element.SetAttributeNode(attr);

                    MemoryStream mem_file = new MemoryStream();

                    // Save the local copy in case there was any formatting changes
                    tsp_node.img_main.Save(mem_file, System.Drawing.Imaging.ImageFormat.Png /* tsp_node.img_main.RawFormat*/); // Use the same format..
                    byte[] img = mem_file.ToArray(); //  File.ReadAllBytes(tsp_node.img_main_fname);
                    string raw_img = Convert.ToBase64String(img);
                    node.AppendChild(doc.CreateTextNode(raw_img));
                    nodes.AppendChild(node);
                }
            }

            tsp.AppendChild(nodes);


            // Finally store file to disk
            doc.Save(fname);
            isSaved = true;
            return true;
        }

        public string last_err = "";
        public bool ParseXml(string cfg_fname)
        {
            config = Path.GetFullPath(cfg_fname);
            return ParseXml();
        }
        public bool ParseXml()
        {
            try
            {
                return IParseXml();
            }
            catch (Exception e)
            {
                last_err = e.Message;
            }
            return false;
        }

        string img_fname;
        public string Img_fname
        {
            get { return img_fname; }
        }

        public bool LoadImage(string bg)
        {
            if (!File.Exists(bg))
            {
                last_err = String.Format("Unable to find bitmap: {0}", Path.GetFullPath(bg));
                return false;
            }
            img_fname = bg;
            Bg_image = new Bitmap(img_fname);
            Background = (Bg_image as Bitmap).GetPixel(0, 0);
            OnXmlChanged(EventArgs.Empty);
            return true;
        }

        string m_db_fname;
        public string Db_fname {
            get { return m_db_fname; }
        }
        public bool LoadSQLiteDatabase(string db_fname)
        {
            if (!File.Exists(db_fname))
            {
                last_err = String.Format("Unable to find SQLite database: {0}", Path.GetFullPath(db_fname));
                return false;
            }
            m_db_fname = db_fname;
            Db = new SqliteDatabaseInterface(db_fname);
            if (!Db.Load(out last_err))
            {
                return false;
            }
            OnXmlChanged(EventArgs.Empty);
            return true;
        }

        // Create an event handler to detect SQL database changes
        public event XmlChangedEventHandler XmlChanged;
        protected virtual void OnXmlChanged(EventArgs e)
        {
            if (XmlChanged != null)
            {
                XmlChanged(this, e);
            }
        }

        // Create an event handler to detect SQL database changes
        public event XmlItemChangedEventHandler XmlItemChanged;
        protected virtual void OnXmlItemChanged(TspItemEventArgs e)
        {
            if (XmlItemChanged != null)
            {
                XmlItemChanged(this, e);
            }
        }

        public void RemoveTspItem(int index)
        {
            Tsp_hw_nodes.RemoveAt(index);
            OnXmlChanged(EventArgs.Empty);
            isSaved = false;
        }
        public void UpdateTspItem(int index, Tsp_hw_node newValue)
        {
            Tsp_hw_nodes[index] = newValue;
            OnXmlItemChanged(new TspItemEventArgs(index));
            isSaved = false;
        }

        public int compareTspItem(Tsp_hw_node a, Tsp_hw_node b)
        {
            if (a.isSystem)
            {
                if (!b.isSystem) return -1; // This is greater
                return a.text.CompareTo(b.text); // Otherwise compare labels
            }
            else if (!a.isActive)
            {
                if (b.isSystem) return 1; // This is lower
                if (b.isActive) return -1; // This is greater
                return a.text.CompareTo(b.text); // Otherwise compare labels
            }
            else
            {
                if (b.isSystem) return 1; // This is lower
                if (!b.isActive) return 1; // This is lower
                return a.id.CompareTo(b.id); // Otherwise compare hardware_id
            }
        }
        public void SortTspItems()
        {
            tsp_hw_nodes.Sort(compareTspItem);
            OnXmlChanged(EventArgs.Empty);
            isSaved = false;
        }

        public void AddTspItem(Tsp_hw_node newValue)
        {
            int last_idx;
            if (newValue.isActive)
            {
                last_idx = tsp_hw_nodes.FindIndex(x => x.id > newValue.id);
                if (last_idx < 0)
                {
                    // Append
                    Tsp_hw_nodes.Add(newValue);
                    last_idx = Tsp_hw_nodes.Count - 1;
                }
                else
                {
                    Tsp_hw_nodes.Insert(last_idx, newValue);
                }
            }
            else
            {
                Tsp_hw_nodes.Add(newValue);
                last_idx = Tsp_hw_nodes.Count - 1;
            }
            OnXmlItemChanged(new TspItemEventArgs(last_idx));
            isSaved = false;
        }

        float font_size;
        public float Font_size
        {
            get { return font_size;  }
            set {
                font_size = value;
                isSaved = false;
            }
        }

        public bool IParseXml()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(Config);
            XmlNode root = doc.DocumentElement;

            // Find the TSP service name
            XmlNode head = root.SelectSingleNode("head");
            name = GetTextValue(head, "");

            // Load the main application image and "detect" background image colour (always first pixel)
            string m_bg = Path.Combine(Path.GetDirectoryName(Config), GetAttribute(head, "bg", @""));
            if (!LoadImage(m_bg))
            {
                return false;
            }

            // Check and load the config SQLITE database
            string db_fname = Path.Combine(Path.GetDirectoryName(Config), GetAttribute(head, "db", @""));
            if (!LoadSQLiteDatabase(db_fname))
            {
                return false;
            }

            font_size = GetAttribute(head, "font_size", 12.0f);

            // Iterate the servers
            XmlNodeList servers = root.SelectNodes("servers/node");
            LogClients = new LogClients(servers.Count);
            foreach (XmlNode server in servers)
            {
                string server_type = GetAttribute(server, "class", "_err_");
                if (server_type == "logclient")
                {
                    LogClient lc = new LogClient()
                    {
                        Hw_name = GetAttribute(server, "hwId", "_err_"),
                        Enabled = GetAttribute(server, "Enabled", true),
                    };
                    lc.Hw_id = Db.GetId(lc.Hw_name);
                    if (lc.Hw_id < 0)
                    {
                        last_err = string.Format("Missing or invalid XML HwID for server node");
                        return false;
                    }

                    List<ChannelItem> channels = Db.m_channel_lookup.FindAll(x=>x.server_id == lc.Hw_id);
                    if (channels == null)
                    {
                        lc.channels = new List<int>(); // None
                    }
                    else
                    {
                        lc.channels = channels.Select(item => item.channel).ToList();
                    }
                    IPAddress ip = GetAttribute(server, "ip", new IPAddress(0));
                    int port = GetAttribute(server, "port", 57000);
                    if ((port <= 0) || (port >= 0xFFFF) || (ip == new IPAddress(0)))
                    {
                        last_err = string.Format("Missing or invalid IP/Port for server node: {0}", lc.Hw_id);
                        return false;
                    }
                    lc.Ep = new IPEndPoint(ip, port);
                    LogClients.Add(ref lc);
                } else {
                    last_err = String.Format("Missing or unsupported server node class: {0}", server_type);
                    return false;
                }
            }

            // Iterate the nodes
            XmlNodeList nodes = root.SelectNodes("nodes/node");
            Tsp_hw_nodes = new List<Tsp_hw_node>(nodes.Count);
            hw_list = new List<int>(nodes.Count);

            foreach (XmlNode node in nodes)
            {
                XmlNodeList Tsp_targets = node.SelectNodes("target");

                // Get button node
                XmlNode button = node.SelectSingleNode("button");
                Tsp_hw_node newButton = new Tsp_hw_node()
                {
                    text = GetTextValue(button, ""),
                    prompt = GetAttribute(node, "prompt", ""),
                    isSystem = false,
                    isActive = false,
                    id = -1,
                    img_main = null,
                    img_main_fname = "",

                    Enabled = GetAttribute(button, "Enabled", true),

                    rect = new Rectangle(
                        GetAttribute(button, "x", -1),
                        GetAttribute(button, "y", -1),
                        GetAttribute(button, "sx", -1),
                        GetAttribute(button, "sy", -1)
                    ),
                    targets = new List<Tsp_hw_target>(Tsp_targets.Count),
                };

                /* Add the targets */
                foreach (XmlNode target in Tsp_targets)
                {
                    XmlNodeList Tsp_target_match_list = target.SelectNodes("match");
                    Tsp_hw_target item = new Tsp_hw_target()
                    {
                        pos = GetAttribute(target, "pos", -1),
                        name = GetAttribute(target, "name", ""),
                        matchList = new List<Tsp_hw_target_match>(Tsp_target_match_list.Count),
                    };

                    foreach (XmlNode match in Tsp_target_match_list)
                    {
                        Tsp_hw_target_match item2 = new Tsp_hw_target_match()
                        {
                            channel = GetAttribute(match, "channel", -1),
                            hw_id = GetAttribute(match, "hwID", ""),
                            hw_src = GetAttribute(match, "src", ""),
                            type = 0,
                        };
                        if (item2.channel != -1) item2.type |= Tsp_hw_target_match_type.MatchChannel;
                        if (item2.hw_id.Length > 0) item2.type |= Tsp_hw_target_match_type.MatchHwID;
                        if (item2.hw_src.Length > 0) item2.type |= Tsp_hw_target_match_type.MatchSource;
                        item.matchList.Add(item2);

                    }
                    newButton.targets.Add(item);
                }

                string image;
                image = GetAttribute(button, "image", "");
                if (image != "")
                {
                    newButton.img_main_fname = Path.Combine(Path.GetDirectoryName(Config), Path.GetFileName(image));
                    newButton.img_main = new Bitmap(newButton.img_main_fname);
                }

                if ((newButton.rect.X < 0) || (newButton.rect.Y < 0))
                {
                    last_err = string.Format("Missing or invalid co-ordinate (x,y) for node {0}", newButton.text);
                    return false;
                }
                if ((newButton.rect.Width < 1) || (newButton.rect.Height < 1))
                {
                    last_err = string.Format("Missing or invalid size (sx/sy) for node {0}", newButton.text);
                    return false;
                }

                // Update context menus
                XmlNodeList menu_list = node.SelectNodes("menu");
                List<WebPage_action> items = new List<WebPage_action>(menu_list.Count);
                foreach (XmlNode menu in menu_list)
                {

                    WebPage_action action = new WebPage_action()
                    {
                        Enabled = GetAttribute(menu, "Enabled", true),
                        URL = GetAttribute(menu, "src", ""),
                        desc = GetTextValue(menu, ""),
                    };

                    items.Add(action);
                }
                newButton.actions = items; // .ToArray();

                if (node.Attributes["class"].Value.Equals("logClient", StringComparison.CurrentCultureIgnoreCase))
                {
                    newButton.isActive = true;
                    newButton.id = Db.GetId(GetAttribute(node, "hwId", "_err_"));
                    if (newButton.id < 0)
                    {
                        last_err = string.Format("Missing or invalid XML HwID for node: {0}", newButton.text);
                        return false;
                    }

#if false
                    if (hw_list.Contains(newButton.id))
                    {
                        last_err = string.Format("Duplicate HwID found for node: {0}", newButton.text);
                        return false;
                    }
#endif
                    hw_list.Add(newButton.id);
                    newButton.UserActions = new List<User_action>(0); // holding point
                }
                else if (node.Attributes["class"].Value.Equals("system", StringComparison.CurrentCultureIgnoreCase))
                {
                    newButton.isSystem = true; // This is the system button
                }
                else if (node.Attributes["class"].Value.Equals("user", StringComparison.CurrentCultureIgnoreCase))
                {
                    // Add user action
                    XmlNodeList action_list = node.SelectNodes("action");
                    List<User_action> user_items = new List<User_action>(action_list.Count);
                    foreach (XmlNode action in action_list)
                    {

                        User_action ua = new User_action()
                        {
                            Enabled = GetAttribute(action, "enabled", true),
                            raw_sql = GetTextValue(action, ""),
                        };
                        IPAddress ip = GetAttribute(action, "ip", new IPAddress(0));
                        int port = GetAttribute(action, "port", 57000);
                        if ((port < 0) || (port > 0xFFFF) || (ip == new IPAddress(0)))
                        {
                            last_err = string.Format("Missing or invalid IP/port for node: {0}", newButton.text);
                            return false;
                        }
                        ua.ep = new IPEndPoint(ip, port);
                        user_items.Add(ua);
                    }
                    newButton.UserActions = user_items; // .ToArray();
                }

                Tsp_hw_nodes.Add(newButton);
            }

            OnXmlChanged(EventArgs.Empty);
            isSaved = true; // We have just loaded
            return true;
        }

        public bool ValidateXml(out List<string> warnings)
        {
            warnings = new List<string>();
            MyClient client = MyClient.Instance;
            LogClient agent = new LogClient();

            // Validate all of the servers can be accessed
            foreach (LogClient server in this.LogClients)
            {
                if (!server.TestLogClient())
                {
                    warnings.Add(string.Format("Unable to contact log client server: {0}:{1}", server.Ep.Address.ToString(), server.Ep.Port));
                }
            }

            foreach (Hw_id hw_id in Db.m_hw_list)
            {
                if (!hw_list.Contains(hw_id.id)) {
                    warnings.Add(String.Format("XML config is missing SQLite node: {0}", hw_id.hw_name));
                }
            }

            // Validate that all of the Web pages can be downloaded
            client.HeadOnly = true;
            foreach (Tsp_hw_node hw in Tsp_hw_nodes)
            {

                // Check for invalid nodes
                if (hw.isActive && (hw.id < 0))
                {
                    warnings.Add(string.Format("Unable to find node {0} in the SQLite database", hw.text));
                }

                // Test all of the log client target buttons are valid
                if (hw.UserActions != null)
                {
                    foreach (User_action action in hw.UserActions)
                    {
                        agent.Ep = action.ep;
                        if (!agent.TestLogClient())
                        {
                            warnings.Add(string.Format("Unable to contact log client on button {0}: {1}:{2}", hw.text, agent.Ep.Address.ToString(), agent.Ep.Port));
                        }
                    }
                }
                

                // Test all of the URLs to make sure that they are valid
                foreach (WebPage_action URI in hw.actions)
                {
                    try
                    {
                        // fine if s1 gets the head (no content downloaded)
                        // else, throws 404 if the webpage is broken
                        string s1 = client.DownloadString(URI.URL);
                    }
                    catch (Exception)
                    {
                        warnings.Add(string.Format("Broken URL on button {0}: {1}", hw.text, URI.URL));
                    }
                }
            }

            return (warnings.Count == 0);
        }

        string GetMemoryStream(XmlNode node, string preset)
        {
            if (node.FirstChild == null) return preset;
            if (node.FirstChild.NodeType != XmlNodeType.Text) return preset;
            return node.FirstChild.Value;
        }

        string GetTextValue(XmlNode node, string preset)
        {
            if (node.FirstChild == null) return preset;
            if (node.FirstChild.NodeType != XmlNodeType.Text) return preset;
            return node.FirstChild.Value;
        }

        bool GetAttribute(XmlNode node, string name, bool preset)
        {
            string attrib = GetAttribute(node, name, preset.ToString());
            bool val;
            if (bool.TryParse(attrib, out val))
            {
                return val;
            }
            return preset;
        }

        int GetAttribute(XmlNode node, string name, int preset)
        {
            string attrib = GetAttribute(node, name, "_err_");
            if (attrib != "_err_")
            {
                int ret;
                if (int.TryParse(attrib, out ret))
                {
                    return ret;
                }
            }
            return preset;
        }

        float GetAttribute(XmlNode node, string name, float preset)
        {
            string attrib = GetAttribute(node, name, "_err_");
            if (attrib != "_err_")
            {
                float ret;
                if (float.TryParse(attrib, out ret))
                {
                    return ret;
                }
            }
            return preset;
        }

        string GetAttribute(XmlNode node, string name, string preset)
        {
            if (node.Attributes == null) return preset; // No attributes
            for (int i = 0; i<node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    return node.Attributes[i].Value;
                }
            }
            return preset; // Or name not found
        }

        IPAddress GetAttribute(XmlNode node, string name, IPAddress preset)
        {
            string attrib = GetAttribute(node, name, "_err_");
            if (attrib != "_err_")
            {
                IPAddress ret;
                if (IPAddress.TryParse(attrib, out ret))
                {
                    return ret;
                }
            }
            return preset;
        }
    }

    public class Hw_id
    {
        public int idx;
        public int id;
        public int hw_type;
        public string hw_name;
        public string location;
        public int isBackup; // -1 is N/A, otherwise 0 or 1
        public bool isRedundant;
        public Hw_id(string name) : this(-1)
        {
            hw_name = name;
        }
        public Hw_id(int pos)
        {
            idx = pos;
        }
        public int id_main; // Source if main
        public int id_backup; // Source if backup
        public List<int> channels; // Channel numbers (empty list if none applicable)
        public int parent_type; // Parent type (-1 if not available)
    };

    public class SqliteDatabaseInterface
    {
        public List<Event_item> m_events;
        public struct Event_item
        {
            public int event_id;
            public string name;
            public string desc;
            public string action;
        };
        public Dictionary<int, int> m_parent_lookup;

        string m_connection_string;
        public SqliteDatabaseInterface(string fname, int version = 3)
        {
            m_connection_string = String.Format("Data Source={0};Version={1};Read Only=True;FailIfMissing=True;", fname, version);

            // Test database connection
            SQLiteConnection con = new SQLiteConnection(m_connection_string);
            con.Open();
            con.Close();
        }
        public int GetId(string lookup)
        {
            lookup = lookup.ToUpper();
            foreach (Hw_id h in m_hw_list)
            {
                if (h.hw_name == lookup) return h.id;
            }

            // -1 is lookup failed
            return -1; // Error
        }

        public Dictionary<int, string> m_hw_types;
        public List<Hw_id> m_hw_list;

        public Dictionary<int, string> m_sw_types;
        public List<Sw_id> m_sw_list;
        public struct Sw_id
        {
            public int id;
            public int hw_id;
            public int sw_type;
        };
        public List<Tsp_node> m_tsp_list;
        public struct Tsp_node
        {
            public int id; // Hardware ID
            public bool isRedundant; // True or false
            public int isBackup; // -1 is N/A, else 0 or 1
            // public int channel;
            public List<Tsp_hw_item> hw_items;
            public List<Tsp_sw_item> sw_items;
        };
        public struct Tsp_hw_item
        {
            public int id; // Hardware ID
            public int channel;
        };
        public struct Tsp_sw_item
        {
            public int id; // Node ID
            public int sw_type; // SW type
        };

        public Hw_id GetHwId(int id)
        {
            foreach (Hw_id h in m_hw_list)
            {
                if (h.id == id) return h;
            }

            // null means lookup failed
            return null; // Error
        }
        public int GetTspHwItemIndex(int hw_id)
        {
            int idx = 0;
            foreach (SqliteDatabaseInterface.Tsp_node tsp_node in m_tsp_list)
            {
                if (tsp_node.id == hw_id)
                {
                    return idx;
                }
                idx++;
            }
            return -1; // Not found
        }

        public bool Load(out string last_err)
        {
            try
            {
                return ILoad(out last_err);
            }
            catch (Exception e)
            {
                last_err = e.Message;
            }
            return false;
        }

        // Load all of the config
        bool ILoad(out string last_err)
        {
            last_err = "";
            SQLiteConnection con = new SQLiteConnection(m_connection_string);
            con.Open();
            SQLiteDataReader reader;
            SQLiteCommand cmd;
            string sql;

            // Get a list of all the hardware types
            sql = "select HW_Type_ID, name from hw_type WHERE (HW_Type_ID > 0) ORDER BY hw_type_ID";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_hw_types = new Dictionary<int, string>();
            while (reader.Read())
            {
                m_hw_types.Add(reader.GetInt32(0), reader.GetString(1));
            }
            reader.Close();

            // Load all of the available hardware nodes from the database
            // Valid channel numbers are grouped together for each item
            // 
            sql = @"SELECT hl.hw_id, hl.HW_Type_ID, hl.name,
CASE count(DISTINCT hcl.backup)
     WHEN 0 THEN hl.backup
     WHEN 1 THEN hcl.backup
     ELSE null END as isBackup,
CASE
     WHEN hl.backup is null and hcl.backup is not null and cl.id_main is not null THEN 1
     ELSE 0 END as isRedundant,
ll.name,
CASE 
     WHEN hcl.channel > 0 THEN group_concat(DISTINCT cl.channel)
     END as channel_list,
 np.node_parent, cl.id_main, cl.id_backup
            FROM hw_list as hl
            LEFT JOIN location_list as ll on ll.Location_ID == hl.Location_ID
            LEFT JOIN hw_channel_list as hcl ON hl.hw_id == hcl.hw_id
            LEFT JOIN node_parent as np on hl.hw_type_id == np.node_type
            LEFT JOIN channel_list as cl ON cl.channel == hcl.channel
            LEFT JOIN channel_list as cl2 ON hl.hw_id == cl2.id_main or hl.hw_id == cl2.id_backup
            GROUP BY hl.hw_id
            ORDER BY hl.hw_id, hcl.channel, hcl.backup";
            // sql = "select HW_ID, HW_Type_ID, Name, Backup from hw_list ORDER BY HW_ID";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            int idx = 0;
            m_hw_list = new List<Hw_id>();
            while (reader.Read())
            {
                Hw_id h = new Hw_id(idx++)
                {
                    id = reader.GetInt32(0),
                    hw_type = reader.GetInt32(1),
                    hw_name = reader.GetString(2).ToUpper(),
                    isBackup = -1, // Not applicable
                    isRedundant = false,
                    id_backup = -1,
                    id_main = -1,
                    location = reader.GetString(5),
                    channels = new List<int>(),
                    parent_type = -1, // None
                };
                if (!reader.IsDBNull(3))
                {
                    h.isBackup = reader.GetInt32(3);
                }
                if (!reader.IsDBNull(4))
                {
                    h.isRedundant = reader.GetBoolean(4);
                }

                if (!reader.IsDBNull(6))
                {
                    string string_array = reader.GetString(6);
                    foreach (string item in string_array.Split(','))
                    {
                        h.channels.Add(int.Parse(item));
                    }

                }
                if (!reader.IsDBNull(7))
                {
                    h.parent_type = reader.GetInt32(7);
                }
                if (!reader.IsDBNull(8))
                {
                    h.id_main = reader.GetInt32(8);
                }
                if (!reader.IsDBNull(9))
                {
                    h.id_backup = reader.GetInt32(9);
                }
                m_hw_list.Add(h);
            }
            reader.Close();

            // Load all of the available hardware nodes from the database
            sql = @"SELECT hcl.hw_id, hcl.channel, hcl.backup, cl.id_main, cl.id_backup
                FROM hw_channel_list as hcl
                LEFT JOIN channel_list as cl ON cl.channel == hcl.channel 
                ORDER BY hcl.hw_id;";
            // sql = "select HW_ID, HW_Type_ID, Name, Backup from hw_list ORDER BY HW_ID";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: HW_CHANNEL_LIST";
                return false;
            }

            // Get a list of all the software types
            sql = "select SW_ID, long_name from sw_type ORDER BY SW_ID";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_sw_types = new Dictionary<int, string>();
            while (reader.Read())
            {
                m_sw_types.Add(reader.GetInt32(0), reader.GetString(1));
            }
            reader.Close();

            // Load all of the available software nodes from the database
            sql = @"select nl.node_list_ID, nl.HW_ID, nl.SW_ID from node_list as nl
                JOIN sw_type as swt on swt.SW_ID == nl.SW_ID
                where nl.SW_ID is not null ORDER BY nl.HW_ID,nl.SW_ID";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: SW_ID";
                return false;
            }
            m_sw_list = new List<Sw_id>();
            while (reader.Read())
            {
                Sw_id h = new Sw_id()
                {
                    id = reader.GetInt32(0),
                    hw_id = reader.GetInt32(1),
                    sw_type = reader.GetInt32(2)
                };
                m_sw_list.Add(h);
            }
            reader.Close();

            // Get a list of the possible servers for this system - will includes SWITCH and KMV hardware along with actual servers
            sql = @"SELECT hwl.hw_id, hwl.backup is not null, hwl.backup FROM hw_list AS hwl
JOIN (SELECT * FROM node_parent WHERE (node_parent IS NULL) OR (node_parent == (SELECT node_type FROM node_parent WHERE node_parent IS NULL))) AS np
ON np.node_type == hwl.hw_type_id";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_tsp_list = new List<Tsp_node>();
            while (reader.Read())
            {
                Tsp_node n = new Tsp_node()
                {
                    id = reader.GetInt32(0), // Top-level Hardward id
                    isRedundant = reader.GetBoolean(1), // Is redundant?
                    isBackup = -1, // Not applicable
                    // channel = -1, // Invalid
                };
                if (!reader.IsDBNull(2)) {
                    n.isBackup = reader.GetInt32(2); // Is backup?
                }

                string sql2 = string.Format(@"select nl.node_list_id, nl.sw_id
                    from node_list as nl
                    JOIN sw_type as sw ON sw.SW_ID == nl.SW_ID
                    WHERE nl.HW_ID == {0} and nl.SW_ID is not null order by nl.SW_ID", n.id);
                SQLiteCommand cmd2 = new SQLiteCommand(sql2, con);
                SQLiteDataReader reader2 = cmd2.ExecuteReader();
                n.sw_items = new List<Tsp_sw_item>();
                while (reader2.Read())
                {
                    Tsp_sw_item s = new Tsp_sw_item()
                    {
                        id = reader2.GetInt32(0),
                        sw_type = reader2.GetInt32(1),
                    };
                    n.sw_items.Add(s);
                }
                reader2.Close();

                if (n.isBackup == 1)
                {
                    sql2 = string.Format(@"SELECT hl.HW_ID, cl.channel FROM hw_list AS hl JOIN hw_channel_list AS hcl ON hcl.hw_id == hl.hw_id JOIN channel_list AS cl ON cl.channel == hcl.channel WHERE hcl.backup == 1 AND cl.id_backup == {0} ORDER BY hl.HW_ID", n.id);
                }
                else /* if (n.isBackup == 0) */
                {
                    sql2 = string.Format(@"SELECT hl.HW_ID, cl.channel FROM hw_list AS hl JOIN hw_channel_list AS hcl ON hcl.hw_id == hl.hw_id JOIN channel_list AS cl ON cl.channel == hcl.channel WHERE hcl.backup != 1 AND cl.id_main == {0} ORDER BY hl.HW_ID", n.id);
                }
                cmd2 = new SQLiteCommand(sql2, con);
                reader2 = cmd2.ExecuteReader();
                n.hw_items = new List<Tsp_hw_item>();
                while (reader2.Read())
                {
                    Tsp_hw_item h = new Tsp_hw_item()
                    {
                        id = reader2.GetInt32(0),
                        //isBackup = n.isBackup,
                        // isRedundant = n.isRedundant,
                        channel = reader2.GetInt32(1),
                    };
                    // m_tsp_list.Add(h);
                    n.hw_items.Add(h);
                }
                reader2.Close();
                m_tsp_list.Add(n);
            }
            reader.Close();


            // Get a list of all the events
            sql = "SELECT id, short_name, description, action FROM event_list";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_events = new List<Event_item>();
            while (reader.Read())
            {
                Event_item item = new Event_item() {
                    event_id = reader.GetInt32(0),
                    name = reader.GetString(1),
                    desc = (!reader.IsDBNull(2)) ? reader.GetString(2) : "",
                    action = (!reader.IsDBNull(3)) ? reader.GetString(3) : "",
                };
                m_events.Add(item);
            }
            reader.Close();

            // Get a list of all the events
            sql = "SELECT node_type, node_parent FROM node_parent";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_parent_lookup = new Dictionary<int, int>();
            while (reader.Read())
            {
                if (!reader.IsDBNull(1))
                {
                    m_parent_lookup[reader.GetInt32(0)] = reader.GetInt32(1);
                }
                else
                {
                    m_parent_lookup[reader.GetInt32(0)] = -1; // End of the list
                }
            }
            reader.Close();

            // Get a list of all channels, and their associated H/W type
            sql = @"SELECT cl1.id_main, cl1.id_backup, hcl2.channel, hcl2.name as hw_type 
            FROM (SELECT hcl.channel as channel, hwt.hw_type_id as name
            FROM hw_channel_list as hcl
            LEFT JOIN hw_list as hw on hw.hw_id == hcl.hw_id 
            LEFT JOIN hw_type as hwt on hw.hw_type_id == hwt.hw_type_id 
            ORDER BY hcl.hw_id) as hcl2
            LEFT JOIN channel_list as cl1 on cl1.channel = hcl2.channel
            GROUP BY hcl2.channel";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_channel_lookup = new List<ChannelItem>();
            while (reader.Read())
            {
                ChannelItem main = new ChannelItem() {
                    server_id = reader.GetInt32(0),
                    channel = reader.GetInt32(2),
                    hw_type = reader.GetInt32(3),
                };
                m_channel_lookup.Add(main);
                ChannelItem backup = new ChannelItem()
                {
                    server_id = reader.GetInt32(1),
                    channel = reader.GetInt32(2),
                    hw_type = reader.GetInt32(3),
                };
                m_channel_lookup.Add(backup);
            }
            reader.Close();

            // Get a list of all channels, and their associated H/W type
            sql = "SELECT ID,Name FROM enum_eventlevel";
            cmd = new SQLiteCommand(sql, con);
            reader = cmd.ExecuteReader();
            if (!reader.HasRows)
            {
                last_err = "Empty table: " + sql;
                return false;
            }
            m_event_enum = new Dictionary<int, string>();
            while (reader.Read())
            {
                m_event_enum.Add(reader.GetInt32(0), reader.GetString(1));
            }
            reader.Close();

            con.Close();
            return true;
        }
        public List<ChannelItem> m_channel_lookup;
        public Dictionary<int, string> m_event_enum;
    }

    public class LogClient
    {
        public bool Enabled
        {
            get;
            set;
        }
        public int Hw_id
        {
            get;
            set;
        }
        public string Hw_name;
        public List<int> channels; // List of valid channels for this logClient
        public IPEndPoint Ep
        {
            get;
            set;
        }

        public bool Connected
            {
            get;
            set;
            } = true;


        int m_timeout;
        public LogClient(int timeout = 10)
        {
            m_timeout = timeout * 1000;
        }

        public List<LogClient.LogSummary> base_array;

         
        public List<LogClient.LogSummary> LatestEvents;                     //CRR added for Issue #40
        public ClassSNMPManager SNMPManager = new ClassSNMPManager();       //CRR added for Issue #40
      



        // int level, int count, string msg
        public struct LogSummary
        {
            public int guid; // Unique index number for event
            public int hw_id;
            public int level;
            public int count;
            public DateTime tstamp;
            public int channel;
            public int eventID;
            public int targetID;
            public string msg;
            public int sw_type;
            public int maintenance;         //CRR # 48 Maintenance mode now also passed through

            public DateTime LastUpdated;    //CRR #40 also record when the event was fetched from the log


            // int.Parse(sc[0])].Update(int.Parse(sc[1]), int.Parse(sc[2]), sc[3].TrimStart('\"').TrimEnd('\"')
        }

        public struct LogSummaryPlusMaintenance                 //#48
            {
            public int guid; // Unique index number for event
            public int hw_id;
            public int level;
            public int count;
            public DateTime tstamp;
            public int channel;
            public int eventID;
            public int targetID;
            public string msg;
            public int sw_type;
            public int maintenance;         //Maintenance mode now also passed through

            public DateTime LastUpdated;    //CRR #40 also record when the event was fetched from the log


            // int.Parse(sc[0])].Update(int.Parse(sc[1]), int.Parse(sc[2]), sc[3].TrimStart('\"').TrimEnd('\"')
            }


        string m_last_err = "";
        public string Last_err
        {
            get { return m_last_err; }
        }

     


        public bool TestLogClient()
        {
            if (!Enabled) return true;

            string reply;
            bool valid = Execute("SELECT * FROM dbname.sqlite_master WHERE type='table';", out reply);
            return valid;
        }

        public async void CheckLogClient()
            {
            Task<int> task = new Task<int>(CheckLogClientAsync);
            task.Start();
            int retval = await task;
            }

        public int CheckLogClientAsync()
            {


            return 1;
            }

        /*
         * CRR - ISSUE #9 Whereby disconnecting a NAS causes the UI to hang.
         * The 'fix' below is to place the connect into an indepdendent task that is run asynchronously.
         * This frees up the UI to continue running.
         */


        public bool Execute(string cmd, out string response)
        {
            
            //Debug.WriteLine("");
            //Debug.WriteLine("|||||||||| Execute START " + this.Ep.Address + " " + cmd.Replace("\r\n", "") + " " + DateTime.Now.ToString("h:mm:ss fff tt"));
            try
            {
                String Execute_Return = "";
                //Debug.WriteLine(this.Ep.Address + " Execute SQL: " + cmd.Replace("\r\n", "") + " " + DateTime.Now.ToString("h:mm:ss fff tt"));

                Execute_Return = IExecute(cmd, "");
                response = Execute_Return;
                if(Execute_Return=="")
                    {
                    //Debug.WriteLine("X|||||||||| Execute FAILED" + cmd.Replace("\r\n", ""));
                    //Debug.WriteLine("");
                    return false;
                    }
                //Debug.WriteLine("|||||||||| Execute Return OK: " + cmd.Replace("\r\n", "") + " Response: " + response);
                //Debug.WriteLine("");
                return true;
            }
            catch (Exception er)
            {
                m_last_err = er.Message;
                //Debug.WriteLine("X|||||||||| Execute  Exception: " + cmd.Replace("\r\n", "") + " " + er.Message);
                //Debug.WriteLine("");
                }

            response = "";
            return false;
        }

    


        //CRR  Non Task based IExecute starts - which hangs
        //CRR Removed out string response
        string IExecute(string sql, string response)
            {
            Debug.WriteLine("IExecute started for " + Ep.Address + " SQL=" + sql.Replace("\r\n", ""));
            response = "";
            try
                {
                if (!Enabled)
                    {
                    return "";                  //CRR Empty response = nothing or error
                    }
                if(!Connected)
                    {
                    Debug.WriteLine("IExecute " + Ep.Address + " - NOT CONNECTED - IGNORE");
                    return "";
                    }
                TcpClient tcp = new TcpClient();
                tcp.SendTimeout = m_timeout;
                tcp.ReceiveTimeout = m_timeout;
                tcp.Connect(Ep);

                Byte[] send = Encoding.ASCII.GetBytes("{\"_def\" : \"sql\", \"_reply\" : true, \"execute\" : \"" + sql.Replace("\r\n", "") + "\"}\x04");
                tcp.Client.Send(send);

                Byte[] recv = new Byte[1 * 1024 * 1024];
                tcp.Client.Receive(recv);
                string reply = Encoding.ASCII.GetString(recv).TrimEnd();

                // Get reply string from json
                Regex regex = new Regex("(\"_reply\": )(.*)(, \"_replyError\")");
                Match match = regex.Match(reply);
                response = match.Groups[2].Value;

                tcp.Close();
                Debug.WriteLine("IExecute finished " + Ep.Address + " response=" + response.Replace("\r\n", ""));
                return response;
                }
            catch (Exception Ex)
                {
                Debug.WriteLine("XXXX EXCEPTION IExecute  " + Ep.Address + " EXCEPTION: Could not connect to " + Ep.Address + " " + Ex.Message);
                }
            Connected = false;
            Debug.WriteLine("IExecute  " + Ep.Address + " Mark as unconnected ");
            return response;
            }


        public bool AcknowledgeByGuid(int guid)
        {
            return Acknowledge(string.Format("id == {0}", guid));
        }
        public bool AcknowledgeByLevel(int level, bool invert = false)
        {
            XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
            int max_level = xml.Db.m_event_enum.Keys.Max();
            int min_level = xml.Db.m_event_enum.Keys.Min();
            if (level == max_level)
            {
                if (!invert)
                {
                    return Acknowledge(string.Format("(level >= {1}) or (level < {0})", min_level, max_level));
                }
                return Acknowledge(string.Format("(level < {1}) and (level >= {0})", min_level, max_level));
            }
            return Acknowledge(string.Format("level {1} {0}", level, invert ? "!=" : "=="));
        }

        public bool AcknowledgeByTstamp(DateTime tstamp, bool newerThan = false)
        {
            return Acknowledge(string.Format("tstamp {1} {0}", tstamp, newerThan ? ">" : "<" ));
        }
        public bool AcknowledgeByChannel(int channel, bool invert = false)
        {
            return Acknowledge(string.Format("channel {1} {0}", channel, invert ? "!=" : "==" ));
        }
        public bool AcknowledgeByEventId(int event_id, bool invert = false)
        {
            return Acknowledge(string.Format("eventId {1} {0}", event_id, invert ? "!=" : "=="));
        }
        public bool AcknowledgeByTarget(int target_id, bool invert = false)
        {
            return Acknowledge(string.Format("targetId {1} {0}", target_id, invert ? "!=" : "=="));
        }
        public bool AcknowledgeBySource(int source_id, bool invert = false)
        {
            return Acknowledge(string.Format("nodeId {1} {0}", source_id, invert ? "!=" : "=="));
        }

        public delegate void EventAcknowledgedHandler(object sender, EventArgs e);
        public event EventAcknowledgedHandler Acknowledged;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnAcknowledged(EventArgs e)
        {
            if (Acknowledged != null)
                Acknowledged.Invoke(this, e);
        }

        bool Acknowledge(string where_clause)
        {
            if (!Enabled) return true;

            string reply;
            bool valid = Execute(string.Format("UPDATE eventLog SET acknowledged=1 WHERE {0}", where_clause), out reply);
            if (valid) OnAcknowledged(EventArgs.Empty);
            return valid;
        }

        public bool SetMaintenanceEvents(IEnumerable<MaintenanceEntry> entry)
        {
            bool ret = true;
            List<MaintenanceEntry> db = GetMaintenanceEvents(false).ToList();

            // Read the remote database, and perform any updates that have occurred
            foreach (var item in entry)
            {
                // Skip any entries that do not belong to this LogClient instance
                if (item.server_id != Hw_id)
                {
                    continue;
                }

                // Find the matching entry
                // db.Find
                var match = db.Find(x => x.channel == item.channel);
                if (match == null)
                {
                    // Error!!
                    this.m_last_err = "Error! Unable to update maintenance log";
                    continue;
                }
                
                // Only make bare minimum of changes to the server as this data may be shared
                // with other software e.g. Event Viewer or other TSP workstations
                if ((!match.active.Equals(item.active) || (match.Desc != item.Desc)) ||
                    (!match.StartDate.Equals(item.StartDate) || !match.EndDate.Equals(item.EndDate)))
                {
                    string reply;
                    string sql;
                    // Use updates to avoid creating a new entry - latest admin tools must be used
                    //sql = String.Format("UPDATE maintenanceLog SET active={1}, active_from={2}, active_to={3}, description={4} WHERE {0}",
                    //    ((item.channel < 0) ? "channel is null" : String.Format("channel={0}",item.channel.ToString())),
                    //    item.active ? 1 : 0, (item.StartDate > DateTime.MinValue) ? String.Format("'{0}'", item.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ")) : "null",
                    //    item.EndDate > DateTime.MinValue ? String.Format("'{0}'", item.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ")) : "null",
                    //    (item.Desc == null) ? "null" : String.Format("'{0}'", item.Desc));
                    //CRR 18/06/2019 - ISSUE2 - Remove Z to see if it stops logClient crashing - Nope
                    //CRR 19/06/2019 - ISSUE2 - Remove T to see if it stops logClient crashing
                    Debug.WriteLine("################## #23 Updating the maintenance mode for " + item.channel.ToString() + " to " + item.active);
                    sql = String.Format("UPDATE maintenanceLog SET active={1}, active_from={2}, active_to={3}, description={4} WHERE {0}",
                        ((item.channel < 0) ? "channel is null" : String.Format("channel={0}", item.channel.ToString())),
                        item.active ? 1 : 0, (item.StartDate > DateTime.MinValue) ? String.Format("'{0}'", item.StartDate.ToString("yyyy-MM-dd HH:mm:ss")) : "null",
                        item.EndDate > DateTime.MinValue ? String.Format("'{0}'", item.EndDate.ToString("yyyy-MM-dd HH:mm:ss")) : "null",
                        (item.Desc == null) ? "null" : String.Format("'{0}'", item.Desc));

                    Debug.WriteLine("MAINT SQL: {0}", sql);

                    if (!Execute(sql, out reply)) ret = false; // Failed to get a reply..

                    //CRR 20/09/2019  Issue #23  - Clear current alarms on a channel if it's maintenance mode is being set to true

                    if(item.active)
                        {
                        string ClearAlarmSQL;
                        string ClearAlarmReply;
                        if (item.channel < 0)
                            {
                            ClearAlarmSQL = String.Format("UPDATE eventLog SET acknowledged=1");        // Acknowledge all Alarms on the NAS
                            }
                        else
                            {
                            ClearAlarmSQL = String.Format("UPDATE eventLog SET acknowledged=1 WHERE channel = {0}", item.channel.ToString()); // Acknowledge alarms for the specific channel
                            }
                        Debug.WriteLine("################## MAINT MODE SO CLEAR ALARM: " + ClearAlarmSQL);
                        if (!Execute(ClearAlarmSQL, out ClearAlarmReply))
                            ret = false; // Failed to get a reply..
                        }
                    }
            }

            /*
            // Perform delete function first
            {
                string reply;
                string sql = @"DELETE FROM maintenanceLog;";
                if (!Execute(sql, out reply)) ret = false; // Failed to get a reply..
            }

            foreach (MaintenanceEntry item in entry)
            {
                string reply;
                string sql = String.Format("INSERT INTO maintenanceLog (active, channel, active_from, active_to, description) VALUES ({0}, {1}, DATETIME('{2}', 'localtime'), DATETIME('{3}', 'utc'), '{4}')", item.active ? 1 : 0, ((item.channel < 0) ? "0" : item.channel.ToString()), item.StartDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), item.EndDate.ToString("yyyy-MM-ddTHH:mm:ssZ"), item.Desc);
                if (!Execute(sql, out reply)) ret = false; // Failed to get a reply..
            }
             */

            return ret;
        }

        // Return a static list to avoid select statements making dynamic calls via a list or similar
        public MaintenanceEntry[] GetMaintenanceEvents(bool limit_to_active = false)
        {
            try
            {
                return IGetMaintenanceEvents(limit_to_active).ToArray();
            }
            catch (Exception er)
            {
                m_last_err = er.Message;
            }
            return new MaintenanceEntry[0]; // Return an empty list
        }

        public IEnumerable<MaintenanceEntry> IGetMaintenanceEvents(bool limit_to_active)
        {
            string reply;
            // string sql = @"SELECT active, channel, active_from, active_to, description FROM maintenanceLog WHERE active_from < active_to ORDER BY channel, active_to DESC";
            string sql = @"SELECT active, channel, active_from, active_to, description FROM maintenanceLog";
            if (limit_to_active)
            {
                sql += " WHERE active AND (active_from < DATETIME('now', 'localtime') AND active_to > DATETIME('now', 'localtime'))";
            }
            if (!Execute(sql, out reply)) yield break; // Failed to get a reply..

            // Get array items from json
            string[] mc = reply.TrimStart('[').TrimEnd(']').Split(new string[] { "], [" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string array2 in mc)
            {
                // string array2 = m.Groups[1].Value;
                string[] sc = array2.Split(new string[] { ", " }, 5, StringSplitOptions.RemoveEmptyEntries);

                // Update each of the status in turn
                //CRR 11/08/19 Generates an exception if sc[0] is null
                //CRR 11/08/19 Deleted
                //MaintenanceEntry me = new MaintenanceEntry(Hw_id, Hw_name)
                //{
                //    //server_id = Hw_id,
                //    //server_name = this.Hw_name,
                //    active = (int.Parse(sc[0]) > 0),
                //    //channel = -1,
                //    //StartDate = new DateTime(), // Null
                //    //EndDate = new DateTime(), // Null
                //    //Desc = sc[4].TrimStart('\"').TrimEnd('\"'),
                //};
                //CRR 11/08/19 - Inserted 
                MaintenanceEntry me = new MaintenanceEntry(Hw_id, Hw_name);
                //CRR 11/08/19 if sc[0] is NULL then active = false;
                if (sc[0].Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    me.active = false;
                    }
                else
                    {
                    me.active = (int.Parse(sc[0]) > 0);
                    }
                //CRR End
                if (!sc[1].Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    int channel = int.Parse(sc[1]);
                    if (!this.channels.Contains(channel))
                    {
                        this.m_last_err = String.Format("Invalid channel ({0}) found in maintenance log!", channel);
                        continue; // Ignore as invalid database
                    }
                    me.channel = channel;
                }
                if (!sc[2].Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    me.StartDate = DateTime.ParseExact(sc[2].Replace("\"", ""), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                }
                if (!sc[3].Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    me.EndDate = DateTime.ParseExact(sc[3].Replace("\"", ""), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
                }
                if (!sc[4].Equals("null", StringComparison.InvariantCultureIgnoreCase))
                {
                    me.Desc = sc[4].TrimStart('\"').TrimEnd('\"');
                }
                yield return me;
            }
        }

        public bool GetLogSummary(out List<LogSummary> logs)
        {
            try
            {
                bool ret = IGetLogSummary(out logs);
                return ret;
            }
            catch (Exception er)
            {
                m_last_err = er.Message;
            }

            logs = null;
            return false;
        }

        public bool CleanUpSummary(ref List<LogSummary> logs, out List<string> warnings)
        {
            warnings = new List<string>();

            XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
            int min_level = xml.Db.m_event_enum.Keys.Min();
            int max_level = xml.Db.m_event_enum.Keys.Max();

            for (int idx = logs.Count -1 ; idx >= 0; idx--)
            {
                LogClient.LogSummary item = logs[idx];
                int hw_id = item.targetID;
                int level = item.level;
                int sw_type = item.sw_type;
                SqliteDatabaseInterface.Sw_id sw_id2 = xml.LookupNodeId(hw_id);

                if ((level > max_level) || (level < min_level))
                {
                    warnings.Add(string.Format("Node {0} with level {1} outside of range {2} to {3}. Using level {3} ({4}) instead.", item.hw_id, item.level, min_level, max_level, xml.Db.m_event_enum[max_level]));
                    level = max_level;
                }

                if (base_array.Exists(x => ((x.hw_id == hw_id) && (x.sw_type == sw_type) && (x.level == level) && (x.channel == item.channel)))) continue; // Node found - all match
                if (base_array.Exists(x => ((x.hw_id == hw_id) && (x.level == level) && (x.channel == item.channel)))) continue; // Node found - all except sw_type match
                if (base_array.Exists(x => ((x.hw_id == hw_id) && (x.sw_type == sw_type)))) continue; // Node found - all except channel match

                int channel = item.channel;
                // Node was not found, so let's try to find it's parent
                while (!base_array.Exists(x => ((x.hw_id == hw_id) && (x.sw_type == sw_id2.sw_type) && (x.level == level) && (x.channel == channel))))
                {
                    SqliteDatabaseInterface.Sw_id sw_id = xml.LookupNodeId(hw_id);
                    if (sw_id.hw_id == -1)
                    {
                        // Error, node was not found in node list - default to the calling object
                        hw_id = Hw_id;
                        sw_type = sw_id.sw_type;
                        channel = -1;
                        break;
                    }

                    Hw_id hw_node = xml.Db.m_hw_list.Find(x => x.id == sw_id.hw_id);
                    // SW_ID was found
                    int hw_type = hw_node.hw_type;
                    while (xml.Db.m_parent_lookup.ContainsKey(hw_type))
                    {
                        hw_type = xml.Db.m_parent_lookup[hw_type];
                        if (hw_type < 0)
                        {
                            hw_id = Hw_id;
                            sw_type = 0; // clear sw type
                            channel = -1;
                            break;
                        }
                        List<Hw_id> parents = xml.Db.m_hw_list.FindAll(x => x.hw_type == hw_type);
                        if (parents.Count == 0) { continue; }
                        else if (parents.Count == 1)
                        {
                            hw_id = parents[0].id;
                            continue;
                        }
                        else if (channel < 0) { continue; }
                        else if (!parents.Exists(x => x.channels.Exists(y => y == channel))) continue;
                        else
                        {
                            hw_id = parents.Find(x => x.channels.Exists(y => y == channel)).id;
                            break;
                        }
                    }
                }

                // warnings.Add(string.Format("Node {0} event was unexpectedly received on server {1}. Using node {2} instead.", item.hw_id, xml.GetNodeName(Hw_id), hw_id));

                // We may have found a better match
                // Replace the list item with a clone
                LogClient.LogSummary clone = new LogClient.LogSummary()
                {
                    hw_id = hw_id, // Always use the HW ID from this node
                    level = level,
                    count = item.count,
                    msg = item.msg,
                    tstamp = item.tstamp,
                    channel = channel,
                    eventID = item.eventID,
                    guid = item.guid,
                    targetID = hw_id,
                    sw_type = sw_type,
                };
                int idx2 = logs.FindIndex(x => (x.targetID == clone.targetID) && (x.sw_type == clone.sw_type)  && (x.level == clone.level) && (x.channel == clone.channel));
                if (idx2 < 0)
                {
                    logs[idx] = clone; // Keep the clone node
                }
                else
                {
                    clone.count += logs[idx2].count;
                    if (clone.tstamp <= logs[idx2].tstamp) // Is the clone older?
                    {
                        // If so, then replace the clone with the existing node contents
                        clone.eventID = logs[idx2].eventID;
                        clone.msg = logs[idx2].msg;
                        clone.tstamp = logs[idx2].tstamp;
                        clone.guid = logs[idx2].guid;
                    }
                    logs[idx2] = clone; // Update the main node
                    logs.RemoveAt(idx); // Delete the clone node
                }
            }

            return (warnings.Count > 0);
        }


        bool IGetLogSummary(out List<LogSummary> logs)
        {
            XmlDatabaseInterface xml = XmlDatabaseInterface.Instance;
            
            bool ret = false;
            logs = null;

            string reply;
            string sql = @"SELECT el.hw_id, el.level, count(*) as count, el.tstamp, el.channel, el.eventID, el.id, el.desc
        FROM eventLog as el 
        WHERE el.[acknowledged]=0 AND el.[maintenance]=0 AND level IS NOT NULL
        GROUP BY channel, hw_id, level
        ORDER BY channel, hw_id, level, id DESC";
            if (!Execute(sql, out reply)) return false; //CRR Changed to call Execute -  Failed to get a reply..

            // Get array items from json
            //Regex getArray = new Regex("\\[+([^\\]]+)\\]");
            //MatchCollection mc = getArray.Matches(reply);
            string[] mc = reply.TrimStart('[').TrimEnd(']', '\"').Split(new string [] { "\"], [" }, StringSplitOptions.RemoveEmptyEntries);
            logs = new List<LogSummary>(mc.Length); //.Count);
            foreach (string array2 in mc)
            {
                // string array2 = m.Groups[1].Value;
                string[] sc = array2.Split(new string[] { ", " }, 8, StringSplitOptions.RemoveEmptyEntries);

                // Update each of the status in turn
                try
                {
                    LogSummary ls = new LogSummary()
                    {
                        hw_id = int.Parse(sc[0]), // Node ID
                        level = int.Parse(sc[1]),
                        count = int.Parse(sc[2]),
                        tstamp = DateTime.ParseExact(sc[3].Replace("\"", ""), "yyyy-MM-ddTHH:mm:ssZ", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                        channel = ((sc[4] != "null") ? int.Parse(sc[4]) : -1), // sc[4]
                        eventID = int.Parse(sc[5]),
                        //targetID = int.Parse(sc[6]), // Target ID
                        guid = int.Parse(sc[6]),
                        // sw_type = int.Parse(sc[8]),
                        msg = sc[7].TrimStart('\"').TrimEnd('\"'), // Always put this message last
                    };
                    ls.targetID = ls.hw_id; // Force these to be the same
                    // if (sc[4] != "null") ls.channel = int.Parse(sc[4]);
                    logs.Add(ls);
                }
                catch (Exception e)
                {
                    string msg  = e.Message;
                }
            }
            ret = true;

            return ret;
        }

    }

    public class LogClients : IEnumerable
    {

        List<LogClient> m_targets;
        public IEnumerator GetEnumerator()
        {
            foreach (LogClient lc in m_targets)
                yield return lc;
        }

        public LogClients(int count = 0)
        {
            m_targets = new List<LogClient>(count);
        }

        public List<LogClient> AsList()
        {
            return m_targets;
        }

        public void Add(ref LogClient client)
        {
            m_targets.Add(client);
        }
        public void RemoveAt(int index)
        {
            m_targets.RemoveAt(index);
        }
        public LogClient this[int Index]
        {
            get
            {
                return m_targets[Index];
            }
            set
            {
                m_targets[Index] = value;
            }
        }

        // Search for id in hw-list
        public bool HasId(int id)
        {
            foreach (LogClient lc in m_targets)
            {
                if (lc.Hw_id == id) return true;
            }
            return false;
        }
    }

    public class LogMessage : object
    {
        public enum LogMessageTypes
        {
            Other = -1,
            Info = 0,
            Warning,
            Error,
        }

        public DateTime tstamp;
        public LogMessageTypes type;
        public string msg;
    }

    public delegate void LogChangedEventHandler(object sender, LogMessage e);

    public class LogInterface: Object
    {
        private string install_base;
        public string Install_base
        {
            get { return install_base; }
            set { install_base = value; }                   //CRR Issue #41
        }
        private static LogInterface instance;

        public static LogInterface Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new LogInterface();
                }
                return instance;
            }
        }

        // An event that clients can use to be notified whenever the
        // elements of the list change
        public event LogChangedEventHandler Changed;

        // Invoke the Changed event; called whenever list changes
        protected virtual void OnChanged(LogMessage e)
        {
            if (Changed != null)
                Changed.Invoke(this, e);
        }

        private IEnumerable<LogMessage> getLogs(DateTime tstamp)
        {
            List<LogMessage> logs = new List<LogMessage>();
            string folder = Path.Combine(install_base, "logs", tstamp.ToString("yyyy-MM-dd"));
            if (Directory.Exists(folder))
            {
                string[] files = Directory.GetFiles(folder, "*.txt");
                foreach (string file in files)
                {
                    StreamReader reader = new StreamReader(file);
                    while (!reader.EndOfStream)
                    {
                        string txt = reader.ReadLine();
                        Regex regex = new Regex("([0-9]+:[0-9]+:[0-9]+)[ -]+(Warning!|Error!|Info!)[ ](.*)", RegexOptions.IgnoreCase);
                        Match me = regex.Match(txt);
                        if (me.Success)
                        {
                            LogMessage msg = new LogMessage()
                            {
                                tstamp = tstamp.Date + TimeSpan.ParseExact(me.Groups[1].Value, "c", CultureInfo.InvariantCulture),
                                type = (LogMessage.LogMessageTypes)Enum.Parse(typeof(LogMessage.LogMessageTypes), me.Groups[2].Value.Replace("!", ""), true),
                                msg = me.Groups[3].Value,
                            };
                            yield return msg;
                        }
                    }
                    reader.Close();
                }
            }

        }

        public IEnumerable<LogMessage> GetAllLogs(DateTime tstamp)
        {
            if (tstamp.Date == DateTime.Now.Date) //CRR issue #18 Use just local time
            {
                return m_logs.AsEnumerable().Reverse();
            }
            return getLogs(tstamp).Reverse();
        }

        List<LogMessage> m_logs;

        private LogInterface()
        {
            install_base = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), @"Thruput\TSP");
            m_lastdate = DateTime.Now; //CRR issue #18 Use just local time

            // At start up, retrieve all of today's logs
            m_logs = new List<LogMessage>();
            m_logs.AddRange(getLogs(m_lastdate));

        }

        public void LogError(string msg)
        {
            LogMessage lm = new LogMessage()
            {
                msg = msg,
                tstamp = DateTime.Now, //CRR issue #18 Use just local time
                type = LogMessage.LogMessageTypes.Error,
            };
            AddLogMessage(ref lm);
        }

        public void LogWarn(string msg)
        {
            LogMessage lm = new LogMessage()
            {
                msg = msg,
                tstamp = DateTime.Now,  //CRR issue #18 Use just local time
                type = LogMessage.LogMessageTypes.Warning,
            };
            AddLogMessage(ref lm);
        }
        public void LogInfo(string msg)
        {
            LogMessage lm = new LogMessage()
            {
                msg = msg,
                tstamp = DateTime.Now, //CRR issue #18 Use just local time
                type = LogMessage.LogMessageTypes.Info,
            };
            AddLogMessage(ref lm);
        }

        DateTime m_lastdate;
        public void AddLogMessage(ref LogMessage lm)
        {

            if (m_lastdate.Date != lm.tstamp.Date)
            {
                m_logs.Clear();
            }
            m_logs.Add(lm);
            OnChanged(lm); // Call event handler

            // DateTime tstamp = DateTime.UtcNow;
            DirectoryInfo dir = Directory.CreateDirectory(Path.Combine(install_base, "logs", lm.tstamp.ToString("yyyy-MM-dd")));
            string fname = lm.tstamp.ToString("yyyyMMdd") + "_" + lm.tstamp.ToString("HH") + ".txt";
            StreamWriter logger = new StreamWriter(Path.Combine(dir.FullName, fname), true);
            logger.WriteLine(lm.tstamp.ToString("HH:mm:ss") + " - " + lm.type.ToString() + "! " + lm.msg);
            logger.Close();
#if DEBUG
            Console.Out.WriteLine(lm.msg);
#endif
        }
    }
    class MyClient : WebClient
    {
        private static MyClient instance;

        public static MyClient Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MyClient();
                }
                return instance;
            }
        }

        private MyClient() : base()
        {
        }

        public bool HeadOnly { get; set; }
        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest req = base.GetWebRequest(address);
            req.Timeout = 3 * 1000; // 3 seconds max
            if (HeadOnly && req.Method == "GET")
            {
                req.Method = "HEAD";
            }
            return req;
        }
    }

    public class MaintenanceEntry: Object
    {
        public bool active; // Is entry activated
        public int server_id; // Hardware ID for the server
        public string server_name; // Human readable name for the server id
        public int channel; // Channel number for the event (-1 == all channels)
        public DateTime StartDate; // Start date for the event
        public DateTime EndDate; // End date for the event
        public string Desc; // Description

        public MaintenanceEntry(int id, string name)
            : this(id, name, -1)
        {
        }

        public MaintenanceEntry(int id, string name, int channel)
            : base()
        {
            active = false;
            server_id = id;
            server_name = name;
            this.channel = channel;
            StartDate = new DateTime();
            EndDate = new DateTime();
            //Desc = null;
        }

        public MaintenanceEntry(MaintenanceEntry clone)
            : this(clone.server_id, clone.server_name, clone.channel)
        {
            active = clone.active;
            //server_id = clone.server_id;
            //server_name = clone.server_name;
            //channel = clone.channel;
            StartDate = clone.StartDate;
            EndDate = clone.EndDate;
            Desc = clone.Desc;
        }

        public override string ToString()
        {
            DateTime now = DateTime.Now;         //CRR issue #18 Use just local time
            string name = string.Format("{0} ({1})", server_name, (channel > 0) ? "CH" + channel.ToString() : "ALL");
            string dates;
            int days = (int)Math.Ceiling((EndDate.Date - StartDate.Date).TotalDays);
            if (EndDate.Date < now.Date)
            {
                dates = string.Format("EXPIRED {0}", EndDate.ToShortDateString());
            }
            else if (days > 1)
            {
                dates = string.Format("{0} TO {1} ({2} day{3})", StartDate.ToShortDateString(), EndDate.ToShortDateString(), days, (days > 1) ? "s" : "");
            }
            else // if (days == 1)
            {
                dates = string.Format("{0}", EndDate.ToShortDateString());
            }

            string desc = Desc;
            if ((desc != null) && (desc.Length > 32))
            {
                desc = desc.Substring(0, 32) + '\u2026';
            }
            return string.Format("{0} - {1} - {2}", name, dates, desc);
        }
    }

    public struct ChannelItem
    {
        public int server_id; // server HW_ID
        public int channel; // Channel number 1-999
        public int hw_type; // 300 = FRC, 550 = RADAR, 600 = GUARDREC, etc
    }

}
