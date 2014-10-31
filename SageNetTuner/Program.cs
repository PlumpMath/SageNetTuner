namespace SageNetTuner
{
    using System;
    using System.Reflection;

    using NLog;

    using Topshelf;

    public class Program
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static int Main(string[] args)
        {

            var settings = Configuration.SageNetTunerSection.Settings;

            return (int)HostFactory.Run(
                config =>
                {

                    config.Service(hostsettings => new NetworkTunerService(settings), s =>
                    {
                        s.BeforeStartingService(
                            _ =>
                                {
                                    var assembly = Assembly.GetExecutingAssembly();
                                    Logger.Info("SageNetTuner v{0}", assembly.GetName().Version);
                                });
                        s.AfterStoppingService(_ => Logger.Trace("SageNetTuner Stopped Succesfully"));
                    });

                    config.RunAsLocalSystem();
                    config.SetDescription("SageTV Network Tuner Service");
                    config.SetDisplayName("SageNetTuner");
                    config.SetServiceName("SageNetTuner");

                    config.UseNLog();

                });
        }

    }
}
