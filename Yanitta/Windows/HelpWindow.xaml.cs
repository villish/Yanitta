using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Yanitta.JSON;

namespace Yanitta.Windows
{
    /// <summary>
    /// Логика взаимодействия для HelpWindow.xaml
    /// </summary>
    public partial class HelpWindow : Window
    {
        public HelpWindow()
        {
            InitializeComponent();
        }

        T GetJSONObject<T>(string url) where T : class
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            T result = default(T);

            try
            {
                var request = WebRequest.Create(url) as HttpWebRequest;
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var bytes = reader.ReadToEnd();
                        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(bytes)))
                        {
                            var serializer = new DataContractJsonSerializer(typeof(T));
                            return serializer.ReadObject(stream) as T;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
            return result;
        }

        public void GetSpellData(uint spellId)
        {
            this.spellId.Text = spellId.ToString();

            var url = $"https://eu.api.battle.net/wow/spell/{spellId}?locale=ru_RU&apikey=ggj4gnyuywzcsdnehuznf6bjdvhfwfue";
            var spell = GetJSONObject<Spell>(url);

            if (spell == null)
            {
                Close();
                return;
            }

            spellName.Text          = spell.Name;
            spellRange.Text         = string.IsNullOrWhiteSpace(spell.Range) ? "" : "Радиус действия: " + spell.Range;
            spellCost.Text          = spell.PowerCost;
            spellCastTime.Text      = spell.CastTime;
            spellDescription.Text   = spell.Description;

            spellIcon.Source = new BitmapImage(
                new Uri($"http://media.blizzard.com/wow/icons/56/{spell.Icon}.jpg",
                    UriKind.Absolute));
        }

        void CommandBinding_GetSpellData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            uint id;
            if (uint.TryParse(spellId.Text.Trim(), out id))
                GetSpellData(id);
        }
    }
}
