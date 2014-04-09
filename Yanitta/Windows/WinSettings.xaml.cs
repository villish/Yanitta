﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Input;
using System.Xml.Serialization;
using Microsoft.Win32;
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
                    Settings.Default.Save();
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
                        using (var response = WebRequest.Create(Settings.Default.UpdateOffset).GetResponse())
                        {
                            var serializer = new XmlSerializer(typeof(Offsets));
                            using (var stream = response.GetResponseStream())
                            {
                                var offsets = (Offsets)serializer.Deserialize(response.GetResponseStream());
                                Extensions.CopyProperies(offsets, Offsets.Default);
                            }
                        }
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
        }
    }
}