namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class TunerElement : ConfigurationElement, IConfigurationElementCollectionElement
    {
        public TunerElement()
        {
            ElementKey = "name";
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = false)]
        public bool Enabled
        {
            get
            {
                return (bool)base["enabled"];
            }
        }

        [ConfigurationProperty("listenerPort", IsRequired = true)]
        public int ListenerPort
        {
            get
            {
                return (int)base["listenerPort"];
            }
        }

        [ConfigurationProperty("encoder", IsRequired = true)]
        public string Encoder
        {
            get
            {
                return (string)base["encoder"];
            }
        }

        [ConfigurationProperty("device", IsRequired = true)]
        public string Device
        {
            get
            {
                return (string)base["device"];
            }
        }

        public object ElementKey { get ; private set; }
    }
}