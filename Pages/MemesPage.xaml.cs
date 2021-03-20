
using System;
using System.IO;
using System.Windows;
using Memenim.Core.Api;
using Memenim.Core.Schema;
using Memenim.Dialogs;
using Memenim.Pages.ViewModel;
using Memenim.Settings;
using Microsoft.Win32;

namespace Memenim.Pages
{
    public partial class MemesPage : PageContent
    {
        private static readonly Random Random = new Random();

        private string[] _spamCommentsList;

        public MemesViewModel ViewModel
        {
            get
            {
                return DataContext as MemesViewModel;
            }
        }

        public MemesPage()
        {
            InitializeComponent();
            DataContext = new MemesViewModel();
        }

        private async void btnSteal_Click(object sender, RoutedEventArgs e)
        {
            btnSteal.IsEnabled = false;

            try
            {
                var victimData = await UserApi.GetProfileById(Convert.ToInt32(txtStealId.Value))
                    .ConfigureAwait(true);

                if (victimData.Data == null)
                    throw new Exception();

                await UserApi.EditProfile(SettingsManager.PersistentSettings.CurrentUser.Token, victimData.Data)
                    .ConfigureAwait(true);

                await DialogManager.ShowDialog("Success", "Profile copied")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSteal.IsEnabled = true;
            }
        }

        private async void btnSpamComments_Click(object sender, RoutedEventArgs e)
        {
            if (_spamCommentsList == null || _spamCommentsList.Length == 0)
                return;

            btnSpamComments.IsEnabled = false;

            try
            {
                for (int i = 0; i < txtCommentsCount.Value; ++i)
                {
                    await PostApi.AddComment(SettingsManager.PersistentSettings.CurrentUser.Token,
                            int.Parse(txtCommentsPostId?.Value?.ToString() ?? string.Empty),
                            _spamCommentsList[Random.Next(0, _spamCommentsList.Length - 1)],
                            chkAnonymousComments.IsChecked)
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "Comment section destroyed ;)")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSpamComments.IsEnabled = true;
            }
        }

        private async void btnOpenCommentsFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();

            try
            {
                if (fileDialog?.ShowDialog() != true)
                    return;

                txtCommentsFilePath.Text = fileDialog.FileName;
                _spamCommentsList = await File.ReadAllLinesAsync(fileDialog.FileName)
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
        }

        private async void btnSharesBoost_Click(object sender, RoutedEventArgs e)
        {
            btnSharesBoost.IsEnabled = false;

            try
            {
                for (int i = 0; i < Convert.ToInt32(txtSharesCount.Value); ++i)
                {
                    await PostApi.AddRepost(Convert.ToInt32(txtPostsPostId.Value))
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "BOOOOSTED")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnSharesBoost.IsEnabled = true;
            }
        }

        private async void btnViewsBoost_Click(object sender, RoutedEventArgs e)
        {
            btnViewsBoost.IsEnabled = false;

            try
            {
                for (int i = 0; i < Convert.ToInt32(txtViewsCount.Value); ++i)
                {
                    await PostApi.GetById(Convert.ToInt32(txtPostsPostId.Value))
                        .ConfigureAwait(true);
                }

                await DialogManager.ShowDialog("Success", "BOOOOSTED")
                    .ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await DialogManager.ShowDialog("Some rtarded shit happened", ex.Message)
                    .ConfigureAwait(true);
            }
            finally
            {
                btnViewsBoost.IsEnabled = true;
            }
        }
    }
}