﻿using AnonymDesktopClient.DataStructs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
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

namespace AnonymDesktopClient
{
    /// <summary>
    /// Interaction logic for Posts.xaml
    /// </summary>
    public partial class PostPage : UserControl
    {
        public PostPage()
        {
            InitializeComponent();
        }

        private async void PostPage_Loaded(object sender, RoutedEventArgs e)
        {
            PostData post = GeneralBlackboard.TryGetValue<PostData>(BlackBoardValues.EPostData);
            if(post != null)
            {
                txtPost.Text = post.text;
                imgPost.Source = new BitmapImage(new Uri(post.attachments[0].photo.photo_medium, UriKind.Absolute));
                lblPosterName.Content = post.owner_name;
                lblPosterName.IsEnabled = post.hidden == 0 ? true : false;
                lblDate.Content = new DateTime(post.date).ToLocalTime().ToString();
                var commentsData = await ApiHelper.GetCommentsForPost(post.id);
                lstComments.Items.Clear();
                for (int i = commentsData.Count - 1; i > 0; --i)
                {
                    UserComment commentWidget = new UserComment();
                    commentWidget.UserName = commentsData[i].user.name;
                    commentWidget.Comment = commentsData[i].text;
                    commentWidget.ImageURL = commentsData[i].user.photo;
                    commentWidget.UserID = commentsData[i].user.id;
                    lstComments.Items.Add(commentWidget);
                }
            }
        }

        private void CopyUserId_Click(object sender, RoutedEventArgs e)
        {
            UserComment comment = (UserComment)lstComments.SelectedItem;
            Clipboard.SetText(comment.UserID.ToString());
        }
    }
}
