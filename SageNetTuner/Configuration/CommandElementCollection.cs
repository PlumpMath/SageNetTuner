namespace SageNetTuner.Configuration
{
    using System.Configuration;

    [ConfigurationCollection(typeof(CommandElement), CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
    public class CommandElementCollection : BaseConfigurationElementCollection<CommandElement>
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new CommandElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return (element as CommandElement).Name;
        }


        public CommandElement this[int index]
        {
            get
            {
                return (CommandElement)base.BaseGet(index);
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

        public new CommandElement this[string id]
        {
            get
            {
                return (CommandElement)base.BaseGet(id);
            }
        }

    }
}