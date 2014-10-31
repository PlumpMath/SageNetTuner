namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(EncoderElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class EncoderElementCollection : BaseConfigurationElementCollection<EncoderElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new EncoderElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as EncoderElement).Name;
        }

        public EncoderElement this[int index]
        {
            get
            {
                return (EncoderElement)base.BaseGet(index);
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

        public new EncoderElement this[string id]
        {
            get
            {
                return (EncoderElement)base.BaseGet(id);
            }
        }
    }
}