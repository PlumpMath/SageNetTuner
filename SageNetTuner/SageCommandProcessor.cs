namespace SageNetTuner
{
    using System;
    using System.Collections.Generic;
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

        private IChannelProvider _channelProvider;

        public SageCommandProcessor(TunerElement tunerSettings, DeviceElement deviceSettings, ExecutableProcessCaptureManager executableProcessCaptureManager, IChannelProvider channelProvider)
        {
            _tunerSettings = tunerSettings;
            _deviceSettings = deviceSettings;
            Logger = LogManager.GetLogger(tunerSettings.Name);

            _executableProcessCapture = executableProcessCaptureManager;
            _channelProvider = channelProvider;
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
                    case "PROPERTIES":
                        response = GetEncoderProperties();
                        break;
                    case "STOP":
                        StopRecording();
                        break;
                    case "START":
                    case "BUFFER":
                        response = StartRecording(new StartCommand(commandArgs[1], commandArgs[3]));
                        break;
                    case "SWITCH":
                    case "BUFFER_SWITCH":
                        response = StartRecording(new StartCommand(commandArgs[0], commandArgs[1]));
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

        private string GetEncoderProperties(string encoderId, string port)
        {
            var props = new List<string>();
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/available_channels=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/brightness=-1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/broadcast_standard=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/contrast=-1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/device_name=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/hue=-1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/last_channel=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/saturation=-1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/sharpness=-1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_mode=Cable", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_plugin=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/tuning_plugin_port=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/video_crossbar_index=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/1/0/video_crossbar_type=1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/audio_capture_device_name=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/broadcast_standard=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/capture_config=2050", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/default_device_quality=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/delay_to_wait_after_tuning=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/encoder_merit=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/fast_network_encoder_switch=false", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/forced_video_storage_path_prefix=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/last_cross_index=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/last_cross_type=1", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/live_audio_input=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/multicast_host=", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/never_stop_encoding=false", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/video_capture_device_name={1}", encoderId, _tunerSettings.Name));
            props.Add(string.Format(@"mmc/encoders/{0}/video_capture_device_num=0", encoderId));
            props.Add(string.Format(@"mmc/encoders/{0}/video_encoding_params=Great", encoderId));

            var response = new StringBuilder();
            response.AppendLine(props.Count.ToString(CultureInfo.InvariantCulture));
            foreach (var prop in props)
            {
                response.AppendLine(prop);
            }
            response.AppendLine("OK");

            return response.ToString();

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

        private string StartRecording(StartCommand command)
        {

            Logger.Debug("StartRecording(): {0}", command);

            try
            {
                // Find the requested channel to get the URL
                var ch = (from x in _lineup.Channels where x.GuideNumber == command.Channel select x).FirstOrDefault();
                if (ch != null)
                {
                    Logger.Debug("StartRecording(): Found Requested Channel: GuideName={0}, GuideNumber={1}, URL={2}", ch.GuideName, ch.GuideNumber, ch.URL);

                    _executableProcessCapture.Start(ch, command.FileName);

                    Logger.Trace("StartRecording(): Recording Started");

                    return "OK";
                }
                else
                {
                    Logger.Warn("StartRecording(): Channel not found");
                    return string.Format("ERROR Channel not found in device lineup. {0}", command.Channel);
                }

            }
            catch (Exception ex)
            {
                Logger.Error("StartRecording(): Exception trying start recording", ex);
                return string.Format("ERROR {0}", ex.Message);
            }
        }

        private void StopRecording()
        {
            Logger.Debug("StopRecording:");
            this._executableProcessCapture.Stop();

        }


        public void Initialize()
        {

            _lineup = _channelProvider.GetLineup(_deviceSettings);

            if (_lineup==null)
                Logger.Warn("Channel Lineup not retrieved.");
            else if (_lineup.Channels.Count> 0)
                Logger.Info("Retrieved Channels: Count=[{0}]", _lineup.Channels.Count);


            
        }

        public void OnStopListening()
        {
            StopRecording();
        }
    }
}