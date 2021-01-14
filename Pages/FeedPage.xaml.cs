﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using Memenim.Commands;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Extensions;
using Memenim.Localization;
using Memenim.Navigation;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Memenim.Widgets;
using WpfAnimatedGif;
using Math = RIS.Mathematics.Math;

namespace Memenim.Pages
{
    public partial class FeedPage : PageContent
    {
        private const int OffsetPerTime = 20;

        private readonly Timer _autoUpdateCountTimer;

        public readonly Storyboard LoadingMoreEnterAnimation;
        public readonly Storyboard LoadingMoreExitAnimation;

        public ReadOnlyDictionary<PostType, string> PostTypes { get; private set; }

        public FeedViewModel ViewModel
        {
            get
            {
                return DataContext as FeedViewModel;
            }
        }

        public FeedPage()
        {
            InitializeComponent();
            DataContext = new FeedViewModel();

            LoadingMoreEnterAnimation = (Storyboard) FindResource(nameof(LoadingMoreEnterAnimation));
            LoadingMoreExitAnimation = (Storyboard) FindResource(nameof(LoadingMoreExitAnimation));

            _autoUpdateCountTimer = new Timer(TimeSpan.FromSeconds(60).TotalMilliseconds);
            _autoUpdateCountTimer.Elapsed += AutoUpdateCountTimerCallback;
            _autoUpdateCountTimer.Stop();

            ReloadPostTypes();

            slcPostTypes.SelectedIndex = 1;

            LocalizationManager.LanguageChanged += OnLanguageChanged;
        }

        ~FeedPage()
        {
            LocalizationManager.LanguageChanged -= OnLanguageChanged;
        }

        private void ReloadPostTypes()
        {
            var names = Enum.GetNames(typeof(PostType));
            var localizedNames = PostType.New.GetLocalizedNames();
            var postTypes = new Dictionary<PostType, string>(names.Length);

            for (var i = 0; i < names.Length; ++i)
            {
                postTypes.Add(
                        Enum.Parse<PostType>(names[i], true),
                        localizedNames[i]);
            }

            slcPostTypes.SelectionChanged -= slcPostTypes_SelectionChanged;

            var selectedIndex = slcPostTypes.SelectedIndex;

            PostTypes = new ReadOnlyDictionary<PostType, string>(postTypes);

            slcPostTypes
                .GetBindingExpression(ItemsControl.ItemsSourceProperty)?
                .UpdateTarget();

            slcPostTypes.SelectedIndex = selectedIndex;

            slcPostTypes.SelectionChanged += slcPostTypes_SelectionChanged;
        }

        public Task UpdatePosts()
        {
            return UpdatePosts(((KeyValuePair<PostType, string>)slcPostTypes.SelectedItem).Key);
        }
        public async Task UpdatePosts(PostType type)
        {
            _autoUpdateCountTimer.Stop();

            slcPostTypes.IsEnabled = false;

            await ShowLoadingGrid(true)
                .ConfigureAwait(true);

            ViewModel.NewPostsCount = 0;

            svPosts.IsEnabled = false;

            foreach (var post in lstPosts.Children)
            {
                PostWidget postWidget = post as PostWidget;

                if (postWidget == null)
                    continue;

                ImageBehavior.SetAnimatedSource(postWidget.PostImage, null);
            }

            lstPosts.Children.Clear();
            svPosts.ScrollToHorizontalOffset(0);

            ViewModel.Offset = 0;

            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            await LoadMorePosts(type)
                .ConfigureAwait(true);

            if (lstPosts.Children.Count == 0)
            {
                ViewModel.LastNewHeadPostId = -1;
            }
            else
            {
                ViewModel.LastNewHeadPostId = (lstPosts.Children[0] as PostWidget)?
                    .CurrentPostData.id ?? -1;
            }

            svPosts.IsEnabled = true;

            await ShowLoadingGrid(false)
                .ConfigureAwait(true);

            slcPostTypes.IsEnabled = true;

            _autoUpdateCountTimer.Start();
        }

        public Task LoadMorePosts()
        {
            return LoadMorePosts(((KeyValuePair<PostType, string>)slcPostTypes.SelectedItem).Key);
        }
        public async Task LoadMorePosts(PostType type)
        {
            if (lstPosts.Children.Count != 0)
            {
                _autoUpdateCountTimer.Stop();

                int newPostsCount = await GetNewPostsCount()
                    .ConfigureAwait(true);

                ViewModel.Offset += newPostsCount;
                ViewModel.NewPostsCount += newPostsCount;

                _autoUpdateCountTimer.Start();
            }

            await LoadMorePosts(type, ViewModel.Offset)
                .ConfigureAwait(true);
        }
        public Task LoadMorePosts(PostType type, int offset)
        {
            return LoadMorePosts(type, OffsetPerTime, offset);
        }
        public async Task LoadMorePosts(PostType type, int count, int offset)
        {
            ShowLoadingMoreGrid(true);

            var result = await PostApi.Get(SettingsManager.PersistentSettings.CurrentUser.Token,
                    type, count, offset)
                .ConfigureAwait(true);

            if (result == null)
                return;

            await AddMorePosts(result.data)
                .ConfigureAwait(true);

            ShowLoadingMoreGrid(false);
        }

        public Task AddMorePosts(List<PostSchema> posts)
        {
            return Task.Run(() =>
            {
                foreach (var post in posts)
                {
                    Dispatcher.Invoke(() =>
                    {
                        PostWidget widget = new PostWidget
                        {
                            CurrentPostData = post
                        };
                        widget.PostClick += OnPost_Click;
                        widget.PostDelete += OnPost_Deleted;

                        lstPosts.Children.Add(widget);
                    });
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.Offset += posts.Count;
                });
            });
        }

        public Task<int> GetNewPostsCount(int offset = 0)
        {
            PostType postType = PostType.Popular;

            Dispatcher.Invoke(() =>
            {
                postType = ((KeyValuePair<PostType, string>)slcPostTypes.SelectedItem).Key;
            });

            return GetNewPostsCount(postType, OffsetPerTime, offset);
        }
        public async Task<int> GetNewPostsCount(PostType type, int countPerTime, int offset)
        {
            int postsCount = 0;

            Dispatcher.Invoke(() =>
            {
                postsCount = lstPosts.Children.Count;
            });

            if (postsCount == 0)
            {
                if (type == PostType.My || type == PostType.Favorite)
                {
                    return await GetAllPostsCount(type, countPerTime, offset)
                        .ConfigureAwait(true);
                }

                return 0;
            }

            int headOldId = -1;

            Dispatcher.Invoke(() =>
            {
                headOldId = ViewModel.LastNewHeadPostId;
            });

            if (headOldId == -1)
                return 0;

            return await Task.Run(async () =>
            {
                int countNew = 0;
                int headNewId = -1;
                bool headOldIsFound = false;
                bool isFirstRequest = true;

                while (!headOldIsFound)
                {
                    var result = await PostApi.Get(SettingsManager.PersistentSettings.CurrentUser.Token,
                            type, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.error != false)
                        continue;

                    if (result.data.Count == 0)
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            await UpdatePosts()
                                .ConfigureAwait(true);
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    if (isFirstRequest)
                    {
                        headNewId = result.data[0].id;
                        isFirstRequest = false;
                    }

                    foreach (var post in result.data)
                    {
                        if (post.id == headOldId)
                        {
                            headOldIsFound = true;
                            break;
                        }

                        ++countNew;
                    }

                    if (countNew >= 500)
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            await UpdatePosts()
                                .ConfigureAwait(true);
                        }).ConfigureAwait(true);

                        return 0;
                    }

                    offset += countPerTime;
                }

                Dispatcher.Invoke(() =>
                {
                    ViewModel.LastNewHeadPostId = headNewId;
                });

                return countNew;
            }).ConfigureAwait(true);
        }

        private async Task<int> GetAllPostsCount(PostType type, int countPerTime, int offset)
        {
            if (!(type == PostType.My || type == PostType.Favorite))
                return 0;

            return await Task.Run(async () =>
            {
                int countNew = 0;

                while (true)
                {
                    var result = await PostApi.Get(SettingsManager.PersistentSettings.CurrentUser.Token,
                            type, countPerTime, offset)
                        .ConfigureAwait(false);

                    if (result?.error != false)
                        continue;

                    if (result.data.Count == 0)
                        break;

                    countNew += result.data.Count;
                    offset += countPerTime;
                }

                return countNew;
            }).ConfigureAwait(true);
        }

        public Task ShowLoadingGrid(bool status)
        {
            if (status)
            {
                loadingIndicator.IsActive = true;
                loadingGrid.Opacity = 1.0;
                loadingGrid.IsHitTestVisible = true;
                loadingGrid.Visibility = Visibility.Visible;

                return Task.CompletedTask;
            }

            loadingIndicator.IsActive = false;

            return Task.Run(async () =>
            {
                for (double i = 1.0; i > 0.0; i -= 0.025)
                {
                    var opacity = i;

                    if (Math.AlmostEquals(opacity, 0.7, 0.01))
                    {
                        Dispatcher.Invoke(() =>
                        {
                            loadingGrid.IsHitTestVisible = false;
                        });
                    }

                    Dispatcher.Invoke(() =>
                    {
                        loadingGrid.Opacity = opacity;
                    });

                    await Task.Delay(4)
                        .ConfigureAwait(false);
                }

                Dispatcher.Invoke(() =>
                {
                    loadingGrid.Visibility = Visibility.Collapsed;
                });
            });
        }

        public void ShowLoadingMoreGrid(bool status)
        {
            if (status)
            {
                loadingMoreIndicator.IsActive = true;
                loadingMoreGrid.IsHitTestVisible = true;

                //LoadingPostEnterAnimation.Begin();
                loadingMoreGrid.BeginStoryboard(LoadingMoreEnterAnimation);

                return;
            }

            loadingMoreIndicator.IsActive = false;
            loadingMoreGrid.IsHitTestVisible = false;

            //LoadingPostExitAnimation.Begin();
            loadingMoreGrid.BeginStoryboard(LoadingMoreExitAnimation);
        }

        protected override void OnEnter(object sender, RoutedEventArgs e)
        {
            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnEnterActive)
            {
                e.Handled = true;
                return;
            }

            base.OnEnter(sender, e);

            ViewModel.OnPostScrollEnd = new BasicCommand(
                _ => true, async _ =>
                {
                    if (svPosts.HorizontalOffset == 0)
                        return;

                    await LoadMorePosts()
                        .ConfigureAwait(true);
                });

            _autoUpdateCountTimer.Start();
        }

        protected override void OnExit(object sender, RoutedEventArgs e)
        {
            UpdateLayout();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            if (!IsOnExitActive)
            {
                e.Handled = true;
                return;
            }

            _autoUpdateCountTimer.Stop();
        }

        private void OnLanguageChanged(object sender, EventArgs e)
        {
            ReloadPostTypes();
        }

        private async void AutoUpdateCountTimerCallback(object sender, ElapsedEventArgs e)
        {
            _autoUpdateCountTimer.Stop();

            int newPostsCount = await GetNewPostsCount()
                .ConfigureAwait(true);

            Dispatcher.Invoke(() =>
            {
                ViewModel.Offset += newPostsCount;
                ViewModel.NewPostsCount += newPostsCount;
            });

            _autoUpdateCountTimer.Start();
        }

        private async void slcPostTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private async void btnNewPostsCount_Click(object sender, RoutedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private async void btnRefreshPosts_Click(object sender, RoutedEventArgs e)
        {
            await UpdatePosts()
                .ConfigureAwait(true);
        }

        private void OnPost_Click(object sender, RoutedEventArgs e)
        {
            if (!(sender is PostWidget post))
                return;

            NavigationController.Instance.RequestOverlay<PostOverlayPage>(new PostOverlayViewModel
            {
                SourcePostWidget = post,
                CurrentPostData = post.CurrentPostData
            });
        }

        private async void OnPost_Deleted(object sender, RoutedEventArgs e)
        {
            _autoUpdateCountTimer.Stop();

            PostWidget post = sender as PostWidget;

            if (post == null)
            {
                _autoUpdateCountTimer.Start();
                return;
            }

            if (ViewModel.LastNewHeadPostId == post.CurrentPostData.id)
            {
                await UpdatePosts()
                    .ConfigureAwait(true);

                _autoUpdateCountTimer.Start();
                return;
            }

            lstPosts.Children.Remove(post);

            --ViewModel.Offset;

            _autoUpdateCountTimer.Start();
        }

        private void CreatePost_Click(object sender, RoutedEventArgs e)
        {
            NavigationController.Instance.RequestPage<SubmitPostPage>();
        }

        private void SvPosts_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ViewModel.ScrollOffset = e.HorizontalOffset;
        }
    }
}
