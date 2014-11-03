namespace SageNetTuner.Configuration
{
    using System;
    using System.Configuration;

    public class EncoderElement : ConfigurationElement, IConfigurationElementCollectionElement
    {
        public EncoderElement()
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

        [ConfigurationProperty("path", IsRequired = false, DefaultValue = "")]
        public string Path
        {
            get
            {

                var tempPath = (string)base["path"];

                tempPath = Environment.ExpandEnvironmentVariables(tempPath);
                
                if (tempPath.StartsWith("."))
                    tempPath = System.IO.Path.GetFullPath(tempPath);  //convert from relative path

                return tempPath;
            }
        }

        [ConfigurationProperty("commandLineFormat", IsRequired = false, DefaultValue = "")]
        public string CommandLineFormat
        {
            get
            {
                return (string)base["commandLineFormat"];
            }
        }

        [ConfigurationProperty("commands")]
        public CommandElementCollection Commands
        {
            get
            {
                return (CommandElementCollection)base["commands"];
            }
        }

        [ConfigurationProperty("id", IsRequired = true)]
        public string Id { get; set; }
       

        public object ElementKey { get; private set; }

    }
}