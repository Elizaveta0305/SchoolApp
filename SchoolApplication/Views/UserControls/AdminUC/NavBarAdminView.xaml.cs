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

namespace SchoolApplication.Views.UserControls.AdminUC
{
    /// <summary>
    /// Логика взаимодействия для NavBarAdminView.xaml
    /// </summary>
    public partial class NavBarAdminView : UserControl
    {
        public NavBarAdminView()
        {
            InitializeComponent();
        }

        private void ExitApp(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}
