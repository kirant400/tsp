using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("TSP Config Utility")]
[assembly: AssemblyDescription(@"TSP Configuration Utility

This is a tool for generating Technical Supervisor config files, or updating existing configuration files.

TSP configuration files will contain an embedded copy of the database and bitmap files used to create the configuration. Existing config file can be refreshed with new bitmaps or configuration databases by loading a new file at the prompt.

The XML file can then be loaded onto a customer site by using the -i flag in TSP. This will force TSP to load a new conguration file on start up.")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("Thruput Ltd")]
[assembly: AssemblyProduct("TSP Config Utility")]
[assembly: AssemblyCopyright("Copyright © 2016-19")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("32c14415-b7bd-4266-999f-a559ad6424ac")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.7.0.*")]
// [assembly: AssemblyFileVersion("1.0.0.0")]
