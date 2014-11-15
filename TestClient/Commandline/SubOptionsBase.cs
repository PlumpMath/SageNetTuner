namespace TestClient.Commandline
{
    using CommandLine;

    public abstract class SubOptionsBase
    {
        [Option('p', "port", Required = true, HelpText = "Port to use")]
        public int Port { get; set; }

        [Option('h', "host", Required = false, DefaultValue = "localhost", HelpText = "Host to send message")]
        public string Host { get; set; }

    }
}