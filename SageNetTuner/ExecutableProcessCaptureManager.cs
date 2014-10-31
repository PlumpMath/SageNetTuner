
namespace SageNetTuner
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Threading;
    using System.Timers;

    using NLog;

    using SageNetTuner.Configuration;
    using SageNetTuner.Model;

    public class ExecutableProcessCaptureManager : ICaptureManager
    {
        private readonly EncoderElement _encoder;

        private readonly TunerElement _tuner;

        private Logger Logger;

        private Logger StdOutLogger;

        private readonly string _executable;

        private readonly string _pathToExecutable;

        private Process _currentProcess;

        private readonly string _tunerName;

        private readonly Stopwatch _recordingStopwatch;

        private readonly System.Timers.Timer _timer;

        private FileStream _recordingFileStream;

        private object[] _replacmentParams;

        public string Filename { get; private set; }


        [DllImport("User32.dll")]
        public static extern bool SetWindowText(IntPtr hwnd, string title);

        public ExecutableProcessCaptureManager(EncoderElement encoder, TunerElement tuner)
        {
            _encoder = encoder;
            _tuner = tuner;

            CommandElement startCommand = null;
            if (!string.IsNullOrEmpty(encoder.Path))
            {

                //  Since they defined a Path/CommandLineFormat on the element 
                //  we'll use it instead of one that might have been defined in the Commands
                startCommand = encoder.Commands.FirstOrDefault(c => c.Event == CommandEvent.Start);
                if (startCommand != null)
                {
                    encoder.Commands.Remove(startCommand);
                }
                startCommand = new CommandElement()
                                   {
                                       Name = "__StartRecording",
                                       Event = CommandEvent.Start,
                                       Path = encoder.Path,
                                       CommandLineFormat = encoder.CommandLineFormat,
                                   };
                encoder.Commands.Add(startCommand);

            }
            else
            {
                if (!encoder.Commands.Any())
                {
                    throw new InvalidOperationException(string.Format("No commands defined for this encoder: name={0}", encoder.Name));
                }


                if (encoder.Commands.All(c => c.Event != CommandEvent.Start))
                {
                    throw new InvalidOperationException(string.Format("No START event commands defined for this encoder: name={0}", encoder.Name));
                }

                startCommand = encoder.Commands.FirstOrDefault(c => c.Event == CommandEvent.Start);

            }

            Logger = LogManager.GetLogger(tuner.Name + ":" + _encoder.Name);

            _executable = Path.GetFileNameWithoutExtension(startCommand.Path);

            if (!File.Exists(startCommand.Path))
            {
                var msg = string.Format("Cannot find capture executable {0}", startCommand.Path);
                throw new InvalidOperationException(msg);
            }

            _executable = Path.GetFileNameWithoutExtension(startCommand.Path);
            _commandLineFormat = startCommand.CommandLineFormat;

            Logger.Debug("PathToExecutable={0}", startCommand.Path);
            Logger.Debug("CommandLineFormat={0}", startCommand.CommandLineFormat);


            _tunerName = tuner.Name;

            LookForExistingProcessForThisTuner();

            _timer = new System.Timers.Timer(TimeSpan.FromSeconds(1).TotalMilliseconds);
            _timer.Elapsed += TimerOnElapsed;
            _recordingStopwatch = new Stopwatch();

        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            if (_currentProcess == null)
            {
                Cleanup();
                return;
            }

            if (_currentProcess.HasExited)
            {
                Logger.Warn("{0} process has exited unexpectedly.  ExitCode: {1}", _executable, _currentProcess.ExitCode);
                Cleanup();
            }

        }

        private void Cleanup()
        {
            _recordingStopwatch.Stop();
            _timer.Stop();

            if (_currentProcess != null)
            {
                Logger.Debug("Cleanup");

                _currentProcess.Dispose();
                _currentProcess = null;

            }

            try
            {
                if (_recordingFileStream != null)
                    _recordingFileStream.Dispose();
                _recordingFileStream = null;
            }
            catch (Exception e)
            {
                Logger.Error("Error cleaning up Filestream: {0}", e.Message);
            }

        }

        private void LookForExistingProcessForThisTuner()
        {
            if (_currentProcess == null)
            {
                var processes = Process.GetProcessesByName(_executable);

                foreach (var process in processes)
                {

                    if (process.MainWindowTitle.Contains(_tunerName))
                    {
                        _currentProcess = process;
                        return;
                    }
                }

                _currentProcess = null;
            }
        }

        public void Start(Channel channel, string filename)
        {

            if (_currentProcess != null)
            {
                //throw new InvalidOperationException("Process is already started, cannot start again");
                Stop();
            }

            if (string.IsNullOrEmpty(channel.URL))
            {
                throw new ArgumentNullException("url");
            }

            if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentNullException("filename");
            }


            var path = Path.GetDirectoryName(filename);
            if (!Directory.Exists(path))
            {
                throw new InvalidOperationException(string.Format("Path requested does not exist, cannot continue: {0}", path));
            }

            if (File.Exists(filename))
            {
                File.Delete(filename);
            }

            Filename = filename;

            StdOutLogger = LogManager.GetLogger(string.Format("stdout-{0}", filename));

            //var args = string.Format("-i {0} -metadata comment=\"Tuner: {3}\" {1} \"{2}\"", url, this._commandLineFormat, filename, _tunerName);

            _replacmentParams = new object[10];
            _replacmentParams[0] = channel.URL;
            _replacmentParams[1] = filename;
            _replacmentParams[2] = Path.GetFileNameWithoutExtension(filename);
            _replacmentParams[3] = Path.GetExtension(filename);
            _replacmentParams[4] = Path.GetDirectoryName(filename);
            _replacmentParams[5] = _tunerName;
            _replacmentParams[6] = channel.GuideName;
            _replacmentParams[7] = channel.GuideNumber;
            _replacmentParams[8] = Guid.NewGuid().ToString();
            _replacmentParams[9] = DateTime.Now;


            ExecuteEventCommands(CommandEvent.BeforeStart, _replacmentParams);

            // --- START executable process to begin recording

            var startCommand = _encoder.Commands.FirstOrDefault(c => c.Event == CommandEvent.Start);
            if (startCommand != null)
            {
                var args = string.Format(startCommand.CommandLineFormat, _replacmentParams);

                Logger.Debug("Starting: [{0} {1}]", this._pathToExecutable, args);

                _currentProcess=StartProcess(CommandEvent.Start, startCommand, args);

                if (_currentProcess != null)
                {
                    Thread.Sleep(10);
                    if (_currentProcess.MainWindowHandle != IntPtr.Zero)
                        SetWindowText(_currentProcess.MainWindowHandle, string.Format("HDHRNetworktuner: {0}", _tunerName));

                    Logger.Debug("  {0} started: PID={1}, WindowTitle=[{2}]", _executable, _currentProcess.Id, _currentProcess.MainWindowTitle);

                    Filename = filename;
                    _recordingStopwatch.Start();
                    _timer.Start();

                }
                else
                {
                    throw new ApplicationException(string.Format("Could not start {0}.exe", _executable));
                }

                ExecuteEventCommands(CommandEvent.AfterStart, _replacmentParams);
            }
        }


        private void ExecuteEventCommands(CommandEvent commandEvent, object[] replacmentParams)
        {
            var commands = _encoder.Commands.Where(c => c.Event == commandEvent).ToList();
            foreach (var command in commands)
            {

                if (!File.Exists(command.Path))
                {
                    throw new InvalidOperationException(string.Format("{0} command path does not exist:  path={1}", commandEvent, command.Path));
                }

                var args = string.Format(command.CommandLineFormat, replacmentParams);


                Logger.Info("{0} command running. [{1} {2}]", commandEvent, command.Path, args);

                var process = StartProcess(commandEvent, command, args);

                if (process != null)
                {
                    process.WaitForExit((int)TimeSpan.FromSeconds(15).TotalMilliseconds);

                    if (!process.HasExited)
                    {
                        process.Kill();
                        process.WaitForExit((int)TimeSpan.FromSeconds(15).TotalMilliseconds);
                        throw new ApplicationException(string.Format("{0} command did not exit within timeout: name={1}, timeout=15 seconds", commandEvent, command.Name));
                    }

                    if (process.ExitCode != 0)
                    {
                        throw new ApplicationException(string.Format("{0} command failed: name={1}", commandEvent, command.Name));
                    }
                }
            }

        }

        private Process StartProcess(CommandEvent commandEvent, CommandElement command, string args)
        {

            Logger.Info("{0} command running. [{1} {2}]", commandEvent, command.Path, args);

            var process = new Process
            {
                StartInfo =
                {
                    FileName = command.Path,
                    Arguments = args,
                    WorkingDirectory = "",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true,
                    RedirectStandardInput = true
                }
            };
            process.ErrorDataReceived += DataReceivedHandler;
            process.OutputDataReceived += DataReceivedHandler;
            process.EnableRaisingEvents = true;
            process.Exited += ProcessExitedHandler;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Logger.Info("Run(): started new process {0}", _currentProcess.Id);

            return process;
        }

        void ProcessExitedHandler(object sender, EventArgs e)
        {
            var proc = (Process)sender;
            Logger.Info("Process {0} has exited with code {1}", proc.Id, proc.ExitCode);

        }

        private void DataReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                StdOutLogger.Trace(e.Data);
            }
        }

        public void Stop()
        {

            //stop checking for executable running.
            _timer.Stop();


            if (_currentProcess == null)
            {
                Logger.Debug("No currrent {0} process, nothing to Stop", _executable);
                return;
            }

            if (_currentProcess.HasExited)
            {
                Logger.Debug("Current {0} process has already exited", _executable);
                Cleanup();
                return;
            }

            ExecuteEventCommands(CommandEvent.BeforeStop, _replacmentParams);

            try
            {
                Logger.Debug("Stopping {0}", _executable);

                _recordingStopwatch.Stop();

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Recording Stats: ");
                    Logger.Debug("  Filename: {0}", Filename);
                    Logger.Debug("  Time: {0}", _recordingStopwatch.Elapsed);
                    Logger.Debug("  Size: {0:G}", GetFileSize());
                }

                Filename = "";
                Logger.Info("{0} stopped", _executable);
            }
            catch (Exception e)
            {
                Logger.Warn("Exception stopping capture executable", e);
            }
            finally
            {
                Cleanup();
            }

            ExecuteEventCommands(CommandEvent.Stop, _replacmentParams);

            ExecuteEventCommands(CommandEvent.AfterStop, _replacmentParams);

        }

        public long GetFileSize()
        {

            if (string.IsNullOrEmpty(Filename))
            {
                Logger.Warn("  No Filename, cannot get file size.");
                return 0;
            }

            if (!File.Exists(Filename))
            {
                Logger.Warn("  File does not exist: {0}", Filename);
                return 0;
            }

            try
            {

                if (_recordingFileStream == null)
                    _recordingFileStream = File.Open(Filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                return _recordingFileStream.Length;

            }
            catch (Exception e)
            {
                Logger.Warn(string.Format("Exception reading file size, {0}", Filename), e);
                return 0;
            }
        }

    }
}
