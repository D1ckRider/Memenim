﻿using System;
using System.Security.Cryptography;
using System.Windows;
using Memenim.Pages;
using Memenim.Settings;
using MahApps.Metro.Controls;
using RIS.Extensions;
using Memenim.Managers;

namespace Memenim
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow CurrentMainWindow;

        public MainWindow()
        {
            InitializeComponent();

            CurrentMainWindow = this;

            Loaded += (s, e) => 
            {
                rootLayout.Children.Add(NavigationController.Instance);
            };

            SettingManager.MainWindow = this;

            Width = SettingManager.AppSettings.WindowWidth;
            Height = SettingManager.AppSettings.WindowHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SettingManager.AppSettings.WindowPositionX - (Width / 2.0);
            Top = SettingManager.AppSettings.WindowPositionY - (Height / 2.0);
            WindowState = (WindowState)SettingManager.AppSettings.WindowState;

            DialogManager.WindowRef = this;

            LocalizationManager.MainWindow = this;
            LocalizationManager.SwitchLanguage(SettingManager.AppSettings.Language);

            try
            {
                string userToken = AppPersistent.GetFromStore("UserToken");
                string userId = AppPersistent.GetFromStore("UserId");

                if (userToken != null && userId != null)
                {
                    AppPersistent.UserToken = AppPersistent.WinUnprotect(userToken, "UserToken");
                    AppPersistent.LocalUserId = AppPersistent.WinUnprotect(userId, "UserId").ToInt();

                    NavigationController.Instance.RequestPage<FeedPage>();
                }
                else
                {
                    NavigationController.Instance.RequestPage<LoginPage>();
                }
            }
            catch (CryptographicException)
            {
                NavigationController.Instance.RequestPage<LoginPage>();
            }
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            SettingManager.AppSettings.WindowWidth = Width;
            SettingManager.AppSettings.WindowHeight = Height;
            SettingManager.AppSettings.WindowPositionX = Left + (Width / 2.0);
            SettingManager.AppSettings.WindowPositionY = Top + (Height / 2.0);
            SettingManager.AppSettings.WindowState = (int)WindowState;
            SettingManager.AppSettings.Save();
        }
    }
}
