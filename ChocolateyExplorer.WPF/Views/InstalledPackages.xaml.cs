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
using ChocolateyExplorer.WPF.ViewModel;

namespace ChocolateyExplorer.WPF.Views
{
    /// <summary>
    /// Interaction logic for InstalledPackages.xaml
    /// </summary>
    public partial class InstalledPackages : UserControl
    {
        public InstalledPackages()
        {
            InitializeComponent();

            this.Loaded += async (a, b) => await (this.DataContext as InstalledPackagesViewModel).RefreshPackages();
        }
    }
}
