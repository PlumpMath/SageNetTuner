namespace SageNetTuner.Configuration
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;

    public abstract class BaseConfigurationElementCollection<TConfigurationElementType> : ConfigurationElementCollection, IList<TConfigurationElementType> where TConfigurationElementType : ConfigurationElement, IConfigurationElementCollectionElement, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new TConfigurationElementType();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((TConfigurationElementType)element).ElementKey;
        }

        #region Implementation of IEnumerable<TConfigurationElementType>

        public new IEnumerator<TConfigurationElementType> GetEnumerator()
        {
            foreach (TConfigurationElementType type in (IEnumerable)this)
            {
                yield return type;
            }
        }


        #endregion

        #region Implementation of ICollection<TConfigurationElementType>

        public void Add(TConfigurationElementType configurationElement)
        {
            BaseAdd(configurationElement, true);
        }

        public void Clear()
        {
            BaseClear();
        }

        public bool Contains(TConfigurationElementType configurationElement)
        {
            return !(IndexOf(configurationElement) < 0);
        }

        public void CopyTo(TConfigurationElementType[] array, int arrayIndex)
        {
            base.CopyTo(array, arrayIndex);
        }

        public bool Remove(TConfigurationElementType configurationElement)
        {
            BaseRemove(GetElementKey(configurationElement));

            return true;
        }

        bool ICollection<TConfigurationElementType>.IsReadOnly
        {
            get { return IsReadOnly(); }
        }

        #endregion

        #region Implementation of IList<TConfigurationElementType>

        public int IndexOf(TConfigurationElementType configurationElement)
        {
            return BaseIndexOf(configurationElement);
        }

        public void Insert(int index, TConfigurationElementType configurationElement)
        {
            BaseAdd(index, configurationElement);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        public TConfigurationElementType this[int index]
        {
            get
            {
                return (TConfigurationElementType)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        #endregion

        //[ConfigurationCollection(typeof(TunerElement), AddItemName = CONST_ELEMENT_NAME, CollectionType = ConfigurationElementCollectionType.BasicMap)]
        //public class CustomConfigurationCollection : BaseConfigurationElementCollection<TunerElement>
        //{
        //    #region Constants
        //    private const string CONST_ELEMENT_NAME = "Custom";
        //    #endregion

        //    public override ConfigurationElementCollectionType CollectionType
        //    {
        //        get
        //        {
        //            return ConfigurationElementCollectionType.BasicMap;
        //        }
        //    }

        //    protected override string ElementName
        //    {
        //        get { return CONST_ELEMENT_NAME; }
        //    }
        //}
    }
}