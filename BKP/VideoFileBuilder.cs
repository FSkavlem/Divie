using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

//public delegate void endThread(object source, endThreadArgs e);
//public class endThreadArgs : EventArgs
//{
//    private bool finished;
//    public endThreadArgs(bool x)
//    {
//        finished = x;
//    }
//    public bool GetStatus() { return finished; }
//}
class VideoFileBuilder
{
    public static void mergeAudioVideo(string FinalPath)
    {
        string path = Application.StartupPath + "\\";
        try
        {
            Process pss = new Process();
            ProcessStartInfo si = new ProcessStartInfo();
            si.FileName = path + "\\ffmpeg.exe";
            si.UseShellExecute = false;
            si.CreateNoWindow = true;

            //Compressionrate er gitt med preset, ultrafast, superfast, veryfast, faster, fast, medium – default preset, slow, slower, veryslow
            string argumentString = string.Format("-i \"{0}tempVid.mp4\" -i \"{0}temp.WAV\" -preset ultrafast  \"{1}\" ", path, FinalPath);
            si.Arguments = argumentString;
            pss.StartInfo = si;
            pss.Start();
            pss.WaitForExit();
            pss.Close();
        }
        finally
        {
            DeleteFile(path + "tempVid.mp4");
            DeleteFile(path + "temp.wav");
        }
            
    }
        
    private static void DeleteFile(string x)
    {
        if (File.Exists(x))
        {
            File.Delete(x);
        }
    }

}

