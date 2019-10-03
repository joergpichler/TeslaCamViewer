using System.Collections.Generic;
using System.Linq;

namespace TeslaCamViewer
{
    /// <summary>
    /// A set of multiple matched TeslaCamFiles (multiple camera angles)
    /// </summary>
    public class TeslaCamFileSet
    {
        public TeslaCamDate Date { get; private set; }

        public List<TeslaCamFile> Cameras { get; private set; }

        public TeslaCamFile ThumbnailVideo => Cameras.First(e => e.CameraLocation == CameraType.Front);

        public void SetCollection(List<TeslaCamFile> cameras)
        {
            Cameras = cameras;
            Date = cameras.First().Date;
        }
    }
}
