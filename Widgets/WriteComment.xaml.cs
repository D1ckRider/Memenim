﻿using System;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class WriteComment : UserControl
    {
        public static readonly RoutedEvent OnCommentAdded =
            EventManager.RegisterRoutedEvent(nameof(CommentAdd), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(WriteComment));
        public static readonly DependencyProperty CommentTextProperty =
            DependencyProperty.Register(nameof(CommentText), typeof(string), typeof(WriteComment),
                new PropertyMetadata(string.Empty));
        public static readonly DependencyProperty UserAvatarSourceProperty =
            DependencyProperty.Register(nameof(UserAvatarSource), typeof(string), typeof(WriteComment),
                new PropertyMetadata((string) null));
        public static readonly DependencyProperty PostIdProperty =
            DependencyProperty.Register(nameof(PostId), typeof(int), typeof(WriteComment),
                new PropertyMetadata(-1));
        public static readonly DependencyProperty IsAnonymousProperty =
            DependencyProperty.Register(nameof(IsAnonymous), typeof(bool), typeof(WriteComment),
                new PropertyMetadata(false));

        public event EventHandler<RoutedEventArgs> CommentAdd
        {
            add
            {
                AddHandler(OnCommentAdded, value);
            }
            remove
            {
                RemoveHandler(OnCommentAdded, value);
            }
        }

        private string _realUserAvatarSource;

        public string CommentText
        {
            get
            {
                return (string)GetValue(CommentTextProperty);
            }
            set
            {
                SetValue(CommentTextProperty, value);
            }
        }
        public string UserAvatarSource
        {
            get
            {
                return (string)GetValue(UserAvatarSourceProperty);
            }
            set
            {
                SetValue(UserAvatarSourceProperty, value);
            }
        }
        public int PostId
        {
            get
            {
                return (int)GetValue(PostIdProperty);
            }
            set
            {
                SetValue(PostIdProperty, value);
            }
        }
        public bool IsAnonymous
        {
            get
            {
                return (bool)GetValue(IsAnonymousProperty);
            }
            set
            {
                SetValue(IsAnonymousProperty, value);
            }
        }

        public WriteComment()
        {
            InitializeComponent();
            DataContext = this;
        }

        private async void btnSend_Click(object sender, RoutedEventArgs e)
        {
            btnSend.IsEnabled = false;

            var res = await PostApi.AddComment(SettingsManager.PersistentSettings.CurrentUserToken, PostId, txtContent.Text, IsAnonymous)
                .ConfigureAwait(true);

            if (res.error)
            {
                await DialogManager.ShowDialog("F U C K", res.message)
                    .ConfigureAwait(true);

                btnSend.IsEnabled = true;
                return;
            }

            txtContent.Text = string.Empty;

            RaiseEvent(new RoutedEventArgs(OnCommentAdded));

            btnSend.IsEnabled = true;
        }

        private void btnAnonymous_Click(object sender, RoutedEventArgs e)
        {
            IsAnonymous = !IsAnonymous;

            if (!IsAnonymous)
            {
                UserAvatarSource = _realUserAvatarSource;
            }
            else
            {
                _realUserAvatarSource = UserAvatarSource;
                UserAvatarSource = null;
            }
        }
    }
}
