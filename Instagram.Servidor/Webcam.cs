using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebCam_Capture;
using System.Drawing;

namespace ServidorInstaConsola
{
    class Webcam
    {
        private WebCamCapture webcam;
        private Image _FrameImage;

        public Webcam()
        {
            webcam = new WebCamCapture();
            webcam.FrameNumber = ((ulong)(0ul));
            webcam.TimeToCapture_milliseconds = 30;
            webcam.ImageCaptured += new WebCamCapture.WebCamEventHandler(webcam_ImageCaptured);
        }

        void webcam_ImageCaptured(object source, WebcamEventArgs e)
        {
            _FrameImage = e.WebCamImage;
        }

        public void Start()
        {
            webcam.TimeToCapture_milliseconds = 30;
            webcam.Start(0);
        }

        public void Stop()
        {
            webcam.Stop();
        }

        public Image GetImage()
        {
            return (Image)_FrameImage.Clone();
        }
    }
}
