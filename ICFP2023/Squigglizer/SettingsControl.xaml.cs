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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ICFP2023
{
    public partial class SettingsControl : UserControl
    {
        public SquigglizerSettings Settings
        {
            get => settings;

            set
            {
                settings = value;

                StringBuilder sb = new();
                foreach (var setting in settings.AllSettings)
                {
                    sb.Append($"{setting.Name}: {settings[setting]}\n");
                }

                Box.Text = sb.ToString();
            }
        }

        private SquigglizerSettings settings;

        public SettingsControl()
        {
            InitializeComponent();
        }
    }
}
