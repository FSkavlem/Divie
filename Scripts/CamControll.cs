using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Accord.Video;
using Accord.Video.DirectShow;
using Accord.Video.FFMPEG;
using Accord.Imaging.Filters;
using Accord.Controls;

using Accord.Audio;
using Accord.DirectSound;

namespace Divie
{

    public class CamVariables
    {
        public string name;
        public bool CameraFeedStatus { get; set; }
        /// <summary>
        /// Status om kamera tar opp eller ikke.
        /// </summary>
        public bool recordingCTRL { get; set; }
        /// <summary>
        /// Skal Current frame lages snapshot av.
        /// </summary>
        public bool SaveApictureCTRL { get; set; }
        public int ContrastValue { get; set; }
        public int BrightnessValue { get; set; }
        public int OverheadTxtColor { get; set; }
        public int desiredFrameWidth { get; set; }
        public int desiredFrameHeight { get; set; }
        public int CamframeWidth { get; set; }
        public int CamframeHeigth { get; set; }
        public bool useAudio { get; set; }
        public string FinalFileName { get; set; }
        public DateTime RECstartTime { get; set; }
        public TimeSpan RECDuration { get; set; }
        /// <summary>
        /// Midlertidig Bitmap som brukes til å holde frame fra kamera.
        /// </summary>
        public Bitmap TempBitmap { get; set; }

    }
    class CamControll
    {

        private IVideoSource videoSource;

        private VideoCaptureDevice videoCaptureDevice;
        private VideoFileWriter videoFileWriter;
        public CamVariables camVars = new CamVariables();

        private AudioControll audio;


        public void initiate()
        {
            //UpdateDesiredFramesize();
            camVars.useAudio = true;
            camVars.recordingCTRL = false;
            camVars.SaveApictureCTRL= false;
        }

        public bool StartCamFeed(VideoSourcePlayer videoSourcePlayer)
        {
            VideoCaptureDeviceForm form = new VideoCaptureDeviceForm();
            if (form.ShowDialog() == DialogResult.OK)
            {   // create video source and open it 
                videoSource = form.VideoDevice;
                if (videoSourcePlayer.IsRunning){videoSourcePlayer.Stop();}
                
                videoSourcePlayer.VideoSource = videoSource;
                videoSourcePlayer.Start();

                camVars.CameraFeedStatus = true;
                return true;
            }
            return false;
        }
        public void videoSourcePlayer_NewFrame(object sender, ref Bitmap image)
        {
            camVars.CamframeWidth = image.Width;
            camVars.CamframeHeigth = image.Height;

            AddText2bitmap(image);
            ImageBrightContrastCorrection(image);
            if (camVars.recordingCTRL) //hvis frame skal recordes til video
            {
                try //try må brukes ettersom det kan komme tilfelle hvor den prøver å legge en ekstra frame når video kompresjonstarter
                {
                    if (camVars.RECstartTime == DateTime.MinValue) { camVars.RECstartTime = DateTime.Now; }
                    camVars.RECDuration = DateTime.Now - camVars.RECstartTime;
                    TimeSpan timestamp = new TimeSpan(DateTime.Now.Ticks - camVars.RECstartTime.Ticks);
                    if (videoFileWriter != null) { videoFileWriter.WriteVideoFrame(image, timestamp); }
   
                }
                catch (Exception)
                {
                }

            }
            if (camVars.SaveApictureCTRL) //hvis frame skal lagres til bilde
            {
                saveImage(image);
                camVars.SaveApictureCTRL = false;
            }


        }

        public bool StartRecordVideo()
        {
            var dialog = new SaveFileDialog();
            int videoBitrate = 8 * 1000 * 1000; //1000*1000 = 1 Mbps, normal 1080p
            int videoFramerate = 25;
            dialog.FileName = TimeDateString();
            dialog.Filter = "MPEG-4(*.mp4)| *.* ";
            dialog.DefaultExt = "mp4";
            dialog.InitialDirectory = Application.StartupPath;
            dialog.AddExtension = true;
            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return false;
            }
            else
            {

                videoFileWriter = new VideoFileWriter();
                audio = new AudioControll();
                camVars.FinalFileName = dialog.FileName;
                camVars.RECstartTime = DateTime.MinValue;
                audio.AudioInitilize();
                videoFileWriter.Open("tempVid.mp4", camVars.CamframeWidth, camVars.CamframeHeigth, videoFramerate, VideoCodec.H264, videoBitrate);
                camVars.recordingCTRL = true;
                return true;
            }
        }
        public void StoppRecordingVideo()
        {
            camVars.recordingCTRL = false;
            System.Threading.Thread.Sleep(50);
            if (videoFileWriter != null)
            {
                videoFileWriter.Close();
                audio.endAudioRecording();
                videoFileWriter.Dispose();

            }
        }



        //public void VideoCaptureDevice_NewFrame(object sender, Accord.Video.NewFrameEventArgs eventArgs)
        //{

        //    camVars.TempBitmap = (Bitmap)eventArgs.Frame.Clone();
        //    setFrameSize(sender, eventArgs);
        //    Task.WaitAll(
        //        Task.Run(() =>
        //        {
        //            camVars.TempBitmap = AddText2bitmap(camVars.TempBitmap);
        //            camVars.TempBitmap = ImageBrightContrastCorrection(camVars.TempBitmap);
        //            if (camVars.recordingCTRL) //hvis frame skal recordes til video
        //            {
        //                try //try må brukes ettersom det kan komme tilfelle hvor den prøver å legge en ekstra frame når video kompresjonstarter
        //                {
        //                    if (camVars.RECstartTime == DateTime.MinValue) { camVars.RECstartTime = DateTime.Now; }
        //                    camVars.RECDuration = DateTime.Now - camVars.RECstartTime;
        //                    TimeSpan timestamp = new TimeSpan(DateTime.Now.Ticks - camVars.RECstartTime.Ticks);
        //                    if (videoFileWriter != null) { videoFileWriter.WriteVideoFrame(camVars.TempBitmap, timestamp); }
        //                }
        //                catch (Exception)
        //                {
        //                }

        //            }
        //            if (camVars.SaveApictureCTRL) //hvis frame skal lagres til bilde
        //            {
        //                saveImage(camVars.TempBitmap);
        //                camVars.SaveApictureCTRL = false;
        //            }
        //            //UpdateDesiredFramesize();  //TempMap som sendes må justeres iht størrelsen til picturebox for og unngå zoooom. ZOOM FUCKER ALT.
        //            Bitmap tempResized = new Bitmap(camVars.TempBitmap, new Size(camVars.desiredFrameWidth, camVars.desiredFrameHeight));
        //        })
        //    );

        //    camVars.TempBitmap.Dispose();
        //}

        //public void StopCamFeed()
        //{
        //    if (camVars.recordingCTRL)      {StoppRecordingVideo(); }
        //    if (camVars.CameraFeedStatus)   
        //    {
        //        videoCaptureDevice.SignalToStop();
        //        camVars.CameraFeedStatus = false;
        //    }
        //    if (videoFileWriter != null)    {videoFileWriter = null; }

        //}




        private Bitmap ImageBrightContrastCorrection(Bitmap tempMap)
        {
            BrightnessCorrection brightnessFilter = new BrightnessCorrection(camVars.BrightnessValue);
            ContrastCorrection contrastCorrection = new ContrastCorrection(camVars.ContrastValue);
            brightnessFilter.ApplyInPlace(tempMap);
            contrastCorrection.ApplyInPlace(tempMap);
            return tempMap;
        }
        private string TimeDateString()
        {
            string x = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss");
            return x;
        }
        private Bitmap AddText2bitmap(Bitmap x)
        {
            string timeNow = DateTime.Now.ToString();
            string Depth = DivingLOG.instance.GetDepthString();
            string Recording = "Status: ";

            if (camVars.recordingCTRL)
            {
                Recording += "Recording " + camVars.RECDuration.ToString(@"hh\:mm\:ss");
            }
            else
            { Recording += "idle"; };

            using (Graphics graphics = Graphics.FromImage(x))
            {
                using (Font arialFont = new Font("Arial", 20, FontStyle.Bold))
                {   //text thats getting added.
                    Color c1 = ColorTranslator.FromHtml("#6DFFE7");
                    double holder = (camVars.OverheadTxtColor / 100.0);
                    c1 = Color.FromArgb(c1.A, (int)(c1.R * holder), (int)(c1.G * holder), (int)(c1.B * holder));
                    Brush THEBRUSH = new SolidBrush(c1);
                    graphics.DrawString(timeNow, arialFont, THEBRUSH, (new PointF(10f, 10f)));
                    graphics.DrawString(Recording, arialFont, THEBRUSH, (new PointF(350f, 10f)));
                    graphics.DrawString(Depth, arialFont, THEBRUSH, (new PointF(10f, 40f)));
                    THEBRUSH.Dispose();
                    arialFont.Dispose();
                    graphics.Dispose();
                }
            }
            return x;
        }
        private void saveImage(Bitmap x)
        {
            string y = Application.StartupPath + "/" + TimeDateString() + ".jpg";
            if (x != null)
            {
                x.Save(y);
            }
        }
        ////private void UpdateDesiredFramesize()
        ////{
        ////    camVars.desiredFrameHeight = Divie.form1.Controls["panelVideoBox"].Controls["pictureBox1"].Height;
        ////    double temp = camVars.desiredFrameHeight *1.777777778;
        ////    camVars.desiredFrameWidth = Convert.ToInt32(temp);
        ////}
    }
}
