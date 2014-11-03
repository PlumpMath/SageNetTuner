namespace SageNetTuner
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Timers;

    using NLog;

    using SageNetTuner.Configuration;
    using SageNetTuner.Contracts;
    using SageNetTuner.Filters;
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
            try
            {

                var context = new RequestContext(request)
                                  {
                                      Settings =
                                          {
                                              Device = _deviceSettings,
                                              Tuner = _tunerSettings,
                                              Lineup = _lineup,
                                          }
                                  };

                var response = new CommandPipeline(context, _executableProcessCapture).Execute();

                return response;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception handling request", ex);
                return "ERROR " + ex.Message;
            }

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

            _executableProcessCapture.Stop();

            //var x = new StopFilter(_executableProcessCapture, Logger);
            //x.Execute(new RequestContext(CommandName.Stop, new string[0]), null);

        }
    }
}