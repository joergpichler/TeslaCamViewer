using System.Collections.Generic;
using System.Linq;

namespace TeslaCamViewer
{
    /// <summary>
    /// Contains multiple TeslaCam File Sets making up one event
    /// Ex. A single Sentry Mode event
    /// </summary>
    public class TeslaCamEventCollection
    {
        public TeslaCamDate StartDate { get; private set; }

        public TeslaCamDate EndDate { get; private set; }

        public List<TeslaCamFileSet> Recordings { get; set; }

        public TeslaCamFile ThumbnailVideo => Recordings.First().ThumbnailVideo;

        public TeslaCamEventCollection()
        {
            Recordings = new List<TeslaCamFileSet>();
        }

        public bool BuildFromDirectory(string directory)
        {
            // Get list of raw files
            var files = System.IO.Directory.GetFiles(directory, "*.mp4").OrderBy(x => x).ToArray();

            // Make sure there's at least one valid file
            if (files.Length < 1)
            {
                return false;
            }

            // Create a list of cam files
            var currentTeslaCams = new List<TeslaCamFile>(files.Length);

            // Convert raw file to cam file
            foreach (var file in files)
            {
                var teslaCamFile = new TeslaCamFile(file);
                currentTeslaCams.Add(teslaCamFile);
            }

            // Now get list of only distinct events
            var distinctEvents = currentTeslaCams.Select(e => e.Date.UTCDateString).Distinct().ToList();

            // Find the files that match the distinct event
            foreach (var currentEvent in distinctEvents)
            {
                var matchedFiles = currentTeslaCams.Where(e => e.Date.UTCDateString == currentEvent).ToList();
                var currentFileSet = new TeslaCamFileSet();

                currentFileSet.SetCollection(matchedFiles);
                Recordings.Add(currentFileSet);
            }

            // Set metadata
            Recordings = Recordings.OrderBy(e => e.Date.UTCDateString).ToList();
            StartDate = Recordings.First().Date;
            EndDate = Recordings.Last().Date;

            // Success
            return true;
        }
    }
}
