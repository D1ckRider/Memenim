﻿using AnonymDesktopClient.Pages;
using System;
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
using Memenim.Core.Data;

namespace AnonymDesktopClient.Widgets
{
    /// <summary>
    /// Interaction logic for PostWidget.xaml
    /// </summary>
    public partial class PostWidget : UserControl
    {
        public string ImageURL { get; set; }
        public string PostText { get; set; }
        public string PostComments { get; set; }
        public string PostShares { get; set; }
        public string PostLikes { get; set; }
        public string PostDislikes { get; set; }
        public PostData CurrentPostData { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void ViewPost_Click(object sender, RoutedEventArgs e)
        {
            GeneralBlackboard.SetValue(BlackBoardValues.EPostData, CurrentPostData.id);
            GeneralBlackboard.SetValue(BlackBoardValues.EBackPage, new PostsPage());
            PageNavigationManager.SwitchToSubpage(new PostPage());
        }

        private void CopyPostID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            stLikes.StatValue = CurrentPostData.likes.count.ToString();
            stDislikes.StatValue = CurrentPostData.dislikes.count.ToString();
            stComments.StatValue = CurrentPostData.comments.count.ToString();
            stShare.StatValue = CurrentPostData.reposts.ToString();
        }
    }
}
