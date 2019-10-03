using System.Linq;
using System.Collections.ObjectModel;

namespace TeslaCamViewer
{
    /// <summary>
    /// A collection of multiple TeslaCam events
    /// Ex. All Sentry Mode recordings
    /// </summary>
    public class TeslaCamDirectoryCollection
    {
        public string DisplayName { get; private set; }

        public ObservableCollection<TeslaCamEventCollection> Events { get; set; }

        public TeslaCamDirectoryCollection()
        {
            Events = new ObservableCollection<TeslaCamEventCollection>();
        }

        public void SetDisplayName(string name)
        {
            DisplayName = name;
        }

        public void BuildFromBaseDirectory(string directory)
        {
            var directories = System.IO.Directory.GetDirectories(directory);
            
            foreach (var dir in directories)
            {
                var teslaCamEventCollection = new TeslaCamEventCollection();

                if (teslaCamEventCollection.BuildFromDirectory(dir))
                {
                    Events.Add(teslaCamEventCollection);
                }
            }

            var baseFiles = System.IO.Directory.GetFiles(directory);

            if (baseFiles.Any())
            {
                var baseCollection = new TeslaCamEventCollection();
                baseCollection.BuildFromDirectory(directory);
                Events.Add(baseCollection);
            }

            Events = new ObservableCollection<TeslaCamEventCollection>(Events.OrderBy(e => e.StartDate.UTCDateString));
            DisplayName = new System.IO.DirectoryInfo(directory).Name;
        }
    }
}
