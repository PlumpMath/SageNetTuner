namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class ChannelProviderElement : ConfigurationElement, IConfigurationElementCollectionElement
    {
        public ChannelProviderElement()
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

        [ConfigurationProperty("type", IsRequired = true)]
        public string Type
        {
            get
            {
                return (string)base["type"];
            }
        }

        public object ElementKey { get; private set; }
    }
}