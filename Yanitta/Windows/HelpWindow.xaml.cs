using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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

        public void GetSpellData(uint spellId)
        {
            this.spellId.Text = spellId.ToString();

            var url = string.Format("https://eu.api.battle.net/wow/spell/{0}?locale=ru_RU&apikey=ggj4gnyuywzcsdnehuznf6bjdvhfwfue", spellId);
            var spell = Extensions.GetJSONObject<Spell>(url);

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
                new Uri(string.Format("http://media.blizzard.com/wow/icons/56/{0}.jpg", spell.Icon),
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
