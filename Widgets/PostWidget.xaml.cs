﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Utils;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Settings;

namespace Memenim.Widgets
{
    public partial class PostWidget : UserControl
    {
        public static readonly RoutedEvent OnPostClicked =
            EventManager.RegisterRoutedEvent(nameof(PostClick), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(PostWidget));
        public static readonly RoutedEvent OnPostDeleted =
            EventManager.RegisterRoutedEvent(nameof(PostDeleted), RoutingStrategy.Direct, typeof(EventHandler<RoutedEventArgs>), typeof(PostWidget));
        public static readonly DependencyProperty CurrentPostDataProperty =
            DependencyProperty.Register(nameof(CurrentPostData), typeof(PostSchema), typeof(PostWidget),
                new PropertyMetadata((PostSchema) null));

        public event EventHandler<RoutedEventArgs> PostClick
        {
            add
            {
                AddHandler(OnPostClicked, value);
            }
            remove
            {
                RemoveHandler(OnPostClicked, value);
            }
        }

        public event EventHandler<RoutedEventArgs> PostDeleted
        {
            add
            {
                AddHandler(OnPostDeleted, value);
            }
            remove
            {
                RemoveHandler(OnPostDeleted, value);
            }
        }

        public PostSchema CurrentPostData
        {
            get
            {
                return (PostSchema)GetValue(CurrentPostDataProperty);
            }
            set
            {
                SetValue(CurrentPostDataProperty, value);
            }
        }
        public bool CommentsIsOpen
        {
            get
            {
                return CurrentPostData?.open_comments == 1;
            }
        }
        public bool PreviewMode { get; set; }

        public PostWidget()
        {
            InitializeComponent();
            DataContext = this;
        }

        public async Task UpdatePost()
        {
            var result = await PostApi.GetById(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id)
                .ConfigureAwait(true);

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            if (result.data == null)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);
                return;
            }

            CurrentPostData = result.data;
        }

        public void LoadImage(string url)
        {
            if (url == null || !Uri.TryCreate(url, UriKind.Absolute, out Uri _))
                return;

            CurrentPostData.attachments[0].photo.photo_medium = url;
        }

        public void ClearImage()
        {
            CurrentPostData.attachments[0].photo.photo_medium = string.Empty;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            wdgPoster.PostTime = TimeUtils.UnixTimeStampToDateTime(CurrentPostData?.date ?? 0L).ToString(CultureInfo.CurrentCulture);
            wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 2;

            stComments.Visibility = !CommentsIsOpen
                ? Visibility.Collapsed
                : Visibility.Visible;

            btnDeletePost.Visibility = CurrentPostData.owner_id == SettingsManager.PersistentSettings.CurrentUserId
                ? Visibility.Visible
                : Visibility.Collapsed;

            if (PreviewMode)
            {
                wdgPoster.PostTime = (CurrentPostData?.date ?? 0L) == 0L
                    ? DateTime.UtcNow.ToLocalTime().ToString(CultureInfo.CurrentCulture)
                    : wdgPoster.PostTime;
                wdgPoster.IsAnonymous = CurrentPostData?.author_watch != 1;

                stLikes.IsEnabled = false;
                stDislikes.IsEnabled = false;
                stComments.IsEnabled = false;
                stViews.IsEnabled = false;
                stReposts.IsEnabled = false;
            }
        }

        private void Post_Click(object sender, RoutedEventArgs e)
        {
            RaiseEvent(new RoutedEventArgs(OnPostClicked));
        }

        private void CopyPostId_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(CurrentPostData.id.ToString());
        }

        private async void Like_Click(object sender, RoutedEventArgs e)
        {
            stLikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.likes.my == 0)
            {
                result = await PostApi.AddLike(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveLike(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stLikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.likes.my == 0)
                ++CurrentPostData.likes.my;
            else
                --CurrentPostData.likes.my;

            CurrentPostData.likes.count = result.data.count;

            stLikes.IsEnabled = true;
        }

        private async void Dislike_Click(object sender, RoutedEventArgs e)
        {
            stDislikes.IsEnabled = false;

            ApiResponse<CountSchema> result;

            if (CurrentPostData.dislikes.my == 0)
            {
                result = await PostApi.AddDislike(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id)
                    .ConfigureAwait(true);
            }
            else
            {
                result = await PostApi.RemoveDislike(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id)
                    .ConfigureAwait(true);
            }

            if (result.error)
            {
                await DialogManager.ShowDialog("F U C K", result.message)
                    .ConfigureAwait(true);

                stDislikes.IsEnabled = true;
                return;
            }

            if (CurrentPostData.dislikes.my == 0)
                ++CurrentPostData.dislikes.my;
            else
                --CurrentPostData.dislikes.my;

            CurrentPostData.dislikes.count = result.data.count;

            stDislikes.IsEnabled = true;
        }

        private async void DeletePost_Click(object sender, RoutedEventArgs e)
        {
            if(await DialogManager.ShowDialog("Confirmation", "are you sure", MahApps.Metro.Controls.Dialogs.MessageDialogStyle.AffirmativeAndNegative) == MahApps.Metro.Controls.Dialogs.MessageDialogResult.Affirmative)
            {
                var result = await PostApi.Remove(SettingsManager.PersistentSettings.CurrentUserToken, CurrentPostData.id).ConfigureAwait(true);

                if (result.error)
                {
                    await DialogManager.ShowDialog("F U C K", result.message)
                        .ConfigureAwait(true);
                }
                else
                {
                    await DialogManager.ShowDialog("S U C C", "Post was succesfuly removed")
                        .ConfigureAwait(true);
                    RaiseEvent(new RoutedEventArgs(OnPostDeleted));
                }
            }
        }
    }
}
