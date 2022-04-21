using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;


using Accord.Video;
using Accord.Video.DirectShow;
using Accord.Video.FFMPEG;
using Accord.Controls;

namespace Divie
{
    class CamControlV2
    {

        private VideoSourcePlayer videoSourcePlayer = Application.OpenForms["Divie"].Controls["panelVideoBox"].Controls["videoSourcePlayer1"] as VideoSourcePlayer;
        private FilterInfoCollection filterInfoCollection;

        public void StartCamFeed()
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();

            if (form.ShowDialog() == DialogResult.OK)
            {
                // create video source
                VideoCaptureDevice videoSource = form.VideoDevice;

                // open it
                OpenVideoSource(videoSource);
            }
        }
        private void OpenVideoSource(IVideoSource source)
        {
            // stop current video source
            CloseVideoSource();
            // start new video source
            videoSourcePlayer.VideoSource = source;
            videoSourcePlayer.Start();
        }

        public void CloseVideoSource()
        {
            if (videoSourcePlayer.VideoSource != null)
            {
                videoSourcePlayer.SignalToStop();

                // wait ~ 3 seconds
                for (int i = 0; i < 30; i++)
                {
                    if (!videoSourcePlayer.IsRunning)
                        break;
                    System.Threading.Thread.Sleep(100);
                }

                if (videoSourcePlayer.IsRunning)
                {
                    videoSourcePlayer.Stop();
                }

                videoSourcePlayer.VideoSource = null;
            }
        }
        private void videoSourcePlayer_NewFrame(object sender, NewFrameEventArgs args)
        {
            DateTime now = DateTime.Now;
            Graphics g = Graphics.FromImage(args.Frame);

            // paint current time
            SolidBrush brush = new SolidBrush(Color.Red);
            g.DrawString(now.ToString(), Application.OpenForms["Divie"].Font, brush, new PointF(5, 5));
            brush.Dispose();

            g.Dispose();
        }
    }
}
