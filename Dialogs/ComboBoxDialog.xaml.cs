﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;

namespace Memenim.Dialogs
{
    public partial class ComboBoxDialog : CustomDialog
    {
        public static readonly DependencyProperty DialogTitleProperty =
            DependencyProperty.Register(nameof(DialogTitle), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty DialogMessageProperty =
            DependencyProperty.Register(nameof(DialogMessage), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty ValuesProperty =
            DependencyProperty.Register(nameof(Values), typeof(ReadOnlyCollection<string>), typeof(ComboBoxDialog),
                new PropertyMetadata(new ReadOnlyCollection<string>(new List<string>())));
        public static readonly DependencyProperty SelectedValueProperty =
            DependencyProperty.Register(nameof(SelectedValue), typeof(string), typeof(ComboBoxDialog),
                new PropertyMetadata(string.Empty));

        public string DialogTitle
        {
            get
            {
                return (string)GetValue(DialogTitleProperty);
            }
            set
            {
                SetValue(DialogTitleProperty, value);
            }
        }
        public string DialogMessage
        {
            get
            {
                return (string)GetValue(DialogMessageProperty);
            }
            set
            {
                SetValue(DialogMessageProperty, value);
            }
        }
        public ReadOnlyCollection<string> Values
        {
            get
            {
                return (ReadOnlyCollection<string>)GetValue(ValuesProperty);
            }
            set
            {
                SetValue(ValuesProperty, value);
            }
        }
        public string SelectedValue
        {
            get
            {
                return (string)GetValue(SelectedValueProperty);
            }
            set
            {
                SetValue(SelectedValueProperty, value);
            }
        }
        public string DefaultValue { get; }

        public ComboBoxDialog(string title = "Enter", string message = "Enter",
            ReadOnlyCollection<string> values = null, string selectedValue = null,
            string defaultValue = null)
        {
            InitializeComponent();
            DataContext = this;

            DialogTitle = title;
            DialogMessage = message;
            Values = values;
            DefaultValue = defaultValue;

            if (selectedValue != null)
                lstValues.SelectedItem = selectedValue;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = (string)lstValues.SelectedItem;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            SelectedValue = DefaultValue;
            MainWindow.Instance.HideMetroDialogAsync(this, DialogSettings);
        }
    }
}