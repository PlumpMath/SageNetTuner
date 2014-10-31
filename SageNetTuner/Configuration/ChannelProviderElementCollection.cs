namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(ChannelProviderElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class ChannelProviderElementCollection : BaseConfigurationElementCollection<ChannelProviderElement>
    {

        protected override ConfigurationElement CreateNewElement()
        {
            return new ChannelProviderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as ChannelProviderElement).Name;
        }


        public ChannelProviderElement this[int index]
        {
            get
            {
                return (ChannelProviderElement)base.BaseGet(index);
            }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);

                }
                base.BaseAdd(index, value);
            }
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