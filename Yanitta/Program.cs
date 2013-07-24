using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Yanitta
{
    public static class Program
    {
        const string FASMDLL_MANAGED = "FasmDllManaged.dll";
        public const int ROTATION_COUNT = 4;

        public static List<Profile> AllProfiles;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            if (!File.Exists(FASMDLL_MANAGED))
            {
                MessageBox.Show("File " + FASMDLL_MANAGED + " not found!");
                return;
            }

            AllProfiles  = new List<Profile>();
            
            Console.SetOut(RichTextBoxWriter.Instance);

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler((o, e) => Console.WriteLine(e.ExceptionObject));

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
        }
    }
}