using System.Windows.Input;

namespace Yanitta
{
    public class YanittaCommand : RoutedUICommand
    {
        private static RoutedUICommand NewCommand(string name, params InputGesture[] gestures)
        {
            return new RoutedUICommand(name, name, typeof(YanittaCommand),
                new InputGestureCollection(gestures));
        }

        public static RoutedUICommand OpenSettings      { get; private set; }
        public static RoutedUICommand OpenCodeExec      { get; private set; }
        public static RoutedUICommand OpenProfiles      { get; private set; }
        public static RoutedUICommand OpenPluginSetting { get; private set; }
        public static RoutedUICommand SaveProfile       { get; private set; }

        public static RoutedUICommand AddAbility        { get; private set; }
        public static RoutedUICommand CopyAbility       { get; private set; }
        public static RoutedUICommand DeleteAbility     { get; private set; }
        public static RoutedUICommand SaveAbilityLua    { get; private set; }
        public static RoutedUICommand LoadAbilityList   { get; private set; }

        public static RoutedUICommand AddRotation       { get; private set; }
        public static RoutedUICommand CopyRotation      { get; private set; }
        public static RoutedUICommand DeleteRotation    { get; private set; }
        public static RoutedUICommand RefreshRotation   { get; private set; }

        public static RoutedUICommand Import            { get; private set; }
        public static RoutedUICommand Export            { get; private set; }

        public static RoutedUICommand Update            { get; private set; }
        public static RoutedUICommand Reload            { get; private set; }

        public static RoutedUICommand Run               { get; private set; }

        static YanittaCommand()
        {
            OpenSettings        = NewCommand("OpenSettings");
            OpenCodeExec        = NewCommand("OpenCodeExec");
            OpenProfiles        = NewCommand("OpenProfiles");
            OpenPluginSetting   = NewCommand("OpenPluginSetting");
            SaveProfile         = NewCommand("SaveProfile");

            AddAbility          = NewCommand("AddAbility");
            CopyAbility         = NewCommand("CopyAbility");
            DeleteAbility       = NewCommand("DeleteAbility");
            SaveAbilityLua      = NewCommand("SaveAbilityLua");

            AddRotation         = NewCommand("AddRotation");
            CopyRotation        = NewCommand("CopyRotation");
            DeleteRotation      = NewCommand("DeleteRotation");
            RefreshRotation     = NewCommand("RefreshRotation");
            LoadAbilityList     = NewCommand("LoadAbilityList");

            Import              = NewCommand("Import");
            Export              = NewCommand("Export");

            Update              = NewCommand("Update");
            Reload              = NewCommand("Reload");

            Run                 = NewCommand("Run", new KeyGesture(Key.F5));
        }
    }
}