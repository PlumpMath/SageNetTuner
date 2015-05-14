namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(CommandElement), AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public class CommandElementCollection : BaseConfigurationElementCollection<CommandElement>
    {

        private const string CONST_ELEMENT_NAME = "command";

        protected override ConfigurationElement CreateNewElement()
        {
            return new CommandElement();
        }

        //protected override object GetElementKey(ConfigurationElement element)
        //{
        //    return (element as CommandElement).Name;
        //}

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



        public new CommandElement this[string id]
        {
            get
            {
                return (CommandElement)base.BaseGet(id);
            }
        }

    }
}