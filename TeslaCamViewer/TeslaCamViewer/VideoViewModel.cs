using System;
using System.Windows.Controls;

namespace TeslaCamViewer
{
    public class VideoViewModel
    {
        public MediaElement MediaElementLeftCamera { get; set; }
        public MediaElement MediaElementRightCamera { get; set; }
        public MediaElement MediaElementFrontCamera { get; set; }
        public MediaElement MediaElementBackCamera { get; set; }
        public TabControl TabControl { get; set; }

        public void LoadFileSet(TeslaCamFileSet teslaCamFileSet)
        {
            MediaElementLeftCamera.Stop();
            MediaElementRightCamera.Stop();
            MediaElementFrontCamera.Stop();
            MediaElementBackCamera.Stop();

            var playLeft = false;
            var playRight = false;
            var playFront = false;
            var playBack = false;

            foreach (var cam in teslaCamFileSet.Cameras)
            {
                switch (cam.CameraLocation)
                {
                    case CameraType.Front:
                        MediaElementFrontCamera.Source = new Uri(cam.FilePath);
                        playFront = true;
                        break;
                    case CameraType.LeftRepeater:
                        MediaElementLeftCamera.Source = new Uri(cam.FilePath);
                        playLeft = true;
                        break;
                    case CameraType.RightRepeater:
                        MediaElementRightCamera.Source = new Uri(cam.FilePath);
                        playRight = true;
                        break;
                    case CameraType.Back:
                        MediaElementBackCamera.Source = new Uri(cam.FilePath);
                        playBack = true;
                        break;
                }
            }

            if (playLeft)
            {
                MediaElementLeftCamera.Play();
            }

            if (playRight)
            {
                MediaElementRightCamera.Play();
            }

            if (playFront)
            {
                MediaElementFrontCamera.Play();
            }

            if (playBack)
            {
                MediaElementBackCamera.Play();
            }

            TabControl.SelectedIndex = 1;
        }
    }
}