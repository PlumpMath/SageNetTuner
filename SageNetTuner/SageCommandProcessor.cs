namespace SageNetTuner
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Timers;

    using NLog;

    using SageNetTuner.Configuration;
    using SageNetTuner.Model;
    using SageNetTuner.Network;

    public class SageCommandProcessor
    {

        private readonly Logger Logger;
        private readonly TunerElement _tunerSettings;

        private readonly DeviceElement _deviceSettings;

        private Lineup _lineup;

        private readonly ExecutableProcessCaptureManager _executableProcessCapture;

        private readonly Timer _noActivtyTimer;

        public SageCommandProcessor(TunerElement tunerSettings, DeviceElement deviceSettings, ExecutableProcessCaptureManager executableProcessCaptureManager)
        {
            _tunerSettings = tunerSettings;
            _deviceSettings = deviceSettings;
            Logger = LogManager.GetLogger(tunerSettings.Name);

            this._executableProcessCapture = executableProcessCaptureManager;

            _noActivtyTimer= new Timer(TimeSpan.FromSeconds(30).TotalMilliseconds);

            _noActivtyTimer.Elapsed += NoActivtyTimerElapsed;
        }

        void NoActivtyTimerElapsed(object sender, ElapsedEventArgs e)
        {
            //Logger.Warn("No Activity within interval stopping recordings:  {0}", TimeSpan.FromMilliseconds(_noActivtyTimer.Interval));
            //StopRecording();
        }

        public string Name
        {
            get
            {
                return _tunerSettings.Name;
            }
        }

        public void OnError(TcpServer server, Exception e)
        {
            Logger.Error("TcpServer.OnError", e);
        }

        public void OnDataAvailable(TcpServerConnection connection)
        {
            Logger.Trace("TcpServer.OnDataAvailable: {0}", connection.ClientAddress.ToString());

            var client = connection.Socket;
            using (var stream = client.GetStream())
            using (var sw = new StreamWriter(stream))
            {

                if (stream.DataAvailable)
                {
                    var data = new byte[5013];

                    var stringBuilder = new StringBuilder();

                    try
                    {
                        do
                        {
                            var bytesRead = stream.Read(data, 0, data.Length);

                            Logger.Debug("{1}:{2} Bytes Received: {0}", bytesRead, connection.ClientAddress, _tunerSettings.ListenerPort);

                            stringBuilder.Append(Encoding.UTF8.GetString(data, 0, bytesRead));
                            string str = stringBuilder.ToString();

                            if (!string.IsNullOrEmpty(str) && str.Length > 2 && str.Substring(str.Length - 2) == "\r\n")
                            {
                                var response = HandleRequest(stringBuilder.ToString().Trim());
                                Logger.Debug("Sending:{0}", response);
                                sw.Write(response + Environment.NewLine);
                            }
                        }
                        while (client.Available > 0);
                    }

                    catch (IOException ex)
                    {
                        Logger.Warn("IOException while reading stream", ex);
                    }

                }
            }
        }

        public void OnConnect(TcpServerConnection connection)
        {
            Logger.Trace("TcpServer.OnConnect: {0}", connection.ClientAddress.ToString());
        }

        private string HandleRequest(string request)
        {

            if (_noActivtyTimer.Enabled)
            {
                // A command was received so reset the timer.
                _noActivtyTimer.Stop();
                _noActivtyTimer.Start();
            }

            Logger.Info("HandleRequest: [{0}]", request);
            var response = "OK";
            try
            {
                //START SageDCT-HDHomeRun Prime Tuner 131A21AF-1 Digital TV Tuner|752|2826835203582|D:\Recordings\PropertyBrothers-BeatrizBrandon-17756746-0.ts|Great
                var command = request.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
                var commandArgs = request.Replace(command, "").Trim().Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);

                if (Logger.IsTraceEnabled)
                {
                    Logger.Trace("  Command={0}", command);
                    Logger.Trace("  CommandArgs:");
                    for (int i = 0; i < commandArgs.Length; i++)
                    {
                        Logger.Trace("    {0}:{1}", i, commandArgs[i]);
                    }
                }

                switch (command)
                {
                    case "STOP":
                        StopRecording();
                        break;
                    case "START":
                    case "BUFFER":
                        StartRecording(new StartCommand(commandArgs[1], commandArgs[3]));
                        break;
                    case "SWITCH":
                    case "BUFFER_SWITCH":
                        StartRecording(new StartCommand(commandArgs[0], commandArgs[1]));
                        break;
                    case "GET_SIZE":
                        response = GetFileSize();
                        break;
                    case "FIRMWARE":
                        response = Assembly.GetEntryAssembly().GetName().Version.ToString();
                        break;
                    case "NOOP":
                        break;
                    case "GET_FILE_SIZE":
                        response = GetFileSize(commandArgs[0]).ToString(CultureInfo.InvariantCulture);
                        break;
                    case "TUNE":
                    case "AUTOTUNE":
                        break;
                    case "VERSION":
                        response = "2.1";
                        break;
                    case "PORT":
                        response = _tunerSettings.ListenerPort.ToString(CultureInfo.InvariantCulture);
                        break;
                    case "AUTOINFOSCAN":
                        var op = "";
                        if (commandArgs.Length > 0) 
                            op= commandArgs[0];
                        response = this.GetAvailableChannels(op);
                        break;
                    default:
                        Logger.Warn("Unknown Command: {0}", command);
                        response = "ERROR Unknown Command";
                        break;
                }

            }
            catch (Exception e)
            {
                response = string.Format("ERROR {0}", e.Message);

                Logger.Warn("Exception caught in HandleRequest", e);
            }
            Logger.Info("Response:[{0}]", response);

            return response;
        }

        private string GetAvailableChannels(string commandArg)
        {
            commandArg = commandArg.Trim();

            Logger.Info("GetAvailableChannels: {0}", commandArg);
            if (commandArg == "0")
            {
                var sb = new StringBuilder();
                foreach (var ch in _lineup.Channels)
                {
                    sb.AppendFormat("{0};", ch.GuideNumber);
                }
                return sb.ToString();
            }
            else
            {
                return "DONE";
            }
        }

        private string GetFileSize()
        {
            Logger.Debug("GetFileSize: Current Recording");
            return this._executableProcessCapture.GetFileSize().ToString(CultureInfo.InvariantCulture);
        }

        private long GetFileSize(string filename)
        {

            Logger.Debug("GetFileSize: Filename={0}", filename);
            if (File.Exists(filename))
            {
                try
                {
                    var fi = new FileInfo(filename);
                    fi.Refresh();
                    return fi.Length;
                }
                catch (Exception e)
                {
                    Logger.Warn(string.Format("Exception getting file size, returning 0: {0}", e.Message), e);
                    return 0;

                }
            }

            Logger.Warn("File does not exist, cannot get file size");
            return 0;
        }

        private void StartRecording(StartCommand command)
        {

            Logger.Debug("Start Recording: {0}", command);

            try
            {
                // Find the requested channel to get the URL
                var ch = (from x in _lineup.Channels where x.GuideNumber == command.Channel select x).FirstOrDefault();
                if (ch != null)
                {
                    Logger.Debug("Found Requested Channel: GuideName={0}, GuideNumber={1}, URL={2}", ch.GuideName, ch.GuideNumber, ch.URL);


                    this._executableProcessCapture.Start(ch, command.FileName);
                    _noActivtyTimer.Start();
                }



            }
            catch (Exception ex)
            {
                Logger.Error("Exception trying start recording", ex);
                throw;
            }
        }

        private void StopRecording()
        {
            _noActivtyTimer.Stop();

            Logger.Debug("StopRecording:");
            this._executableProcessCapture.Stop();

        }


        public void Initialize()
        {

            GetLineup();
        }

        private void GetLineup()
        {
            try
            {

                var url = string.Format("http://{0}/lineup.xml", _deviceSettings.Address);
                Logger.Debug("Getting Channel Lineup: {0}", url);
                var client = new WebClient();
                var channels = client.DownloadString(url);

                var lineup = XmlHelper.FromXml<Lineup>(channels);
                _lineup = lineup;

                Logger.Debug("  Found Channels: {0}", _lineup.Channels.Count);
            }
            catch (Exception e)
            {
                this.Logger.Warn(string.Format("  Exception reading channels:  Name={0}", this._tunerSettings.Name), e);
            }
        }

        public void OnStopListening()
        {
            StopRecording();
        }
    }
}