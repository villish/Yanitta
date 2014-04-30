using System;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using Yanitta.Properties;

namespace Yanitta
{
    public partial class App : Application
    {
        public static ProcessList ProcessList { get; set; }

        static App()
        {
            ConsoleWriter.Initialize("Yanitta.log", true);
            ProcessList = new ProcessList();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            if (e.Args.Length > 0 && e.Args[0] == "/e")
                StartupUri = new Uri("Windows/WinProfileEditor.xaml", UriKind.Relative);
            else if (e.Args.Length > 0 && e.Args[0] == "/ex")
                StartupUri = new Uri("Windows/WinCodeExecute.xaml", UriKind.Relative);

            if (Settings.Default.Language != "auto")
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(Settings.Default.Language);

            var fileName = Settings.Default.ProfilesFileName;
            Console.WriteLine("Yanitta startup!...");

            try
            {
                ProfileDb.Instance = XmlManager.Load<ProfileDb>(fileName);

                //Если загрузка прошла удачно - сделаем резервную копию.
                if (File.Exists(fileName))
                    File.Copy(fileName, fileName + ".bak", true);

                Console.WriteLine("База успешно загружена, резервная копия создана.");
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine("Ошибка загрузки базы данных, возможно база повреждена.");
                Console.WriteLine(ex.Message);
                // Попытка загрузить резервную базу.
                if (File.Exists(fileName + ".bak"))
                {
                    Console.WriteLine("Найден файл резервной копии...");
                    File.Copy(fileName + ".bak", fileName, true);
                }
                else
                    throw new FileNotFoundException("Файл резервной копии не найден.", fileName + ".bak");

                try
                {
                    Console.WriteLine("Попытка загрузить базу данных из резервной копии.");
                    ProfileDb.Instance = XmlManager.Load<ProfileDb>(fileName);
                    Console.WriteLine("База из резервной копии успешно загружена.");
                }
                catch (Exception ex_inner)
                {
                    throw new YanittaException("Ошибка загрузки базы из резервной копии.\r\n" + ex_inner.Message);
                }
            }

            base.OnStartup(e);
        }

        protected override void OnExit(ExitEventArgs e)
        {
            ProfileDb.Save();
            Console.WriteLine("Yanitta stoped ... !");
            ConsoleWriter.CloseWriter();
            base.OnExit(e);
        }
    }
}