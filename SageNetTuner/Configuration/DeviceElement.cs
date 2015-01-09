namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class DeviceElement : ConfigurationElement, IConfigurationElementCollectionElement
    {

        public DeviceElement()
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

        [ConfigurationProperty("address", IsRequired = true)]
        public string Address
        {
            get
            {
                return (string)base["address"];
            }
        }

        [ConfigurationProperty("channelProvider", IsRequired = true)]
        public string ChannelProvider
        {
            get
            {
                return (string)base["channelProvider"];
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



        public override string ToString()
        {
            return string.Format("{0}@{1}", Name,Address);
        }

        public object ElementKey { get ; private set; }
    }
}