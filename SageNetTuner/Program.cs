﻿namespace SageNetTuner
{
    using System;
    using System.Reflection;

    using Autofac;

    using NLog;

    using SageNetTuner.Contracts;
    using SageNetTuner.Model;

    using Topshelf;
    using Topshelf.Autofac;

    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {


            var container = BuildContainer();

            return (int)HostFactory.Run(
                config =>
                {
                    config.UseNLog();
                    config.UseAutofacContainer(container);
                    config.RunAsLocalSystem();
                    config.SetDescription("SageTV Network Tuner Service");
                    config.SetDisplayName("SageNetTuner");
                    config.SetServiceName("SageNetTuner");


                    config.Service<NetworkTunerService>(
                        s =>
                        {
                            s.ConstructUsingAutofacContainer();
                            s.WhenStarted(
                                (service, control) =>
                                {
                                    var assembly = Assembly.GetExecutingAssembly();
                                    Logger.Info("SageNetTuner v{0}", assembly.GetName().Version);
                                    return service.Start(control);

                                });
                            s.WhenStopped((service, control) =>
                                {
                                    Logger.Trace("SageNetTuner Stopped Succesfully");
                                    return service.Stop(control);
                                });
                        });


                });
        }

        private static IContainer BuildContainer()
        {
            var builder = new ContainerBuilder();

            builder.RegisterInstance(Configuration.SageNetTunerSection.Settings);

            builder.RegisterType<NetworkTunerService>();
            builder.RegisterType<Providers.HDHomeRunChannelProvider>().As<IChannelProvider>();
            builder.RegisterType<ExecutableProcessCaptureManager>().As<ICaptureManager>();
            builder.RegisterType<RequestParser>().As<IRequestParser>();
            builder.RegisterType<SageCommandProcessor>();

            return builder.Build();
        }


    }
}
