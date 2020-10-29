﻿using System;
using System.Windows;
using System.Windows.Controls;
using MahApps.Metro.IconPacks;

namespace Memenim.Widgets
{
    public partial class StatButton : UserControl
    {
        public static readonly RoutedEvent OnButtonClicked = EventManager.RegisterRoutedEvent("ButtonClick", RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(StatButton));
        public static readonly DependencyProperty StatValueProperty =
            DependencyProperty.Register("StatValue", typeof(string), typeof(StatButton), new PropertyMetadata("0"));
        public static readonly DependencyProperty ButtonSizeProperty =
            DependencyProperty.Register("ButtonSize", typeof(int), typeof(StatButton), new PropertyMetadata(48));

        public event EventHandler<RoutedEventArgs> ButtonClick
        {
            add
            {
                AddHandler(OnButtonClicked, value);
            }
            remove
            {
                RemoveHandler(OnButtonClicked, value);
            }
        }

        public string StatValue
        {
            get
            {
                return (string)GetValue(StatValueProperty);
            }
            set
            {
                SetValue(StatValueProperty, value);
            }
        }
        public int ButtonSize
        {
            get
            {
                return (int)GetValue(ButtonSizeProperty);
            }
            set
            {
                SetValue(ButtonSizeProperty, value);
            }
        }
        public PackIconModernKind IconKind { get; set; } = PackIconModernKind.Xbox;

        public StatButton()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnButtonClicked));
        }
    }
}
