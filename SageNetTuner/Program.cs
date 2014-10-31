namespace SageNetTuner
{
    using System;

    using Topshelf;

    public class Program
    {
        static int Main(string[] args)
        {

            var settings = Configuration.SageNetTunerSection.Settings;

            return (int)HostFactory.Run(
                config =>
                {

                    config.Service(hostsettings => new NetworkTunerService(settings), s =>
                    {
                        s.BeforeStartingService(_ => Console.WriteLine("BeforeStart"));
                        s.BeforeStoppingService(_ => Console.WriteLine("BeforeStop"));
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
