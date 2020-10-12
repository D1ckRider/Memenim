﻿using System.Windows;
using System.Windows.Controls;
using Memenim.Core;
using Memenim.Core.Data;

namespace AnonymDesktopClient.Core.Pages
{
    /// <summary>
    /// Interaction logic for AccountViewPage.xaml
    /// </summary>
    public partial class UserProfilePage : UserControl
    {
        public int UserID { get; set; }

        private ProfileData m_UserInfo { get; set; }

        public UserProfilePage()
        {
            InitializeComponent();
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var res = await UsersAPI.GetUserProfileByID(UserID);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            m_UserInfo = res.data[0];
            DataContext = m_UserInfo;
        }
    }
}
