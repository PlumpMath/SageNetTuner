namespace TestClient.Commandline
{
    using CommandLine;

    public class FileSizeSubOptions : SubOptionsBase
    {
        [Option('f',Required = false, DefaultValue = "")]
        public string Filename { get; set; }

    }
}