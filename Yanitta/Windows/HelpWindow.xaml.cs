using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Yanitta.Windows
{
    [DataContract]
    public class Spell
    {
        [DataMember(Name = "id")]
        public uint Id            { get; set; }

        [DataMember(Name = "name")]
        public string Name        { get; set; }

        [DataMember(Name = "icon")]
        public string Icon        { get; set; }

        [DataMember(Name = "description")]
        public string Description { get; set; }

        [DataMember(Name = "range")]
        public string Range       { get; set; }

        [DataMember(Name = "powerCost")]
        public string PowerCost   { get; set; }

        [DataMember(Name = "castTime")]
        public string CastTime    { get; set; }

        public string IconSource => $"http://media.blizzard.com/wow/icons/56/{Icon}.jpg";
    }

    public partial class HelpWindow : Window
    {
        uint spellId;
        public HelpWindow()
        {
            InitializeComponent();
        }

        Spell GetSpellData(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentNullException(nameof(url));

            Spell result = new Spell();

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
                            var serializer = new DataContractJsonSerializer(typeof(Spell));
                            return serializer.ReadObject(stream) as Spell;
                        }
                    }
                }
            }
            catch (WebException wex)
                when (wex.Status == WebExceptionStatus.ProtocolError)
            {
                result.Id = spellId;
                result.Description = wex.Message;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return result;
        }

        public void GetSpellData(uint spellId)
        {
            this.spellId = spellId;

            var url = $"https://eu.api.battle.net/wow/spell/{spellId}?locale=ru_RU&apikey=ggj4gnyuywzcsdnehuznf6bjdvhfwfue";
            DataContext = GetSpellData(url);
        }

        void CommandBinding_GetSpellData_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            GetSpellData(spellId);
        }
    }
}
