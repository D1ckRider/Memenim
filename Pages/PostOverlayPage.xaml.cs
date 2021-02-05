﻿using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Storage;
using Memenim.Utils;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class PostOverlayPage : PageContent
    {
        private double _writeCommentMinHeight;
        private double _writeCommentMaxHeight;

        public PostOverlayViewModel ViewModel
        {
            get
            {
                return DataContext as PostOverlayViewModel;
            }
        }
        public bool CommentsIsOpen
        {
            get
            {
                return ViewModel.CurrentPostData?.open_comments == 1;
            }
        }

        public PostOverlayPage()
        {
            InitializeComponent();
            DataContext = new PostOverlayViewModel();

            _writeCommentMinHeight = PostGrid.RowDefinitions[1].MinHeight;
            _writeCommentMaxHeight = ActualHeight * 0.3;

            SettingsManager.PersistentSettings.CurrentUserChanged += OnCurrentUserChanged;
            ProfileUtils.AvatarChanged += OnAvatarChanged;
        }

        ~PostOverlayPage()
        {
            SettingsManager.PersistentSettings.CurrentUserChanged -= OnCurrentUserChanged;
            ProfileUtils.AvatarChanged -= OnAvatarChanged;
        }

        public Task UpdatePost()
        {
            return UpdatePost(ViewModel.CurrentPostData.id);
        }
        public async Task UpdatePost(int id)
        {
            if (id < 1)
            {
                if (!NavigationController.Instance.IsCurrentPage<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                string message = LocalizationUtils.GetLocalized("PostNotFound");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            ViewModel.CurrentPostData.id = id;

            var result = await PostApi.GetById(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    ViewModel.CurrentPostData.id)
                .ConfigureAwait(true);

            if (result.error)
            {
                if (!NavigationController.Instance.IsCurrentPage<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                await DialogManager.ShowErrorDialog(result.message)
                    .ConfigureAwait(true);

                return;
            }

            if (result.data == null)
            {
                if (!NavigationController.Instance.IsCurrentPage<PostOverlayPage>())
                    return;

                NavigationController.Instance.GoBack(true);

                string message = LocalizationUtils.GetLocalized("PostNotFound");

                await DialogManager.ShowErrorDialog(message)
                    .ConfigureAwait(true);

                return;
            }

            ViewModel.CurrentPostData = result.data;

            if (ViewModel.SourcePostWidget?.CurrentPostData.id == ViewModel.CurrentPostData.id)
            {
                ViewModel.SourcePostWidget?.SetValue(
                    PostWidget.CurrentPostDataProperty,
                    ViewModel.CurrentPostData);
            }
        }

        public async Task<PostSchema> GetUpdatedPostData()
        {
            PostSchema postData = null;

            Dispatcher.Invoke(() =>
            {
                postData = ViewModel.CurrentPostData;
            });

            if (postData == null)
                return postData;

            var result = await PostApi.GetById(
                    SettingsManager.PersistentSettings.CurrentUser.Token,
                    postData.id)
                .ConfigureAwait(true);

            if (result.error)
                return postData;

            return result.data ?? postData;
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            if (!CommentsIsOpen)
            {
                wdgWriteComment.Visibility = Visibility.Collapsed;
                wdgCommentsList.Visibility = Visibility.Collapsed;
            }
            else
            {
                wdgCommentsList.Visibility = Visibility.Visible;
                wdgWriteComment.Visibility = Visibility.Visible;
            }

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            svPost.ScrollToVerticalOffset(0.0);

            await UpdatePost()
                .ConfigureAwait(true);

            var result = await UserApi.GetProfileById(
                    SettingsManager.PersistentSettings.CurrentUser.Id)
                .ConfigureAwait(true);

            if (result.data != null)
                wdgWriteComment.SetRealUserAvatarSource(result.data.photo);

            var draft = await StorageManager.GetPostCommentDraft(
                    SettingsManager.PersistentSettings.CurrentUser.Id,
                    ViewModel.CurrentPostData.id)
                .ConfigureAwait(true);

            wdgWriteComment.CommentText = draft.CommentText;
            wdgWriteComment.IsAnonymous = draft.IsAnonymous;

            wdgWriteComment.txtContent.ScrollToEnd();
            wdgWriteComment.txtContent.CaretIndex = draft.CommentText.Length;
            wdgWriteComment.txtContent.Focus();

            if (svPost.VerticalOffset >= svPost.ScrollableHeight - 20)
                svPost.ScrollToVerticalOffset(0.0);
        }

        protected override async void OnExit(object sender, RoutedEventArgs e)
        {
            base.OnExit(sender, e);

            await StorageManager.SetPostCommentDraft(
                    SettingsManager.PersistentSettings.CurrentUser.Id,
                    ViewModel.CurrentPostData.id,
                    wdgWriteComment.CommentText,
                    wdgWriteComment.IsAnonymous)
                .ConfigureAwait(true);

            wdgCommentsList.lstComments.Children.Clear();

            wdgCommentsList.UpdateLayout();
            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }
        }

#pragma warning disable SS001 // Async methods should return a Task to make them awaitable
        protected override async void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is PostOverlayViewModel oldViewModel)
            {
                if (oldViewModel.SourcePostWidget?.CurrentPostData.id == oldViewModel.CurrentPostData.id)
                {
                    oldViewModel.SourcePostWidget?.SetValue(
                        PostWidget.CurrentPostDataProperty,
                        oldViewModel.CurrentPostData);
                }

                await StorageManager.SetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUser.Id,
                        oldViewModel.CurrentPostData.id,
                        wdgWriteComment.CommentText,
                        wdgWriteComment.IsAnonymous)
                    .ConfigureAwait(true);
            }

            wdgWriteComment.CommentText = string.Empty;
            wdgWriteComment.IsAnonymous = false;

            if (e.NewValue is PostOverlayViewModel newViewModel)
            {
                if (newViewModel.SourcePostWidget?.CurrentPostData.id == newViewModel.CurrentPostData.id)
                {
                    newViewModel.SourcePostWidget?.SetValue(
                        PostWidget.CurrentPostDataProperty,
                        newViewModel.CurrentPostData);
                }

                var draft = await StorageManager.GetPostCommentDraft(
                        SettingsManager.PersistentSettings.CurrentUser.Id,
                        newViewModel.CurrentPostData.id)
                    .ConfigureAwait(true);

                wdgWriteComment.CommentText = draft.CommentText;
                wdgWriteComment.IsAnonymous = draft.IsAnonymous;

                wdgWriteComment.txtContent.ScrollToEnd();
                wdgWriteComment.txtContent.CaretIndex = draft.CommentText.Length;
                wdgWriteComment.txtContent.Focus();
            }

            base.OnDataContextChanged(sender, e);
        }
#pragma warning restore SS001 // Async methods should return a Task to make them awaitable

        protected override async void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.ViewModelPropertyChanged(sender, e);

            if (e.PropertyName.Length == 0)
            {
                await UpdatePost()
                    .ConfigureAwait(true);
            }
            else if (e.PropertyName == nameof(ViewModel.CurrentPostData))
            {
                if (ViewModel.SourcePostWidget?.CurrentPostData.id == ViewModel.CurrentPostData.id)
                {
                    ViewModel.SourcePostWidget?.SetValue(
                        PostWidget.CurrentPostDataProperty,
                        ViewModel.CurrentPostData);
                }
            }
        }

        private async void OnCurrentUserChanged(object sender, UserChangedEventArgs e)
        {
            if (e.NewUser.Id == -1)
                return;

            await UpdatePost()
                .ConfigureAwait(true);

            var result = await UserApi.GetProfileById(
                    SettingsManager.PersistentSettings.CurrentUser.Id)
                .ConfigureAwait(true);

            if (!result.error
                && result.data != null)
            {
                wdgWriteComment.SetRealUserAvatarSource(result.data.photo);
            }

            await StorageManager.SetPostCommentDraft(
                    e.OldUser.Id,
                    ViewModel.CurrentPostData.id,
                    wdgWriteComment.CommentText,
                    wdgWriteComment.IsAnonymous)
                .ConfigureAwait(true);

            var draft = await StorageManager.GetPostCommentDraft(
                    e.NewUser.Id,
                    ViewModel.CurrentPostData.id)
                .ConfigureAwait(true);

            if (!string.IsNullOrEmpty(draft.CommentText))
            {
                wdgWriteComment.CommentText = draft.CommentText;
                wdgWriteComment.IsAnonymous = draft.IsAnonymous;

                wdgWriteComment.txtContent.ScrollToEnd();
                wdgWriteComment.txtContent.CaretIndex = draft.CommentText.Length;
                wdgWriteComment.txtContent.Focus();
            }
        }

        private void OnAvatarChanged(object sender, UserPhotoChangedEventArgs e)
        {
            wdgWriteComment.SetRealUserAvatarSource(e.NewPhoto);
        }

        private void SvPost_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.VerticalOffset;
        }

        private async void CommentsList_CommentsUpdate(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });
        }

        private async void CommentsList_CommentReply(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            UserComment comment = e.OriginalSource as UserComment;

            if (comment == null)
                return;

            double verticalOffset = svPost.VerticalOffset;
            double scrollableHeight = svPost.ScrollableHeight;
            string replyText = //$">>> {commentsList.PostId}@{comment.CurrentCommentData.id}@{comment.CurrentCommentData.user.id} {comment.CurrentCommentData.user.name}:\n\n"
                               $">>> {comment.CurrentCommentData.user.name}:\n\n"
                               + $"{comment.CurrentCommentData.text}\n\n"
                               + ">>>\n\n";

            int oldCaretIndex = wdgWriteComment.txtContent.CaretIndex;
            wdgWriteComment.CommentText = replyText + wdgWriteComment.CommentText;
            wdgWriteComment.txtContent.CaretIndex = replyText.Length + oldCaretIndex;
            wdgWriteComment.txtContent.ScrollToEnd();
            wdgWriteComment.txtContent.Focus();

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();

            if (comment.CurrentCommentData.user.id == SettingsManager.PersistentSettings.CurrentUser.Id)
                return;

            for (int i = 0; i < 2; ++i)
            {
                if (comment.CurrentCommentData.likes.my == 0)
                {
                    await PostApi.AddLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            comment.CurrentCommentData.id)
                        .ConfigureAwait(true);
                }
                else
                {
                    await PostApi.RemoveLikeComment(
                            SettingsManager.PersistentSettings.CurrentUser.Token,
                            comment.CurrentCommentData.id)
                        .ConfigureAwait(true);
                }

                if (comment.CurrentCommentData.likes.my == 0)
                    ++comment.CurrentCommentData.likes.my;
                else
                    --comment.CurrentCommentData.likes.my;
            }
        }

        private async void CommentsList_CommentDelete(object sender, RoutedEventArgs e)
        {
            CommentsList commentsList = sender as CommentsList;

            if (commentsList == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });
        }

        private async void WriteComment_CommentAdd(object sender, RoutedEventArgs e)
        {
            WriteComment writeComment = sender as WriteComment;

            if (writeComment == null)
                return;

            var postData = await GetUpdatedPostData()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.CurrentPostData.comments = postData.comments;
            });

            await wdgCommentsList.LoadNewComments()
                .ConfigureAwait(true);
        }

        private void WriteComment_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            double verticalOffset = svPost.VerticalOffset;
            double scrollableHeight = svPost.ScrollableHeight;
            double writeCommentHeight =
                Math.Min(Math.Max(e.NewSize.Height, _writeCommentMinHeight),
                    _writeCommentMaxHeight);

            PostGrid.RowDefinitions[0].Height = new GridLength(
                ActualHeight - writeCommentHeight);
            PostGrid.RowDefinitions[1].Height = new GridLength(
                writeCommentHeight);

            //wdgWriteComment.MaxHeight = writeCommentHeight + 1;

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();

            //svPost.ScrollToVerticalOffset(
            //    svPost.VerticalOffset + (e.NewSize.Height - e.PreviousSize.Height));
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (!e.HeightChanged)
            //    return;

            _writeCommentMaxHeight = e.NewSize.Height * 0.3;
            wdgWriteComment.MaxHeight = _writeCommentMaxHeight;

            double verticalOffset = svPost.VerticalOffset;
            double scrollableHeight = svPost.ScrollableHeight;
            double writeCommentHeight =
                Math.Min(Math.Max(wdgWriteComment.ActualHeight, _writeCommentMinHeight),
                    _writeCommentMaxHeight);

            PostGrid.RowDefinitions[0].Height = new GridLength(
                e.NewSize.Height - writeCommentHeight);
            PostGrid.RowDefinitions[1].Height = new GridLength(
                writeCommentHeight);

            //wdgWriteComment.MaxHeight = writeCommentHeight + 1;

            if (verticalOffset >= scrollableHeight - 20)
                svPost.ScrollToEnd();
        }
    }
}
