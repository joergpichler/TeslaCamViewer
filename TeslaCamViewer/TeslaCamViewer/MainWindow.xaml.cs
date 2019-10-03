using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace TeslaCamViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private readonly MainWindowViewModel _viewModel;
        private TimeSpan _totalTime;
        private bool _isPaused;

        public MainWindow()
        {
            _viewModel = new MainWindowViewModel();

            DataContext = _viewModel;

            _viewModel.LeftStatusText = "Ready";
            
            InitializeComponent();

            _viewModel.VideoModel.MediaElementLeftCamera = MediaElementLeft;
            _viewModel.VideoModel.MediaElementRightCamera = MediaElementRight;
            _viewModel.VideoModel.MediaElementFrontCamera = MediaElementFront;
            _viewModel.VideoModel.MediaElementBackCamera = MediaElementBack;
            _viewModel.VideoModel.TabControl = TabControl;
        }

        private void MediaElementLeftOnMediaOpened(object sender, RoutedEventArgs e)
        {
            _totalTime = MediaElementLeft.NaturalDuration.TimeSpan;

            var timerVideoTime = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };

            timerVideoTime.Tick += TimerVideoTimeOnTick;

            timerVideoTime.Start();
        }

        private void TimerVideoTimeOnTick(object sender, EventArgs e)
        {
            if (MediaElementLeft.NaturalDuration.HasTimeSpan && MediaElementLeft.NaturalDuration.TimeSpan.TotalSeconds > 0 &&
                _totalTime.TotalSeconds > 0)
            {
                _viewModel.RightStatusText =
                    MediaElementLeft.Position.ToString(@"mm\:ss") + " / " + _totalTime.ToString(@"mm\:ss");
                SliderTime.Value = MediaElementLeft.Position.TotalSeconds / _totalTime.TotalSeconds;
            }
        }

        private void SetPosition()
        {
            if (_totalTime.TotalSeconds > 0)
            {
                var position = TimeSpan.FromSeconds(SliderTime.Value * _totalTime.TotalSeconds);
                MediaElementLeft.Position = position;
                MediaElementRight.Position = position;
                MediaElementFront.Position = position;
                MediaElementBack.Position = position;
            }
        }

        private void SliderTimeOnDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            MediaElementLeft.Pause();
            MediaElementRight.Pause();
            MediaElementFront.Pause();
            MediaElementBack.Pause();
        }

        private void SliderTimeOnDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            MediaElementLeft.Play();
            MediaElementRight.Play();
            MediaElementFront.Play();
            MediaElementBack.Play();
        }

        private void SliderTimeOnDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            SetPosition();
        }

        private void OnItemMouseDoubleClick(object sender, MouseButtonEventArgs args)
        {
            if (sender is TreeViewItem treeViewItem)
            {
                if (!treeViewItem.IsSelected)
                {
                    return;
                }

                if (TreeView.SelectedItem is TeslaCamFileSet teslaCamFileSet)
                {
                    _viewModel.LoadFileSet(teslaCamFileSet);
                }
            }
        }

        private void WindowOnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                var file = files.FirstOrDefault();

                if (file == null)
                {
                    return;
                }

                var attr = File.GetAttributes(file);

                if (attr.HasFlag(FileAttributes.Directory))
                {
                    _viewModel.ListItems.Clear();
                    var teslaCamDirectoryCollection = new TeslaCamDirectoryCollection();
                    teslaCamDirectoryCollection.BuildFromBaseDirectory(file);
                    _viewModel.ListItems.Add(teslaCamDirectoryCollection);
                    _viewModel.LeftStatusText = "Location: " + file;
                    FrameBrowse.Navigate(new TeslaCamViewer.Views.RootCollectionView(this._viewModel));
                }
            }
        }

        private async Task TeslaCamSearchAsync()
        {
            try
            {
                // Update Status
                _viewModel.LeftStatusText = "Searching for TeslaCam ...";

                // Placeholder variables used during and after worker task
                DirectoryInfo teslaCamDir = null;
                TeslaCamDirectoryCollection recentClips = null;
                TeslaCamDirectoryCollection savedClips = null;
                TeslaCamDirectoryCollection sentryClips = null;

                // Run the following in a worker thread and wait for it to finish
                await Task.Run(() =>
                {
                    // Get all drives
                    var drives = System.IO.DriveInfo.GetDrives();

                    drives = drives.Where(e => e.DriveType == DriveType.Removable ||
                        e.DriveType == DriveType.Network ||
                        e.DriveType == DriveType.Fixed).ToArray();

                    // Find the first drive containing a TeslaCam folder and select that folder
                    teslaCamDir = (from drive in drives
                              let dirs = drive.RootDirectory.GetDirectories()
                              from dir in dirs
                              where dir.Name == "TeslaCam"
                              select dir).FirstOrDefault();

                    // If root is found load Recent and Saved
                    if (teslaCamDir != null)
                    {
                        // Get child dirs
                        var recentClipsDir = teslaCamDir.GetDirectories().FirstOrDefault(e => e.Name == "RecentClips");
                        var savedClipsDir = teslaCamDir.GetDirectories().FirstOrDefault(e => e.Name == "SavedClips");
                        var sentryClipsDir = teslaCamDir.GetDirectories().FirstOrDefault(e => e.Name == "SentryClips");

                        // Load if found
                        if (recentClipsDir != null)
                        {
                            recentClips = new TeslaCamDirectoryCollection();
                            recentClips.BuildFromBaseDirectory(recentClipsDir.FullName);
                            recentClips.SetDisplayName("Recent Clips");
                        }
                        if (savedClipsDir != null)
                        {
                            savedClips = new TeslaCamDirectoryCollection();
                            savedClips.BuildFromBaseDirectory(savedClipsDir.FullName);
                            savedClips.SetDisplayName("Saved Clips");
                        }
                        if (sentryClipsDir != null)
                        {
                            sentryClips = new TeslaCamDirectoryCollection();
                            sentryClips.BuildFromBaseDirectory(sentryClipsDir.FullName);
                            sentryClips.SetDisplayName("Sentry Clips");
                        }
                    }
                });

                // Do finial UI updating back on main thread
                if (teslaCamDir != null)
                {
                    // Update status to show drive was found
                    _viewModel.LeftStatusText = "Location: " + teslaCamDir.FullName;

                    // Add clips to UI tree
                    if (recentClips != null)
                    {
                        _viewModel.ListItems.Add(recentClips);
                    }

                    if (savedClips != null)
                    {
                        _viewModel.ListItems.Add(savedClips);
                    }

                    if (sentryClips != null)
                    {
                        _viewModel.ListItems.Add(sentryClips);
                    }

                    // Navigate
                    FrameBrowse.Navigate(new Views.RootCollectionView(this._viewModel));
                }
                else
                {
                    // Update status to show that drive could not be found
                    _viewModel.LeftStatusText = "Ready";
                    await this.ShowMessageAsync("TeslaCam Drive Not Found", "A TeslaCam drive could not automatically be found. Drag a folder or file to start playing.");
                }
            }
            catch (Exception ex)
            {
                this.ShowMessageAsync("Could not load TeslaCam Drive", "An error ocurred: " + ex.Message).Wait();
            }
        }

        private async void MenuItemSearchForTeslaCamDriveOnClick(object sender, RoutedEventArgs e)
        {
            _viewModel.ListItems.Clear();
            await TeslaCamSearchAsync();
        }

        private void ButtonPlayPauseOnClick(object sender, RoutedEventArgs e)
        {
            if (_isPaused)
            {
                MediaElementLeft.Play();
                MediaElementRight.Play();
                MediaElementFront.Play();
                MediaElementBack.Play();
            }
            else
            {
                MediaElementLeft.Pause();
                MediaElementRight.Pause();
                MediaElementFront.Pause();
                MediaElementBack.Pause();
            }
            _isPaused = !_isPaused;
        }

        private void MenuItemExitOnClick(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void WindowOnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    ButtonPlayPauseOnClick(sender, null);
                    break;
                case Key.F:
                    MenuItemFullScreen.IsChecked = !MenuItemFullScreen.IsChecked;
                    SetFullscreen(MenuItemFullScreen.IsChecked);
                    break;
                case Key.Escape:
                {
                    if (MenuItemFullScreen.IsChecked)
                    {
                        MenuItemFullScreen.IsChecked = !MenuItemFullScreen.IsChecked;
                        SetFullscreen(MenuItemFullScreen.IsChecked);
                    }

                    break;
                }
            }
        }

        private async void WindowOnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.EnableAutoSearch)
            {
                await TeslaCamSearchAsync();
            }
        }

        private void MenuItemAboutOnClick(object sender, RoutedEventArgs e)
        {
            this.ShowMessageAsync("TeslaCam Viewer V0.5.0", "TeslaCam Viewer V0.5.0");
        }

        private void MenuItemViewOnGitHubOnClick(object sender, RoutedEventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/joergpichler/TeslaCamViewer");
        }

        private void SetFullscreen(bool enable)
        {
            if (enable)
            {
                this.SetCurrentValue(IgnoreTaskbarOnMaximizeProperty, true);
                this.SetCurrentValue(WindowStateProperty, WindowState.Maximized);
                this.SetCurrentValue(UseNoneWindowStyleProperty, true);
            }
            else
            {
                this.SetCurrentValue(WindowStateProperty, WindowState.Normal);
                this.SetCurrentValue(UseNoneWindowStyleProperty, false);
                this.SetCurrentValue(ShowTitleBarProperty, true);
                this.SetCurrentValue(IgnoreTaskbarOnMaximizeProperty, false);
            }
        }

        private void MenuItemFullScreenOnClick(object sender, RoutedEventArgs e)
        {
            SetFullscreen(MenuItemFullScreen.IsChecked);
        }

        private void MediaElementLeftOnMediaEnded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_viewModel.EnableAutoPlaylist)
                {
                    var teslaCamFileSet = _viewModel.CurrentPlaybackFile;
                    var teslaCamEventCollection = _viewModel.ListItems.SelectMany(d => d.Events)
                        .First(d => d.Recordings.Contains(teslaCamFileSet));

                    if (teslaCamEventCollection != null)
                    {
                        var currentFileIndex = teslaCamEventCollection.Recordings.IndexOf(teslaCamFileSet);
                        if (teslaCamEventCollection.Recordings.Count - 1 > currentFileIndex)
                        {
                            var nextSet = teslaCamEventCollection.Recordings[currentFileIndex + 1];

                            _viewModel.LoadFileSet(nextSet);

                            var tvi = FindTreeViewItemFromObjectRecursive(TreeView, nextSet);

                            if (tvi != null)
                            {
                                tvi.IsSelected = true;
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }

        public static TreeViewItem FindTreeViewItemFromObjectRecursive(ItemsControl itemsControl, object o)
        {
            if (itemsControl.ItemContainerGenerator.ContainerFromItem(o) is TreeViewItem treeViewItem)
            {
                return treeViewItem;
            }

            foreach (var item in itemsControl.Items)
            {
                var treeViewItem2 = itemsControl.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                treeViewItem = FindTreeViewItemFromObjectRecursive(treeViewItem2, o);
                
                if (treeViewItem != null)
                {
                    return treeViewItem;
                }
            }

            return null;
        }

        private void SliderPlaybackSpeedOnDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            MediaElementLeft.SpeedRatio = _viewModel.CalculatedPlaybackSpeed;
            MediaElementRight.SpeedRatio = _viewModel.CalculatedPlaybackSpeed;
            MediaElementFront.SpeedRatio = _viewModel.CalculatedPlaybackSpeed;
            MediaElementBack.SpeedRatio = _viewModel.CalculatedPlaybackSpeed;
        }
    }
}
