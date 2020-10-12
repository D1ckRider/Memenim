﻿using System;

namespace AnonymDesktopClient.Core.Settings
{
    public static class SettingManager
    {
        public static MainWindow MainWindow;

        public static AppSettings AppSettings { get; }

        static SettingManager()
        {
            AppSettings = new AppSettings();
        }
    }
}
