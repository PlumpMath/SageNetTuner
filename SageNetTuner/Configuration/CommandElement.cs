namespace SageNetTuner.Configuration
{
    using System;
    using System.Configuration;

    public class CommandElement : ConfigurationElement, IConfigurationElementCollectionElement
    {

        public CommandElement()
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
            set
            {
                base["name"] = value;
            }
        }

        [ConfigurationProperty("path", IsRequired = true)]
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
            set
            {
                base["path"] = value;
            }
            
        }

        [ConfigurationProperty("commandLineFormat", IsRequired = false, DefaultValue = "")]
        public string CommandLineFormat
        {
            get
            {
                return (string)base["commandLineFormat"];
            }
            set
            {
                base["commandLineFormat"] = value;
            }
        }

        [ConfigurationProperty("event", IsRequired = true)]
        public CommandEvent Event
        {
            get
            {
                return (CommandEvent)base["event"];
            }
            set
            {
                base["event"] = value;
            }
        }
        

        public object ElementKey { get; private set; }
    }

    public enum CommandEvent
    {
        BeforeStart,
        Start,
        AfterStart,
        BeforeStop,
        Stop,
        AfterStop
    }
}