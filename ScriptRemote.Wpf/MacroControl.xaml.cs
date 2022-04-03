﻿using System;
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
    /// Interaction logic for MacroControl.xaml
    /// </summary>
    public partial class MacroControl : UserControl
    {
        public MacroControl()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void macroMinus_Click(object sender, RoutedEventArgs e)
        {
            macroGrid.Children.Clear();
        }
    }
}
