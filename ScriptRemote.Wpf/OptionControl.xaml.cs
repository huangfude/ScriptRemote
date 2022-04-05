using ScriptRemote.Core.Common;
using ScriptRemote.Core.Utils;
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

namespace ScriptRemote.Wpf
{
    /// <summary>
    /// Interaction logic for OptionControl.xaml
    /// </summary>
    public partial class OptionControl : UserControl
    {

        public OptionControl()
        {
            InitializeComponent();
            DataContext = this;

            string theme = ConfigUtil.FindByName(CommonConst.ThemeName).Value;
            if ("Light.Blue".Equals(theme))
            {
                lightBtn.IsChecked = true;
            }
            else
            {
                darkBtn.IsChecked = true;
            }
        }

        private void lightBtn_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.ChangeTheme("Light.Blue");
        }

        private void darkBtn_Checked(object sender, RoutedEventArgs e)
        {
            App.Current.ChangeTheme("Dark.Blue");
        }
    }
}
