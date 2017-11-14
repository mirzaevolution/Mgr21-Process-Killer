using System;
using System.IO;
using System.Data;
using System.Linq;
using System.Timers;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;
using CoreModel;
using CoreSecurityLib;
using CoreSecurityLib.Security;
using CoreSecurityLib.Process;
using CoreSecurityLib.Common;
namespace Mgr21CoreSvc
{
    public partial class Mgr21CoreService : ServiceBase
    {
        private Timer _timer;
        private IOSecurity _ioSecurity;
        private List<ProcessModel> _processList;
        private EventLog _log;
        private bool _isStarted;
        private Settings _settings;
        public Mgr21CoreService()
        {
            InitializeComponent();
            _log = new EventLog();
            if (!EventLog.SourceExists("Prockillersrc"))
                EventLog.CreateEventSource("Prockillersrc", "Mgr21log");
            _log.Source = "Prockillersrc";
            _log.Log = "Mgr21log";
            _timer = new Timer();
            _timer.Elapsed += OnTimerElapsed;
            _ioSecurity = new IOSecurity();
            _processList = new List<ProcessModel>();
        }
        #region Sync
        private List<ProcessModel> RetrieveList()
        {
            var dataResult = _ioSecurity.RetrieveData();
            if(!dataResult.MainResult.Success)
            {
                _log.WriteEntry($"Error (RetrieveList): {dataResult.MainResult.ErrorMessage}",EventLogEntryType.Error);
                return new List<ProcessModel>();
            }
            return dataResult.Data;
        }
        private Settings RetrieveSettings()
        {
            var result = Config.RetrieveSettings();
            if(!result.MainResult.Success)
            {
                _log.WriteEntry($"Error (RetrieveSettings): {result.MainResult.ErrorMessage}", EventLogEntryType.Error);
                return new Settings
                {
                    ComparisonType = ComparisonType.ByName,
                    Interval = 1000
                };
            }
            return result.Data;
        }
        #endregion
        #region Core Operations
        private void Executor(ProcessModel source, ProcessModel target, Process processToKill)
        {
            try
            {
                if (Checker.CompareProcess(source, target))
                {
                    if (!processToKill.HasExited) //to avoid race condition without locking
                        processToKill.Kill();
                }
            }
            catch(Exception ex)
            {
                _log.WriteEntry($"Error (ProcessKiller): {ex.Message}", EventLogEntryType.Error);
            }
            
        }
        private void FindByName(string processName)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                try
                {
                    if(!process.HasExited) //to avoid race condition without locking
                        process.Kill();
                }
                catch (Exception ex)
                {
                    _log.WriteEntry($"Error (ProcessKiller): {ex.Message}", EventLogEntryType.Error);
                }
            }
        }
        private void FindAll(ProcessModel model)
        {
            foreach (Process process in Process.GetProcesses())
            {
                byte[] buffer = new byte[2048];
                long length = 0;
                try
                {
                    using (FileStream fs = File.OpenRead(process.MainModule.FileName))
                    {
                        length = fs.Length;
                        if (length < buffer.Length)
                            buffer = new byte[length];
                        fs.Read(buffer, 0, buffer.Length);
                    }
                }
                catch
                {
                    buffer = null;
                }
                if (buffer != null)
                {
                    //we copy all those variables, and handle them to task!
                    ProcessModel source = model;
                    ProcessModel target = new ProcessModel
                    {
                        FileLength = length,
                        FileLocation = process.MainModule.FileName,
                        ProcessName = process.ProcessName,
                        InitialBytes = buffer
                    };
                    Process targetProcess = process;
                    Task.Run(() => Executor(source, target, targetProcess));
                }
            }

        }
        private void BasicHandler()
        {
            foreach (string name in _processList.Select(x => x.ProcessName))
            {
                foreach (Process process in Process.GetProcessesByName(name))
                {
                    try
                    {
                        process.Kill();
                    }
                    catch (Exception ex)
                    {
                        _log.WriteEntry($"Error (ProcessKiller): {ex.Message}", EventLogEntryType.Error);
                    }
                }
            }
        }
        private void AdvancedHandler()
        {
            foreach (ProcessModel model in _processList)
            {
                ProcessModel copy = model;
                if (Environment.ProcessorCount > 2)
                {
                    Task.Run(() => FindByName(copy.ProcessName));
                    Task.Run(() => FindAll(copy));
                }
                else
                {
                    FindByName(copy.ProcessName);
                    FindAll(copy);
                }
            }
        }
        private void HandleProcesses()
        {
            if (_processList != null)
            {
                if (_processList.Count > 0)
                {
                    switch(_settings.ComparisonType)
                    {
                        case ComparisonType.ByName:
                            BasicHandler();
                            break;
                        case ComparisonType.By2KBInitialBytes:
                            AdvancedHandler();
                            break;
                    }
                }
            }
        }
        private void OnTimerElapsed(object sender, ElapsedEventArgs e)
        {
            HandleProcesses();
        }
        private void Loaded()
        {
            if (File.Exists(Global.DataFileLocation))
            {
                _processList = RetrieveList();
                _settings = RetrieveSettings();
                if(_timer.Interval!=_settings.Interval)
                    _timer.Interval = _settings.Interval;
                _isStarted = true;
                _timer.Start();

            }
        }
        private void Stopped()
        {
            if (_isStarted)
            {
                _isStarted = false;
                _timer.Stop();
            }
        }
        #endregion
        #region Service Methods
        protected override void OnStart(string[] args)
        {
            _log.WriteEntry($"Service started at: {DateTime.Now}",EventLogEntryType.Information);
            Loaded();
        }
        protected override void OnPause()
        {
            _log.WriteEntry($"Service paused at: {DateTime.Now}", EventLogEntryType.Information);
            Stopped();
        }
        protected override void OnContinue()
        {
            _log.WriteEntry($"Service continued at: {DateTime.Now}", EventLogEntryType.Information);
            //after paused, we need to check our list again.
            Loaded();
        }
        protected override void OnStop()
        {
            _log.WriteEntry($"Service stopped at: {DateTime.Now}", EventLogEntryType.Information);
            Stopped();
        }
        #endregion
    }
}
