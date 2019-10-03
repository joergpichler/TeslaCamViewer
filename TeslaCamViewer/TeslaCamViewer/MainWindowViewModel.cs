using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;

namespace TeslaCamViewer
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<TeslaCamDirectoryCollection> ListItems { get; set; }
        public TeslaCamFileSet CurrentPlaybackFile { get; set; }

        private GridLength _topVideoRowHeight;

        public GridLength TopVideoRowHeight
        {
            get => _topVideoRowHeight;
            set
            {
                if (value != _topVideoRowHeight)
                {
                    _topVideoRowHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private GridLength _bottomVideoRowHeight;

        public GridLength BottomVideoRowHeight
        {
            get => _bottomVideoRowHeight;
            set
            {
                if (value != _bottomVideoRowHeight)
                {
                    _bottomVideoRowHeight = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private GridLength _leftVideoColumnWidth;

        public GridLength LeftVideoColumnWidth
        {
            get => _leftVideoColumnWidth;
            set
            {
                if (value != _leftVideoColumnWidth)
                {
                    _leftVideoColumnWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private GridLength _rightVideoColumnWidth;

        public GridLength RightVideoColumnWidth
        {
            get => _rightVideoColumnWidth;
            set
            {
                if (value != _rightVideoColumnWidth)
                {
                    _rightVideoColumnWidth = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private double _displayPlaybackSpeed;

        public double DisplayPlaybackSpeed
        {
            get => _displayPlaybackSpeed;
            set
            {
                if (value != _displayPlaybackSpeed)
                {
                    _displayPlaybackSpeed = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged(nameof(CalculatedPlaybackSpeed));
                }
            }
        }

        public double CalculatedPlaybackSpeed
        {
            get
            {
                if (DisplayPlaybackSpeed < 0)
                {
                    var calculatedMin = 0.25;
                    var calculatedMax = 1.00;
                    var displayMin = -50;
                    var displayMax = 0;

                    var calc = (calculatedMax - calculatedMin) / (displayMax - displayMin) * (DisplayPlaybackSpeed - displayMax) + calculatedMax;
                    return calc;
                }

                return DisplayPlaybackSpeed + 1.0;
            }
        }

        private string _leftStatusText;

        public string LeftStatusText
        {
            get => _leftStatusText;
            set
            {
                if (value != _leftStatusText)
                {
                    _leftStatusText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private string _rightStatusText;

        public string RightStatusText
        {
            get => _rightStatusText;
            set
            {
                if (value != _rightStatusText)
                {
                    _rightStatusText = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool EnableAutoSearch
        {
            get => Properties.Settings.Default.EnableAutoSearch;
            set
            {
                Properties.Settings.Default.EnableAutoSearch = value;
                Properties.Settings.Default.Save();
            }
        }

        public bool EnableAutoPlaylist
        {
            get => Properties.Settings.Default.EnableAutoPlaylist;
            set
            {
                Properties.Settings.Default.EnableAutoPlaylist = value;
                Properties.Settings.Default.Save();
            }
        }

        public VideoViewModel VideoModel { get; set; }

        public MainWindowViewModel()
        {
            ListItems = new ObservableCollection<TeslaCamDirectoryCollection>();
            VideoModel = new VideoViewModel();

            ResetVideoDisplay();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ICommand SelectFullVideo => new DelegateCommand(DisplayFullVideo);

        private CameraType _currentFullVideo;

        private void DisplayFullVideo(object camera)
        {
            if (camera is CameraType cameraType)
            {
                if (_currentFullVideo == CameraType.Unknown)
                {
                    switch (cameraType)
                    {
                        case CameraType.Front:
                            BottomVideoRowHeight = new GridLength(0);
                            TopVideoRowHeight = new GridLength(1, GridUnitType.Star);
                            LeftVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                            RightVideoColumnWidth = new GridLength(0);
                            break;
                        case CameraType.LeftRepeater:
                            TopVideoRowHeight = new GridLength(0);
                            BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
                            LeftVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                            RightVideoColumnWidth = new GridLength(0);
                            break;
                        case CameraType.RightRepeater:
                            TopVideoRowHeight = new GridLength(0);
                            BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
                            RightVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                            LeftVideoColumnWidth = new GridLength(0);
                            break;
                        case CameraType.Back:
                            BottomVideoRowHeight = new GridLength(0);
                            TopVideoRowHeight = new GridLength(1, GridUnitType.Star);
                            RightVideoColumnWidth = new GridLength(1, GridUnitType.Star);
                            LeftVideoColumnWidth = new GridLength(0);
                            break;
                    }

                    _currentFullVideo = cameraType;
                }
                else
                {
                    ResetVideoDisplay();
                    _currentFullVideo = CameraType.Unknown;
                }
            }
        }

        private void ResetVideoDisplay()
        {
            TopVideoRowHeight = new GridLength(1, GridUnitType.Star);
            BottomVideoRowHeight = new GridLength(1, GridUnitType.Star);
            LeftVideoColumnWidth = new GridLength(1, GridUnitType.Star);
            RightVideoColumnWidth = new GridLength(1, GridUnitType.Star);
        }

        public void LoadFileSet(TeslaCamFileSet set)
        {
            VideoModel.LoadFileSet(set);
            CurrentPlaybackFile = set;
        }
    }
}
