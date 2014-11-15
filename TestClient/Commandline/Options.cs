namespace TestClient.Commandline
{
    using CommandLine;
    using CommandLine.Text;

    public class Options
    {

        [VerbOption("start", HelpText = "Start a Recording")]
        public StartSubOptions StartVerb { get; set; }

        [VerbOption("stop", HelpText = "Stop the current recording")]
        public StopSubOptions StopVerb { get; set; }

        [VerbOption("filesize", HelpText = "Get the file size of the current recording or a specfied file")]
        public FileSizeSubOptions FileSizeVerb { get; set; }

        [VerbOption("noop", HelpText = "Send a NOOP command, used to verify tuner is operating")]
        public NoopSubOptions NoopVerb { get; set; }

        [VerbOption("version", HelpText = "Send a VERSION command, returns version of the Network Tuner")]
        public VersionSubOptions VersionVerb { get; set; }


        [VerbOption("port", HelpText = "Send a PORT command, returns port tuner is listening on")]
        public PortSubOptions PortVerb { get; set; }

        [VerbOption("autoinfoscan", HelpText = "Send a AUTOINFOSCAN command, returns the list of available channels")]
        public AutoInfoScanOptions AutoInfoScanVerb { get; set; }


        [ParserState]
        public IParserState LastParserState { get; set; }


        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }
}