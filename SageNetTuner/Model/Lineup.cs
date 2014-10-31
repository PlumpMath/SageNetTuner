namespace SageNetTuner.Model
{
    using System.Collections.Generic;
    using System.Xml.Serialization;

    public class Lineup
    {

        [XmlElement("Program")]
        public List<Channel> Channels { get; set; } 
    }
}