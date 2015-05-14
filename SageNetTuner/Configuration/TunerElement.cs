namespace SageNetTuner.Configuration
{
    using System.Configuration;

    public class TunerElement : ConfigurationElement, IConfigurationElementCollectionElement
    {
        public TunerElement()
        {
            ElementKey = "name";
        }

        [ConfigurationProperty("name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["name"];
            }
        }

        [ConfigurationProperty("enabled", IsRequired = false, DefaultValue = false)]
        public bool Enabled
        {
            get
            {
                return (bool)base["enabled"];
            }
        }

        [ConfigurationProperty("listenerPort", IsRequired = true, DefaultValue = 1)]
        [IntegerValidator(MinValue = 1, MaxValue = 65536)]
        public int ListenerPort
        {
            get
            {
                return (int)base["listenerPort"];
            }
        }

        [ConfigurationProperty("captureProfile", IsRequired = true)]
        public string Encoder
        {
            get
            {
                return (string)base["captureProfile"];
            }
        }

        //[ConfigurationProperty("device", IsRequired = true)]
        //public string Device
        //{
        //    get
        //    {
        //        return (string)base["device"];
        //    }
        //}


        [ConfigurationProperty("sageTvId", IsRequired = true)]
        public string SageTvId { get; set; }
       


        public object ElementKey { get ; private set; }
    }
}