using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.Audio.Filters;
using Accord.DirectSound;

namespace Divie
{
    public partial class AudioSelector : Form
    {
        public AudioDeviceInfo audiocapturedeviceinfo { get; set; }
        private AudioDeviceCollection audioCaptureCollection;

        public AudioSelector()
        {
            InitializeComponent();
        }
        private void form_loader(object sender, EventArgs e)
        {
            audioCaptureCollection = new AudioDeviceCollection(AudioDeviceCategory.Capture); //recording devices list
            if (audioCaptureCollection != null)
            {
                foreach (var item in audioCaptureCollection)
                {
                    comboBox1.Items.Add(item);
                }
            }

        }

        private void yes_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem != null)
            {
                audiocapturedeviceinfo = (AudioDeviceInfo)comboBox1.SelectedItem;
            }
            this.Close();
        }

        private void Abbrechen_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
