namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(CaptureProfileElement), AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class CaptureProfileElementCollection : BaseConfigurationElementCollection<CaptureProfileElement>
    {
        private const string CONST_ELEMENT_NAME = "profile";

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

        protected override ConfigurationElement CreateNewElement()
        {
            return new CaptureProfileElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CaptureProfileElement).Name;
        }

        public new CaptureProfileElement this[string id]
        {
            get
            {
                return (CaptureProfileElement)base.BaseGet(id);
            }
        }
    }
}