/**************************************************************************************************
 
	Simple Debug Class


	Copyright (C) 2019-2020 Thruput Ltd
    All rights reserved
	
	Filename:		ClassDebug.cs
	Project:		TSP
	Developer:		CRR 
	Date:			2020-02-19
	Contact:		support@thruput.co.uk
	Notes:			Simple Debug class to be extended in the future

**************************************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
/// <summary>
/// Simple Debug class - to be extended
/// </summary>
namespace TechnicalSupervisor
    {
    public class ClassDebug
        {
        public ClassDebug()
            {
            }

        public static void DebugPrintLine(string s)
            {
            Debug.WriteLine(s);
            }
        }
    }

