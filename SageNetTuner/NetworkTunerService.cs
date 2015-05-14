namespace SageNetTuner
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Sockets;
    using System.Runtime.InteropServices;
    using System.Text;

    using Autofac;

    using NLog;

    using SageNetTuner.Configuration;
    using SageNetTuner.Contracts;
    using SageNetTuner.Network;

    using Topshelf;

    public class NetworkTunerService : ServiceControl
    {

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly SageNetTunerSection _settings;

        private readonly List<TcpServer> _servers;

        private TcpServer _discoveryServer;

        private readonly List<SageCommandProcessor> _processors;

        private readonly ILifetimeScope _lifetimeScope;

        public NetworkTunerService(SageNetTunerSection settings, ILifetimeScope lifetimeScope)
        {

            _settings = settings;
            _lifetimeScope = lifetimeScope;

            _servers = new List<TcpServer>();
            _processors = new List<SageCommandProcessor>();


        }


        public bool Start(HostControl hostControl)
        {

            Logger.Info("Starting...");
            foreach (var device in _settings.Devices)
            {
                foreach (var tuner in device.Tuners)
                {
                    //var device = _settings.Devices[tuner.Device];

                    Logger.Info("Tuner: Name={0}, Enabled={3}, Device={1}, Port={2}", tuner.Name, device, tuner.ListenerPort, tuner.Enabled);

                    if (tuner.Enabled)
                    {
                        var logger = LogManager.GetLogger(tuner.Name);
                        var encoder = _settings.CaptureProfiles[tuner.Encoder];

                        //Create a LifetimeScope for this tuner
                        var innerScope = _lifetimeScope.BeginLifetimeScope(
                            "tuner",
                            builder =>
                            {
                                builder.RegisterInstance(logger);
                                builder.RegisterInstance(tuner);
                                builder.RegisterInstance(device);
                                builder.RegisterInstance(encoder);
                                builder.RegisterInstance(GetChannelProvider(device)).As<IChannelProvider>();
                            });

                        //Get a Processor from the innerScope
                        var p = innerScope.Resolve<SageCommandProcessor>();

                        p.Initialize();
                        _processors.Add(p);


                        var t = new TcpServer { Port = tuner.ListenerPort };
                        t.OnConnect += p.OnConnect;
                        t.OnDataAvailable += p.OnDataAvailable;
                        t.OnError += p.OnError;

                        t.Open();

                        _servers.Add(t);

                        hostControl.RequestAdditionalTime(TimeSpan.FromSeconds(5));
                    }
                }
            }


            if (_settings.EnableDiscovery)
            {
                Logger.Info("Starting Discovery Server on Port 8167");
                _discoveryServer = new TcpServer { Port = 8167 };

                _discoveryServer.OnConnect += DiscoveryServerOnConnect;
                _discoveryServer.OnDataAvailable += DiscoveryServerOnDataAvailable;
                _discoveryServer.Open();
            }
            else
            {
                Logger.Info("Discovery Server not enabled.");
            }
            return true;
        }

        private IChannelProvider GetChannelProvider(DeviceElement device)
        {
            var ch = device.ChannelProvider;
            var typeName = _settings.ChannelProviders[ch].Type;

            Logger.Debug("Creating ChannelProvider:  [{0}] {1}", ch, typeName);

            var channelProviderType = Type.GetType(typeName);

            IChannelProvider channelProvider = null;
            if (channelProviderType == null)
                Logger.Warn("Could get ChannelProvider [{0}]  Type:{1}", ch, typeName);
            else
                channelProvider = (IChannelProvider)Activator.CreateInstance(channelProviderType);

            if (channelProvider != null)
                Logger.Info("Created ChannelProvider:[{0}] ", channelProvider.GetType().FullName);

            return channelProvider;
        }

        public void DiscoveryServerOnConnect(TcpServerConnection connection)
        {
            Logger.Trace("DiscoveryServer.OnConnect: {0}", connection.ClientAddress.ToString());
        }

        public void DiscoveryServerOnDataAvailable(TcpServerConnection connection)
        {
            Logger.Trace("DiscoveryServer.OnDataAvailable: {0}", connection.ClientAddress.ToString());

            var client = connection.Socket;
            using (var stream = client.GetStream())
            using (var sw = new StreamWriter(stream))
            {

                if (stream.DataAvailable)
                {
                    var data = new byte[5013];

                    try
                    {
                        var bytesRead = stream.Read(data, 0, data.Length);

                        Logger.Debug("{1} Bytes Received: {0}", bytesRead, connection.ClientAddress);

                        var request = new EncoderRequest();
                        request.Deserialize(ref data);

                        foreach (var device in _settings.Devices)
                        {
                            foreach (var tuner in device.Tuners)
                            {

                                if (tuner.Enabled)
                                {
                                    var response = new EncoderResponse
                                                       {
                                                           Name = tuner.Name,
                                                           Length = tuner.Name.Length,
                                                           Prefix = request.Prefix,
                                                           Version = "210",
                                                           Port = (uint)tuner.ListenerPort
                                                       };

                                    Logger.Debug("Sending:{0}", response);
                                    sw.Write(response);
                                }
                            }
                        }

                        connection.ForceDisconnect();
                    }

                    catch (IOException ex)
                    {
                        Logger.Warn("IOException while reading stream", ex);
                    }

                }
            }
        }

        public struct EncoderRequest
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Prefix;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Version;

            // this method will deserialize a byte array into the struct.
            public void Deserialize(ref byte[] data)
            {
                var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                this = (EncoderRequest)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(EncoderRequest));
                gch.Free();
            }

        }

        public struct EncoderResponse
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Prefix;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 3)]
            public string Version;

            public uint Port;

            public int Length;

            public string Name;

            // Calling this method will return a byte array with the contents
            // of the struct ready to be sent via the tcp socket.
            public byte[] Serialize()
            {
                // allocate a byte array for the struct data
                var buffer = new byte[Marshal.SizeOf(typeof(EncoderResponse))];

                // Allocate a GCHandle and get the array pointer
                var gch = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                var pBuffer = gch.AddrOfPinnedObject();

                // copy data from struct to array and unpin the gc pointer
                Marshal.StructureToPtr(this, pBuffer, false);
                gch.Free();

                return buffer;
            }

            // this method will deserialize a byte array into the struct.
            public void Deserialize(ref byte[] data)
            {
                var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
                this = (EncoderResponse)Marshal.PtrToStructure(gch.AddrOfPinnedObject(), typeof(EncoderResponse));
                gch.Free();
            }
        }

        public bool Stop(HostControl hostControl)
        {

            Logger.Info("Stopping TCPServers");

            foreach (var server in this._servers)
            {
                try
                {
                    Logger.Debug("Server: {0}", server.Port);
                    server.Close();
                }
                catch (Exception e)
                {
                    Logger.Error("Exception shutting down TCPServer", e);
                }
            }

            Logger.Info("Stopping SageCommandProcessors");
            foreach (var processor in _processors)
            {
                try
                {
                    Logger.Debug("Processor: {0}", processor.Name);
                    processor.StopListening();
                }
                catch (Exception e)
                {
                    Logger.Error("Exception shutting down SageCommandProcessor", e);
                }
            }

            Logger.Info("Stopping Discovery Server");
            if (_discoveryServer != null)
            {
                try
                {
                    _discoveryServer.Close();
                }
                catch (Exception e)
                {
                    Logger.Error("Exception shutting down Discovery Server", e);
                }
            }

            Logger.Info("Stopped");
            return true;
        }


    }
}