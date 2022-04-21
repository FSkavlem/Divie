using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Accord.Audio;
using Accord.Audio.Formats;
using Accord.Audio.Filters;
using Accord.DirectSound;

using System.Windows.Forms;

namespace Divie
{
    class AudioControll 
    {
        //kan kun ta av seg recording, playback skal kun instansiates en gang.
        private AudioCaptureDevice audioCaptureDevice;
        private AudioDeviceCollection audioCaptureCollection;
        private AudioDeviceInfo audioCaptureDeviceInfo;
        private FileStream stream;
        private WaveEncoder encoder;
 

        int audioChannels = 1;
        int audioBitrate = 44100;
        int audioSampleRate = 44100;  // evt 320*1000;
        int audioFrameSize = 4096; //evt 4096*10

        public void AudioInitilize()
        {
            audioCaptureCollection = new AudioDeviceCollection(AudioDeviceCategory.Capture); //recording devices list
            if (audioCaptureCollection != null)
            {


                //foreach (var item in audioCaptureCollection)
                //{
                //    if (item.Description.Contains("Arctis"))
                //    {
                //        audioCaptureDeviceInfo = item;
                //    }
                //}

                //debug
                audioCaptureDeviceInfo = SoundSelectorForm();
                if (audioCaptureDeviceInfo == null)
                {
                    return;
                }

            }

            audioCaptureDevice = new AudioCaptureDevice(audioCaptureDeviceInfo)
            {
                DesiredFrameSize = audioFrameSize,
                SampleRate = audioSampleRate,
                Format = SampleFormat.Format16Bit //dunno
            };

            audioCaptureDevice.AudioSourceError += Device_AudioSourceEerror;
            audioCaptureDevice.NewFrame += audioDevice_Newframe;
            stream = new FileStream(Application.StartupPath +"\\temp.wav", FileMode.Create);
            encoder = new WaveEncoder(stream);
            encoder.Open(stream);
            audioCaptureDevice.Start();
        }

        private void audioDevice_Newframe(object sender, NewFrameEventArgs eventArgs)
        {
            encoder.Encode(eventArgs.Signal);
        }

        private void Device_AudioSourceEerror(object sender, AudioSourceErrorEventArgs e)
        {
            throw new NotImplementedException();
        }
        public void endAudioRecording()
        {
            if (stream != null)
            {
                stream.Close();
                encoder.Close();
                audioCaptureDevice.SignalToStop();
            }

        }
        private AudioDeviceInfo SoundSelectorForm()
        {
            AudioSelector audioSelector = new AudioSelector();
            audioSelector.ShowDialog();
            return audioSelector.audiocapturedeviceinfo;

        }
    }
}
