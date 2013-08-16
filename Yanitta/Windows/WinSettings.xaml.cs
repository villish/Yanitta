using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using Yanitta.Properties;

namespace Yanitta
{
    /// <summary>
    /// Логика взаимодействия для WindowSettings.xaml
    /// </summary>
    public partial class WindowSettings : Window
    {
        public WindowSettings()
        {
            InitializeComponent();

            this.CommandBindings.AddRange(new CommandBinding[] {
                new CommandBinding(ApplicationCommands.Save,  (o, e) => {
                    Offsets.Default.Save();
                    this.Close();
                    e.Handled = true;
                }),
                new CommandBinding(ApplicationCommands.Close, (o, e) => {
                    this.Close();
                    e.Handled = true;
                }),
                new CommandBinding(ApplicationCommands.Open,  (o, e) => {
                    var dialog = new OpenFileDialog() {
                        FileName = Settings.Default.ProfilesFileName,
                        Filter   = Localization.ProfileFileMask
                    };
                    if (dialog.ShowDialog() == true)
                        Settings.Default.ProfilesFileName = dialog.FileName;

                    e.Handled = true;
                }),
                new CommandBinding(ApplicationCommands.Find, (o, e) => {
                    Cursor = Cursors.Wait;
                    try
                    {
                        this.LoadOffsets();
                    }
                    catch(Exception ex)
                    {
                        MessageBox.Show(ex.Message,
                            "Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error
                            );
                    }
                    Cursor = Cursors.Arrow;
                })
            });

            this.InputBindings.AddRange(new InputBinding[] {
                new KeyBinding(ApplicationCommands.Save,  Key.S,  ModifierKeys.Control),
                new KeyBinding(ApplicationCommands.Close, Key.F4, ModifierKeys.Alt),
                new KeyBinding(ApplicationCommands.Open,  Key.F3, ModifierKeys.None),
            });
        }

        private void LoadOffsets()
        {
            var address = string.Empty;

            using (var response = (HttpWebResponse)WebRequest.Create(Settings.Default.UpdateOffsetURL + "/offsets.txt").GetResponse())
            {
                using (var reader = new StreamReader(response.GetResponseStream()))
                {
                    var content = reader.ReadToEnd();
                    address = content.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault();
                }
            }

            if (string.IsNullOrWhiteSpace(address))
                throw new Exception("Нет данных для загрузки");

            Func<XmlReader, int> read = (reader) =>
            {
                var str = reader.ReadString();
                if (string.IsNullOrWhiteSpace(str))
                    return 0;

                int result = 0;
                if (str.StartsWith("0x"))
                    int.TryParse(str.Substring(2), System.Globalization.NumberStyles.AllowHexSpecifier, null, out result);
                else
                    int.TryParse(str, out result);

                return result;
            };

            var request = WebRequest.Create(Settings.Default.UpdateOffsetURL + '/' + address);

            using (var response = (HttpWebResponse)request.GetResponse())
            {
                using (var reader = XmlReader.Create(response.GetResponseStream()))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "CurrentWoWVersion":           Offsets.Default.Build                           = read(reader); break;
                                case "PlayerName":                  Offsets.Default.PlayerName                      = read(reader); break;
                                case "PlayerClass":                 Offsets.Default.PlayerClass                     = read(reader); break;
                                case "GameState":                   Offsets.Default.IsInGame                        = read(reader); break;
                                case "Lua_DoStringAddress":         Offsets.Default.FrameScript_ExecuteBuffer       = read(reader); break;
                                case "Lua_GetLocalizedTextAddress": Offsets.Default.FrameScript_GetLocalizedText    = read(reader); break;
                                default: reader.Read(); break;
                            }
                        }
                    }
                }
            }
        }
    }
}