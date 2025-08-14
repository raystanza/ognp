using System;
using System.Windows.Forms;

namespace ognp
{
    internal static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();
            string? path = args.Length > 0 ? args[0] : null; // open file from cmdline if provided
            Application.Run(new MainForm(path));
        }
    }
}
