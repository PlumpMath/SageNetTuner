namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(TunerElement), AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class TunerElementCollection : BaseConfigurationElementCollection<TunerElement>
    {

        private const string CONST_ELEMENT_NAME = "tuner";

        protected override ConfigurationElement CreateNewElement()
        {
            return new TunerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as TunerElement).Name;
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


        public new TunerElement this[string id]
        {
            get
            {
                return (TunerElement)base.BaseGet(id);
            }
        }


    }
}