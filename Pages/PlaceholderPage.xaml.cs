﻿using System;
using System.Windows;
using System.Windows.Controls;

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for PlaceholderPage.xaml
    /// </summary>
    public partial class PlaceholderPage : UserControl
    {
        private string[] SmilesPool = new string[]
        {
            "(ﾟдﾟ；)",
            "(ó﹏ò｡)",
            "(´ω｀*)",
            "(┛ಠДಠ)┛彡┻━┻",
            "(* _ω_)…"
        };

        public PlaceholderPage()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            Random rnd = new Random();
            int idx = rnd.Next(0, SmilesPool.Length - 1);
            txtSmile.Text = SmilesPool[idx];
        }
    }
}
