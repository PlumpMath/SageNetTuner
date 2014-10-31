namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(DeviceElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class DeviceElementCollection : BaseConfigurationElementCollection<DeviceElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new DeviceElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as DeviceElement).Name;
        }


        public DeviceElement this[int index]
        {
            get
            {
                return (DeviceElement)base.BaseGet(index);
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

        public new DeviceElement this[string id]
        {
            get
            {
                return (DeviceElement)base.BaseGet(id);
            }
        }
        
    }
}