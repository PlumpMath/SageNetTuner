namespace SageNetTuner.Providers
{
    using System.Net;

    using NLog;

    using SageNetTuner.Configuration;
    using SageNetTuner.Model;

    public class HDHomeRunChannelProvider : IChannelProvider
    {

        private readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public Lineup GetLineup(DeviceElement deviceSettings)
        {
            var url = string.Format("http://{0}/lineup.xml", deviceSettings.Address);
            Logger.Debug("Getting Channel Lineup: {0}", url);
            var client = new WebClient();
            var channels = client.DownloadString(url);

            var lineup = XmlHelper.FromXml<Lineup>(channels);


            Logger.Debug("  Found Channels: {0}", lineup.Channels.Count);

            return lineup;
        }
    }
}
