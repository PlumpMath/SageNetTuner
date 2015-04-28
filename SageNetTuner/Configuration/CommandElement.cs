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

        //[ConfigurationProperty("name", IsRequired = false)]
        //public string Name
        //{
        //    get
        //    {
        //        return (string)base["name"];
        //    }
        //    set
        //    {
        //        base["name"] = value;
        //    }
        //}

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

        [ConfigurationProperty("event", IsRequired = true, DefaultValue = CommandEvent.Start)]
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


        [ConfigurationProperty("delayAfterStart", IsRequired = false)]
        public TimeSpan DelayAfterStart
        {
            get
            {
                return (TimeSpan)base["delayAfterStart"];
            }
            set
            {
                base["delayAfterStart"] = value;
            }
        }

        [ConfigurationProperty("settings", IsRequired = false)]
        public NameValueConfigurationCollection Settings
        {
            get
            {
                return (NameValueConfigurationCollection)base["settings"];
            }
            set
            {
                base["settings"] = value;
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