using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Wakeup
{
    delegate string GetState();
    delegate void MakeAction();
    

    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern EXECUTION_STATE SetThreadExecutionState(EXECUTION_STATE esFlags);

        [FlagsAttribute]
        public enum EXECUTION_STATE : uint
        {
            ES_AWAYMODE_REQUIRED = 0x00000040,
            ES_CONTINUOUS = 0x80000000,
            ES_DISPLAY_REQUIRED = 0x00000002,
            ES_SYSTEM_REQUIRED = 0x00000001
            // Legacy flag, should not be used.
            // ES_USER_PRESENT = 0x00000004
        }

        GetState getState;
        MakeAction makeAction;

        string appState = "Waiting";       

        int moveRange = 10;

        Thread doWorkThread;        

        public Form1()
        {            
            InitializeComponent();
            initState();                        
        }

        private void initState()
        {
            makeAction += PreventSleep;
            getState += () => this.appState;

            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.notifyIcon.Visible = true;
            this.notifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;            

            this.ShowInTaskbar = false;
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
        }

        private void NotifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Normal;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            appState = "Running";
            this.btnStart.Enabled = false;
            this.btnStop.Enabled = true;
            this.notifyIcon.Text = "ON";

            doWorkThread = new Thread(() =>
            {
                var currentState = this.Invoke(getState);

                Console.WriteLine(currentState);

                while ((string)currentState == "Running")
                {
                    Thread.Sleep(3000);                                       
                    currentState = this.Invoke(getState);                    
                    this.Invoke(makeAction);
                }
            });

            doWorkThread.Start();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            appState = "Waiting";
            this.btnStart.Enabled = true;
            this.btnStop.Enabled = false;
            this.notifyIcon.Text = "OFF";
        }

        void PreventSleep()
        {
            //refer to https://stackoverflow.com/questions/49045701/prevent-screen-from-sleeping-with-c-sharp            
            SetThreadExecutionState(EXECUTION_STATE.ES_DISPLAY_REQUIRED);
        }
    }
}
