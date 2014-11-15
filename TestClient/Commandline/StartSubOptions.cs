namespace TestClient.Commandline
{
    using System;

    using CommandLine;

    public class StartSubOptions : SubOptionsBase
    {
        [Option('c', HelpText = "Channel to tune", Required = true)]
        public string Channel { get; set; }

        [Option('f', HelpText = "Filename to save recording.  Local to tuner service", Required = true)]
        public string Filename { get; set; }

        [Option('t', HelpText = "Tuner name", DefaultValue = "Tuner", Required = false)]
        public string Tuner { get; set; }

        [Option('q', HelpText = "Recording Quality", DefaultValue = "GREAT", Required = false)]
        public string Quality { get; set; }
    }
}