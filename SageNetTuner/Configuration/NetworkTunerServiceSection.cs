namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class NetworkTunerServiceSection : ConfigurationSection
    {
        private static readonly NetworkTunerServiceSection _configSection = (NetworkTunerServiceSection)ConfigurationManager.GetSection("NetworkTunerService");
        public static NetworkTunerServiceSection Settings
        {
            get
            {
                return _configSection;
            }
        }

        [ConfigurationProperty("encoders")]
        public EncoderElementCollection Encoders
        {
            get
            {
                return (EncoderElementCollection)base["encoders"];
            }
        }

        [ConfigurationProperty("channelProviders")]
        public ChannelProviderElementCollection ChannelProviders
        {
            get
            {
                return (ChannelProviderElementCollection)base["channelProviders"];
            }
        }

        [ConfigurationProperty("tuners")]
        public TunerElementCollection Tuners
        {
            get
            {
                return (TunerElementCollection)base["tuners"];
            }
        }

        [ConfigurationProperty("devices")]
        public DeviceElementCollection Devices
        {
            get
            {
                return (DeviceElementCollection)base["devices"];
            }
        }
    }
}
