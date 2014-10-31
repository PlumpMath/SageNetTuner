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
