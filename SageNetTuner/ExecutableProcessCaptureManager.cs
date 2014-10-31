
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

        private readonly Logger Logger;

        private Logger _stdOutLogger;

        private Process _currentProcess;

        private FileStream _recordingFileStream;

        private object[] _replacmentParams;

        private CommandElement _currentCommand;

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


            if (!File.Exists(startCommand.Path))
            {
                var msg = string.Format("Cannot find capture executable {0}", startCommand.Path);
                throw new InvalidOperationException(msg);
            }

            Logger.Debug("PathToExecutable={0}", startCommand.Path);
            Logger.Debug("CommandLineFormat={0}", startCommand.CommandLineFormat);

            LookForExistingProcessForThisTuner(startCommand.Path);


        }

        private void Cleanup()
        {
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

        private void LookForExistingProcessForThisTuner(string executableName)
        {
            if (_currentProcess == null)
            {
                var processes = Process.GetProcessesByName(executableName);

                foreach (var process in processes)
                {

                    if (process.MainWindowTitle.Contains(_tuner.Name))
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

            _stdOutLogger = LogManager.GetLogger(string.Format("stdout-{0}", Path.GetFileName(filename)));

            //var args = string.Format("-i {0} -metadata comment=\"Tuner: {3}\" {1} \"{2}\"", url, this._commandLineFormat, filename, _tunerName);

            _replacmentParams = new object[10];
            _replacmentParams[0] = channel.URL;
            _replacmentParams[1] = filename;
            _replacmentParams[2] = Path.GetFileNameWithoutExtension(filename);
            _replacmentParams[3] = Path.GetExtension(filename);
            _replacmentParams[4] = Path.GetDirectoryName(filename);
            _replacmentParams[5] = _tuner.Name;
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

                Logger.Debug("Starting: [{0} {1}]", startCommand.Path, args);

                _currentProcess = StartProcess(CommandEvent.Start, startCommand, args);

                if (_currentProcess != null)
                {
                    Thread.Sleep(10);
                    if (_currentProcess.MainWindowHandle != IntPtr.Zero)
                        SetWindowText(_currentProcess.MainWindowHandle, string.Format("SageNetTuner: {0}", _tuner.Name));

                    Logger.Debug("  {0} started: PID={1}, WindowTitle=[{2}]", _currentProcess.MainModule.FileName, _currentProcess.Id, _currentProcess.MainWindowTitle);

                    Filename = filename;

                }
                else
                {
                    throw new ApplicationException(string.Format("Could not start {0}.exe", startCommand.Path));
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

            Logger.Info("StartProcess(): {0} command running. [{1} {2}]", commandEvent, command.Path, args);

            _currentCommand = command;

            var process = new Process();
            process.StartInfo.FileName = command.Path;
            process.StartInfo.Arguments = args;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.WindowStyle=ProcessWindowStyle.Hidden;

            process.Start();

            if (process != null)
            {
                Logger.Info("StartProcess(): started new process. ({1}) {0}, ", command.Path, process.Id);
                process.ErrorDataReceived += DataReceivedHandler;
                process.OutputDataReceived += DataReceivedHandler;
                process.EnableRaisingEvents = true;
                process.Exited += ProcessExitedHandler;

                try
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                }
                catch (Exception e)
                {
                    Logger.Warn("StartProcess(): Exception trying to capture stdout/stderr", e);
                    process.ErrorDataReceived -= DataReceivedHandler;
                    process.OutputDataReceived -= DataReceivedHandler;
                }

            }
            else
            {
                throw new Exception(string.Format("StartProcess(): Could not start {0}", command.Path));
            }
            return process;
        }

        void ProcessExitedHandler(object sender, EventArgs e)
        {
            var proc = (Process)sender;
            Logger.Info("Process {0} has exited with code {1}.  Runtime={2}", proc.Id, proc.ExitCode, (proc.ExitTime - proc.StartTime));

        }

        private void DataReceivedHandler(object sender, DataReceivedEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                _stdOutLogger.Trace(e.Data);
            }
        }

        public void Stop()
        {

            var executableName = Path.GetFileNameWithoutExtension(_currentCommand.Path);

            if (_currentProcess == null)
            {
                Logger.Debug("No currrent {0} process, nothing to Stop", executableName);
                return;
            }

            if (_currentProcess.HasExited)
            {
                Logger.Debug("Current {0} process has already exited", executableName);
                Cleanup();
                return;
            }

            ExecuteEventCommands(CommandEvent.BeforeStop, _replacmentParams);

            try
            {
                Logger.Debug("Stopping ({1}) {0}", executableName, _currentProcess.Id);


                if (_currentProcess.MainWindowHandle != IntPtr.Zero)
                {
                    _currentProcess.CloseMainWindow();
                    _currentProcess.WaitForExit(5000);
                }

                if (!_currentProcess.HasExited)
                    _currentProcess.Kill();

                if (Logger.IsDebugEnabled)
                {
                    Logger.Debug("Recording Stats: ");
                    Logger.Debug("  Filename: {0}", Filename);
                    Logger.Debug("  Time: {0}", (_currentProcess.ExitTime - _currentProcess.StartTime));
                    Logger.Debug("  Size: {0:G}", GetFileSize());
                }

                Filename = "";
                Logger.Info("{0} stopped", executableName);
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
