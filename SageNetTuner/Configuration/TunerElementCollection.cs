namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(TunerElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class TunerElementCollection : BaseConfigurationElementCollection<TunerElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TunerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as TunerElement).Name;
        }


        public TunerElement this[int index]
        {
            get
            {
                return (TunerElement)base.BaseGet(index);
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

        public new TunerElement this[string id]
        {
            get
            {
                return (TunerElement)base.BaseGet(id);
            }
        }


    }
}