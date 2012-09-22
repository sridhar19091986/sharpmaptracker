using System;
using System.Collections.Generic;
using System.Linq;
using SharpTibiaProxy.Domain;
using System.Diagnostics;
using System.Text;
using SharpTibiaProxy.Memory;
using System.Windows.Forms;

namespace SharpMapTracker
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
