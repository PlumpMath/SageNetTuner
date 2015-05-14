namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(DeviceElement),AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class DeviceElementCollection : BaseConfigurationElementCollection<DeviceElement>
    {

        private const string CONST_ELEMENT_NAME = "device";


        protected override ConfigurationElement CreateNewElement()
        {
            return new DeviceElement();
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
            return (element as DeviceElement).Name;
        }


        public new DeviceElement this[string id]
        {
            get
            {
                return (DeviceElement)base.BaseGet(id);
            }
        }
        
    }
}