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
using Memenim.Core;
using AnonymDesktopClient.Core;

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

        public static readonly RoutedEvent OnPostClicked = EventManager.RegisterRoutedEvent("OnPostClick", RoutingStrategy.Direct, typeof(RoutedEventHandler), typeof(PostWidget));

        // expose our event
        public event RoutedEventHandler PostClick
        {
            add { AddHandler(OnPostClicked, value); }
            remove { RemoveHandler(OnPostClicked, value); }
        }


        public PostWidget()
        {
            InitializeComponent();
        }

        private void ViewPost_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RoutedEventArgs newEventArgs = new RoutedEventArgs(OnPostClicked);
            RaiseEvent(newEventArgs);
        }

        private void CopyPostID_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            DataContext = CurrentPostData;

            wdgPoster.PosterName = CurrentPostData.owner_name;
            wdgPoster.PostTime = Utils.UnixTimeStampToDateTime(CurrentPostData.date).ToString();

            stLikes.StatValue = CurrentPostData.likes.count.ToString();
            stDislikes.StatValue = CurrentPostData.dislikes.count.ToString();
            stComments.StatValue = CurrentPostData.comments.count.ToString();
            stShare.StatValue = CurrentPostData.reposts.ToString();
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostAPI.LikePost(CurrentPostData.id, AppPersistent.UserToken);
            if (res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            await UpdatePostStats();
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            var res = await PostAPI.DislikePost(CurrentPostData.id, AppPersistent.UserToken);
            if(res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }
            await UpdatePostStats();
        }

        async Task UpdatePostStats()
        {
            var res = await PostAPI.GetPostById(CurrentPostData.id, AppPersistent.UserToken);

            if(res.error)
            {
                DialogManager.ShowDialog("F U C K", res.message);
                return;
            }

            CurrentPostData = res.data[0];

            stLikes.StatValue = CurrentPostData.likes.count.ToString();
            stDislikes.StatValue = CurrentPostData.dislikes.count.ToString();
            stComments.StatValue = CurrentPostData.comments.count.ToString();
            stShare.StatValue = CurrentPostData.reposts.ToString();

        }

    }
}