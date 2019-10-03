using System;

namespace TeslaCamViewer
{
    /// <summary>
    /// A single TeslaCam File
    /// </summary>
    public class TeslaCamFile
    {
        private readonly string FileNameRegex = "([0-9]{4}-[0-9]{2}-[0-9]{2}_[0-9]{2}-[0-9]{2}(?:-[0-9]{2})?)-([a-z_]*).mp4";

        public string FilePath { get; }

        public string FileName => System.IO.Path.GetFileName(FilePath);

        public TeslaCamDate Date { get; }

        public CameraType CameraLocation { get; }

        public string FileDirectory => System.IO.Path.GetDirectoryName(FilePath);

        public Uri FileUri => new Uri(this.FilePath);

        public TeslaCamFile(string filePath)
        {
            FilePath = filePath;

            var matches = new System.Text.RegularExpressions.Regex(FileNameRegex).Matches(FileName);
            
            if (matches.Count != 1)
            {
                throw new Exception("Invalid TeslaCamFile '" + FileName + "'");
            }

            Date = new TeslaCamDate(matches[0].Groups[1].Value);

            var cameraType = matches[0].Groups[2].Value;

            switch (cameraType)
            {
                case "front":
                    CameraLocation = CameraType.Front;
                    break;
                case "left_repeater":
                    CameraLocation = CameraType.LeftRepeater;
                    break;
                case "right_repeater":
                    CameraLocation = CameraType.RightRepeater;
                    break;
                case "back":
                    CameraLocation = CameraType.Back;
                    break;
                default:
                    throw new Exception("Invalid Camera Type: '" + cameraType + "'");
            }
        }
    }
}
