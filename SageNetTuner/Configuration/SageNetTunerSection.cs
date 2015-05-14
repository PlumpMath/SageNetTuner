namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class SageNetTunerSection : ConfigurationSection
    {
        private static readonly SageNetTunerSection ConfigSection = (SageNetTunerSection)ConfigurationManager.GetSection("sageNetTuner");
        public static SageNetTunerSection Settings
        {
            get
            {
                return ConfigSection;
            }
        }

        [ConfigurationProperty("enableDiscovery", DefaultValue = false, IsRequired = false)]
        public bool EnableDiscovery
        {
            get
            {
                return (bool)base["enableDiscovery"];
            }
        }


        [ConfigurationProperty("captureProfiles")]
        public CaptureProfileElementCollection CaptureProfiles
        {
            get
            {
                return (CaptureProfileElementCollection)base["captureProfiles"];
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
