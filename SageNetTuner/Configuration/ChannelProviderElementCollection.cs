namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ChannelProviderElement), AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class ChannelProviderElementCollection : BaseConfigurationElementCollection<ChannelProviderElement>
    {
        private const string CONST_ELEMENT_NAME = "provider";

        protected override ConfigurationElement CreateNewElement()
        {
            return new ChannelProviderElement();
        }

        protected override string ElementName
        {
            get
            {
                return CONST_ELEMENT_NAME;
            }
        }
        public override ConfigurationElementCollectionType CollectionType
        {
            get
            {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }


        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ChannelProviderElement).Name;
        }


        public new ChannelProviderElement this[string id]
        {
            get
            {
                return (ChannelProviderElement)base.BaseGet(id);
            }
        }


    }
}