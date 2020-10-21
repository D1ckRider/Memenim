﻿using System;
using System.Security.Cryptography;
using System.Windows;
using Memenim.Pages;
using Memenim.Settings;
using MahApps.Metro.Controls;
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
                string userToken = AppPersistent.GetFromStore("UserToken");
                string userId = AppPersistent.GetFromStore("UserId");

                if (userToken != null && userId != null)
                {
                    AppPersistent.UserToken = AppPersistent.WinUnprotect(userToken, "UserToken");
                    AppPersistent.LocalUserId = AppPersistent.WinUnprotect(userId, "UserId").ToInt();

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
