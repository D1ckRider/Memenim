﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Tenor;
using Tenor.Schema;
using Memenim.Commands;
using Memenim.Settings;
using Memenim.Widgets;

namespace Memenim.Pages
{
    public partial class TennorSearchPage : PageContent
    {
        public static readonly DependencyProperty SearchCommandProperty =
            DependencyProperty.Register("SearchCommand", typeof(ICommand), typeof(TennorSearchPage),
                new PropertyMetadata(new BasicCommand(_ => false)));

        public Func<string, Task> OnPicSelect { get; set; }

        public ICommand SearchCommand
        {
            get
            {
                return (ICommand)GetValue(SearchCommandProperty);
            }
            set
            {
                SetValue(SearchCommandProperty, value);
            }
        }

        public TennorSearchPage()
        {
            InitializeComponent();
            DataContext = this;

            SearchCommand = new BasicCommand(
                _ => true, async query => await ExecuteSearch((string)query)
                    .ConfigureAwait(true)
            );
        }

        public async Task ExecuteSearch(string query)
        {
            var config = new TenorConfiguration
            {
                ApiKey = SettingsManager.PersistentSettings.GetTenorAPIKey(),
                Locale = CultureInfo.CurrentCulture,
                ContentFilter = ContentFilter.Medium,
                MediaFilter = MediaFilter.Minimal,
                AspectRatio = AspectRatio.All
            };

            var client = new TenorClient(config);

            IEnumerable<ImagePost> searchResults = !string.IsNullOrEmpty(query)
                ? (await client.SearchAsync(query, 50)
                    .ConfigureAwait(true)).Results
                : (await client.GetTrendingPostsAsync(50)
                    .ConfigureAwait(true)).Results;

            lstImages.Children.Clear();

            if (searchResults == null)
                return;

            foreach (var data in searchResults)
            {
                ImagePreviewButton previewButton = new ImagePreviewButton
                {
                    ButtonSize = 150,
                    ButtonPressAction = OnPicSelect
                };

                foreach (var media in data.Media)
                {
                    if (!media.TryGetValue(MediaType.TinyGif, out MediaItem value))
                        continue;

                    previewButton.SmallImageSource = value?.Url;

                    if (!media.TryGetValue(MediaType.Gif, out value))
                        continue;

                    previewButton.ImageSource = value?.Url;
                }

                if (previewButton.SmallImageSource == null
                    || previewButton.ImageSource == null)
                {
                    continue;
                }

                //var media = data.Media.First();

                //if (!media.TryGetValue(MediaType.TinyGif, out MediaItem value))
                //    continue;

                //previewButton.SmallImageSource = value?.Url;

                //if (!media.TryGetValue(MediaType.Gif, out value))
                //    continue;

                //previewButton.ImageSource = value?.Url;

                lstImages.Children.Add(previewButton);
            }
        }

        protected override async void OnEnter(object sender, RoutedEventArgs e)
        {
            base.OnEnter(sender, e);

            await ExecuteSearch(txtSearchQuery.Text)
                .ConfigureAwait(true);
        }
    }
}
