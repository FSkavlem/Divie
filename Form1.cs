using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using System.IO;
using System.Resources;

namespace Divie
{
    public partial class Divie : Form
    {
        public static Form form1;
        public Divie()
        {
            InitializeComponent();
            form1 = this;
            btConnect_Video.Enabled = true;
            btDisconnect_Video.Enabled = false;
            btStart_Dive.Enabled = true;
            btStop_Dive.Enabled = false;
        }

        private CamControll cam1;

        private DivingLOG divingLOG;
        private int ChartLength, SideMenuWidth;
        private Button activebutton;
        private bool isResizeMode, SideMenuVisable;

        private void Form1_load(object sender, EventArgs e)
        {

            this.MinimumSize = this.Size; //Setter minste størrelse til Form1 

            //kamera
            cam1 = new CamControll();

            cam1.initiate();

            //Log
            divingLOG = new DivingLOG();
            divingLOG.singleton(); //brukes for og kun refere til denne instansen av DivingLog
            divingLOG.newLogEntryAvailable += new LogEventHandler(updateLogs); //Event

            //diagram 
            ChartLength = 300;
            chart1.ChartAreas[0].AxisX.MaximumAutoSize = 100; //sier at 100% av axisx.maximum skal vises
            chart1.ChartAreas[0].AxisX.Maximum = ChartLength; //hvor mange axisX entries
            chart1.ChartAreas[0].AxisX.Interval = 10;         //avstanden mellom hver linje på X akse
            activebutton = btChart5min;
            isResizeMode = false;
            SideMenuVisable = true;
            SideMenuWidth = panelSideMenu.Width;
        }



        #region Buttons
        private void ConnectVideo_Click(object sender, EventArgs e)
        {
            if (cam1.StartCamFeed(videoSourcePlayer1))
            {
                buttonEnableSwapper(btConnect_Video, btDisconnect_Video);
                btStart_Recording.Enabled = true;
                btTake_Picture.Enabled = true;
            }
        }
        private void DisconnectVideo_Click(object sender, EventArgs e)
        {
            string x = "Are you sure you wish to end videofeed? Ending videofeed will terminate any recording.";
            if (MessageBox(x, "Disconnect video"))
            {
                buttonEnableSwapper(btConnect_Video, btDisconnect_Video);
                btTake_Picture.Enabled = false;
                btStart_Recording.Enabled = false;
                btStop_Recording.Enabled = false;
                //cam1.StopCamFeed();
            }
        }
        private void Start_Dive_Click(object sender, EventArgs e)
        {
            buttonEnableSwapper(btStart_Dive, btStop_Dive);
            ClearCharts();
            divingLOG.initate();
        }

        private void Stop_Dive_Click(object sender, EventArgs e)
        {
            if (MessageBox("Are you sure you want to end dive?", "End Dive"))
            {
                buttonEnableSwapper(btStart_Dive, btStop_Dive);
                divingLOG.stop();
            }

        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            OpenFolderinExplorer(Application.StartupPath);
        }
        private void Take_Picture_Click(object sender, EventArgs e)
        {
            string path = Application.StartupPath + "/mixkit-camera-lens-shutter-1433.wav";
            System.Media.SoundPlayer player = new System.Media.SoundPlayer(path);
            player.Play();
            cam1.camVars.SaveApictureCTRL = true; //v1
        }
        private void Start_Recording_Click(object sender, EventArgs e)
        {
            if (cam1.StartRecordVideo())
            {
                buttonEnableSwapper(btStart_Recording, btStop_Recording);
            }
        }

        private void Stop_Recording_Click(object sender, EventArgs e)
        {
            string x = "Do you wish to end recording and process video?";
            if (MessageBox(x, "End recording"))
            {
                StoppRecordingProcessVideo();
            }
            
        }

        private void Exit_Click(object sender, EventArgs e)
        {
            string x = "Do you wish to exit and shutdown?";
            if (MessageBox(x,"Exit"))
            {
                //cam1.StopCamFeed(); //v1
                Task t = StoppRecordingProcessVideo();
                t.GetAwaiter();

                this.Close();
            }
        }
        //private void btTopCorner_Click(object sender, EventArgs e) //for sliding sidemenu, must have labels insteed of button text.
        //{
        //    SideMenuSlider();
        //}


        private void btChart5min_Click(object sender, EventArgs e) => TabSorter((Button)sender);
        private void btChart10min_Click(object sender, EventArgs e) => TabSorter((Button)sender);
        private void btChart15min_Click(object sender, EventArgs e) => TabSorter((Button)sender);
        private void btChart30min_Click(object sender, EventArgs e) => TabSorter((Button)sender);
        private void btChart60min_Click(object sender, EventArgs e) => TabSorter((Button)sender);
        private void ContrastLabel_DoubleClick(object sender, EventArgs e) => trackBarContrast.Value = 0;
        private void BrightnessLabel_DoubleClick(object sender, EventArgs e) => trackBarBrightness.Value = 0;

        #endregion
        #region TrggeredByEvents
        private void videoSourcePlayer1_NewFrame(object sender, ref Bitmap image)
        {
            cam1.videoSourcePlayer_NewFrame(sender, ref image);
        }
        private void updateLogs(object source, newLogEntryArgs ez)
        {
            Chart chart = chart1;
            UpdateCharts(chart1);
        }
        private void videoFileBuilderComplete(object sender, EventArgs e)
        {
            buttonEnableSwapper(btStart_Recording, btStop_Recording);
        }
        private void Divie_Leave(object sender, EventArgs e)
        {
            //save logs, video etc if running....!!!
        }
        private void trackBarBrightness_ValueChanged(object sender, EventArgs e) => cam1.camVars.BrightnessValue = trackBarBrightness.Value*10;
        private void trackBarContrast_ValueChanged(object sender, EventArgs e) => cam1.camVars.ContrastValue = trackBarContrast.Value*10;
        private void trackBarOverHeadColor_ValueChanged(object sender, EventArgs e) => cam1.camVars.OverheadTxtColor = trackBarOverHeadColor.Value*5;
        private void panelChartSizeGrabber_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isResizeMode = true;
            }
        }
        private void panelChartSizeGrabber_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizeMode)
            {
                panelChartMenu.Size = new Size(panelChartMenu.Size.Width, panelChartMenu.Size.Height - e.Y);
            }
        }
        private void panelChartSizeGrabber_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isResizeMode = false;
            }
        }

        #endregion
        #region Function
        private async Task StoppRecordingProcessVideo() //async Task
        {
            btStop_Recording.Enabled = false;
            btStart_Recording.Text = "PROCESSING VIDEO";
            btStop_Recording.Text = "PROCESSING VIDEO";

            cam1.StoppRecordingVideo();
            //audio1.endAudioRecording();
            Action action = () => VideoFileBuilder.mergeAudioVideo(cam1.camVars.FinalFileName); 
            Task t = new Task(action);
            t.Start(); //starter video kompressoren.

            await Task.WhenAll(t); //venter på at video kompresjonen skal gjennomføres.

            btStart_Recording.Text = "Start Recording";
            btStop_Recording.Text = "Stop Recording";
            btStart_Recording.Enabled = true;
        }
        private void buttonEnableSwapper(Button a, Button b)
        {
            if (a.Enabled)
            {
                a.Enabled = false;
                b.Enabled = true;
            }
            else
            {
                a.Enabled = true;
                b.Enabled = false;
            }
        }
        public void OpenFolderinExplorer(string x)
        {
            Process.Start(x);
        }
        private void UpdateCharts(Chart chart)
        {
            try
            {
                Invoke(new Action(() =>
                {
                    if (chart.Series != null)
                    {
                        chart.Series.Clear();
                    }

                    chart1.ChartAreas[0].AxisX.Maximum = ChartLength; //hvor mange axisX entries

                    Series series = chart.Series.Add("Length");
                    series.ChartType = SeriesChartType.SplineRange;
                    series.Color = ColorTranslator.FromHtml("#55BEC0");
                    series.LabelBackColor = Color.White;

                    int startPos = 0;
                    if (divingLOG.lOGvarsList.Count > ChartLength)                  //hvis dykket har akkurat startet og entry < tid
                    {
                        startPos = divingLOG.lOGvarsList.Count - ChartLength;
                    }
                    for (int i = startPos; i < divingLOG.lOGvarsList.Count; i++)
                    {
                        //hvis performance blir et issue, vurder å paste array rett til chart!! 
                        TimeSpan sinceStart = divingLOG.lOGvarsList[i].RecordTime.Subtract(divingLOG.startTime);
                        string pasteString = sinceStart.Hours + "h:" + sinceStart.Minutes + "m:" + sinceStart.Seconds + "s";
                        series.Points.AddXY(pasteString, -divingLOG.lOGvarsList[i].Depth);//OBS OBS er satt på minus her for å gjøre de negative.
                    }
                }));
            }
            catch (Exception)
            {
            }
        }
        public void ClearCharts()
        {
            chart1.Series.Clear();
        }
        private void TabSorter(Button newZ)
        {
            ChartLength = Convert.ToInt32(newZ.Tag);
            int tempholder = ChartLength / 33;

            Color activelight = activebutton.BackColor;
            Color notActiveDark = newZ.BackColor;

            newZ.BackColor = activelight;
            activebutton.BackColor = notActiveDark;
            chart1.ChartAreas[0].AxisX.Interval = tempholder;
            activebutton = newZ;
        }
        private void LoopButtonsInPanelColorChanger(Color color) //not in use;
        {
            foreach (Control c in panelSideMenu.Controls)
            {
                if (c is Button)
                {
                    c.ForeColor = color;
                }
            }
        }

        private void SideMenuSlider()
        {
            if (SideMenuVisable)
            {
                SideMenuVisable = false;
                panelSideMenu.Size = new Size(Width = 40, Height = panelSideMenu.Height);
                //btTopCorner.Image = (Image)Properties.Resources.ResourceManager.GetObject("outline_arrow_forward_ios_white_24dp");
                LoopButtonsInPanelColorChanger(Color.Yellow);
            }
            else
            {
                SideMenuVisable = true;
                panelSideMenu.Size = new Size(Width = SideMenuWidth, Height = panelSideMenu.Height);
               // btTopCorner.Image = (Image)Properties.Resources.ResourceManager.GetObject("outline_arrow_back_ios_white_24dp");
                LoopButtonsInPanelColorChanger(Color.White);
            }
        }//ingen sidemenuslide, hvis det skal sttes inn, bruk panel + button uten text kun bilde.

        private bool MessageBox(string message, string title)
        {
            DialogResult dialogResult = System.Windows.Forms.MessageBox.Show(message, title, MessageBoxButtons.YesNo,MessageBoxIcon.Warning );
            if (dialogResult == DialogResult.Yes)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion


    }
}
