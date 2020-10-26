﻿using System;
using System.Security.Cryptography;
using System.Windows;
using Memenim.Pages;
using Memenim.Settings;
using MahApps.Metro.Controls;
using Memenim.Localization;
using Memenim.Utils;
using RIS.Extensions;

namespace Memenim
{
    public partial class MainWindow : MetroWindow
    {
        public static MainWindow CurrentInstance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            CurrentInstance = this;

            Width = SettingManager.AppSettings.WindowWidth;
            Height = SettingManager.AppSettings.WindowHeight;
            WindowStartupLocation = WindowStartupLocation.Manual;
            Left = SettingManager.AppSettings.WindowPositionX - (Width / 2.0);
            Top = SettingManager.AppSettings.WindowPositionY - (Height / 2.0);
            WindowState = (WindowState)SettingManager.AppSettings.WindowState;

            PageNavigationManager.PageContentControl = contentArea;

            LocalizationManager.SwitchLanguage(SettingManager.AppSettings.Language).Wait();

            try
            {
                SettingManager.PersistentSettings.CurrentUserLogin =
                    SettingManager.PersistentSettings.GetCurrentUserLogin();

                if (string.IsNullOrEmpty(SettingManager.PersistentSettings.CurrentUserLogin))
                {
                    PageNavigationManager.SwitchToPage(new LoginPage());
                    return;
                }

                string userToken = SettingManager.PersistentSettings.GetUserToken(
                    SettingManager.PersistentSettings.CurrentUserLogin);
                string userId = SettingManager.PersistentSettings.GetUserId(
                    SettingManager.PersistentSettings.CurrentUserLogin);

                if (!string.IsNullOrEmpty(userToken) && !string.IsNullOrEmpty(userId))
                {
                    SettingManager.PersistentSettings.CurrentUserToken =
                        PersistentUtils.WinUnprotect(userToken, $"UserToken-{SettingManager.PersistentSettings.CurrentUserLogin}");
                    SettingManager.PersistentSettings.CurrentUserId =
                        PersistentUtils.WinUnprotect(userId, $"UserId-{SettingManager.PersistentSettings.CurrentUserLogin}").ToInt();

                    PageNavigationManager.SwitchToPage(new ApplicationPage());
                }
                else
                {
                    PageNavigationManager.SwitchToPage(new LoginPage());
                }
            }
            catch (CryptographicException)
            {
                PageNavigationManager.SwitchToPage(new LoginPage());
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
