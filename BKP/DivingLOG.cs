using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.IO; 

namespace Divie
{
    public delegate void LogEventHandler(object source, newLogEntryArgs ez);

    public class newLogEntryArgs : EventArgs
    {
        private LOGvars newLogEntry;
        public newLogEntryArgs(LOGvars tempLOG)
        {
            newLogEntry = tempLOG;
        }
        public LOGvars GetLOGvars() { return newLogEntry; }
    }

    public class LOGvars
    {
        public DateTime RecordTime{ get; set; }
        public double Depth { get; set; }
    }
    public class DivingLOG
    {
        //Singleton, mark this private so that no other instances of Divinglog can be created.
        public static DivingLOG instance;
        //end Singleton

        private static Timer enTimer;
        public event LogEventHandler newLogEntryAvailable;
        public List<LOGvars> lOGvarsList = new List<LOGvars>();
        public DateTime startTime;
        //--------
        private string[] debugfile;
        private Random rand = new Random();
        //------
        public void singleton() => instance = this;
        public void initate()
        {

            enTimer = new Timer(100); //hvor ofte skal den oppdatere i MS
            enTimer.Elapsed += onTimedEvent;
            enTimer.AutoReset = true;
            enTimer.Enabled = true;
            startTime = DateTime.Now;
            //DEBUG Henter tekst fil, må erstattes med dybde input.
            debugfile = File.ReadAllLines("Random_Stoy.txt");
            //end DEBUG
        }
        public void onTimedEvent(object source,ElapsedEventArgs e)
        {
            if (newLogEntryAvailable != null) //sjekker om vi har noen referance til event.
            {
                LOGvars temp = GetRawData();
                lOGvarsList.Add(temp);
                newLogEntryAvailable(this, new newLogEntryArgs(temp));//fyrer av eventen
            }
        }
        private LOGvars GetRawData()
        {
            //bare drit fordi jeg ikke har noen dybde sensor
            double rawDATA = Convert.ToDouble(debugfile[rand.Next(3600)]);
            LOGvars lOGvars = new LOGvars { RecordTime = DateTime.Now, Depth = rawDATA };
            //some code to get from GPIO and new method for processing data

            return lOGvars;
        }
        private void printLogToFile()
        {
            if (lOGvarsList.Count > 0)
            {
                string pastestring = DateTime.Now.ToString("dd-MM-yyyy_HH-mm-ss") + " DepthLog.txt";
                string[] zArr = new string[lOGvarsList.Count];
                int i = 0;
                foreach (LOGvars item in lOGvarsList)
                {
                    zArr[i] = (string)(item.RecordTime + "," + item.Depth);
                    i++;
                }
                File.WriteAllLines(pastestring, zArr);
                zArr = null;
            }
        }
        public void stop()
        {
            enTimer.Stop();
            enTimer.Dispose();
            printLogToFile();
            lOGvarsList.Clear();
        }
        public string GetDepthString()
        {
            string Depth;
            if (lOGvarsList.Count > 0)
            {
                Depth = "Depth: "+lOGvarsList[lOGvarsList.Count-1].Depth.ToString("00.00") +"M";
            }
            else
            {
                Depth = "Depth: NIL";
            }
            return Depth;
        }
    }
}
