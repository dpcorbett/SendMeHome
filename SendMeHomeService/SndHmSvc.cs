using SharpTools.Log;
using SharpTools.Network;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;

namespace SendMeHomeService
{
    public partial class SndHmSvc : ServiceBase
    {
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        // Create logger.
        private static readonly log4net.ILog _log = SharpToolsLog.GetLogger();

        public SndHmSvc()
        {
            InitializeComponent();

            eventLogMain = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("SendMeHome"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "SendMeHome", "SMHLog");
            }
            eventLogMain.Source = "SendMeHome";
            eventLogMain.Log = "SMHLog";
        }

        protected override void OnStart(string[] args)
        {
            eventLogMain.WriteEntry("Starting...");
            _log.Debug("Starting...");
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            _Pulse();
            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(ServiceHandle, ref serviceStatus);

        }

        protected override void OnStop()
        {
            eventLogMain.WriteEntry("Stopping...");
            _log.Debug("Stopping...");
        }

        protected override void OnContinue()
        {
            eventLogMain.WriteEntry("Continuing...");
            _log.Debug("Stopping...");
        }

        private void _Pulse()
        {
            // Set up a timer to trigger every minute.
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 60000; // 60 seconds
            timer.Elapsed += new System.Timers.ElapsedEventHandler(this.OnTimer);
            timer.Start();
        }

        public void OnTimer(object sender, System.Timers.ElapsedEventArgs args)
        {
            string strExtIP = SharpToolsNetwork.GetPublicIP();
            // TODO: Insert monitoring activities here.
            eventLogMain.WriteEntry("Monitoring the System", EventLogEntryType.Information);
            _log.Debug("Monitoring the System");
            eventLogMain.WriteEntry(strExtIP);
            _log.Debug(strExtIP);
        }

    }
}
