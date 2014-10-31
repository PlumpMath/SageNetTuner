namespace SageNetTuner
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Serialization;

    public class XmlHelper
    {

        public static T FromXml<T>(string xml) where T : class
        {
            return FromXml(xml, typeof(T)) as T;
        }

        public static object FromXml(string xml, Type objType)
        {
            var ser = new XmlSerializer(objType);

            Object obj;
            using (var xmlReader = new XmlTextReader(new StringReader(xml)))
            {
                obj = ser.Deserialize(xmlReader);
            }

            return obj;
        }


        public static string ToXml<T>(object obj) where T : class
        {
            return ToXml(obj, typeof(T));
        }



        public static string ToXml(object obj, Type objType)
        {
            string xml = null;

            var ser = new XmlSerializer(objType);


            using (var xmlWriter = new XmlTextWriter(new MemoryStream(), Encoding.UTF8) { Namespaces = true })
            {
                var memStream = xmlWriter.BaseStream as MemoryStream;
                ser.Serialize(xmlWriter, obj);

                if (memStream != null)
                {
                    xml = Encoding.UTF8.GetString(memStream.GetBuffer());
                    xml = xml.Substring(xml.IndexOf(Convert.ToChar(60)));
                    xml = xml.Substring(0, (xml.LastIndexOf(Convert.ToChar(62)) + 1));
                }
            }


            return xml;
        }

        public static XDocument ToXDocument(string xml)
        {
            var sr = new StringReader(xml);
            XDocument doc = XDocument.Load(sr, LoadOptions.None);
            sr.Close();
            return doc;
        }


        public static string GetElementValueFromXml(string xml, string elementName)
        {
            var sr = new StringReader(xml);
            XDocument doc = XDocument.Load(sr, LoadOptions.None);
            sr.Close();
            return doc.Descendants(elementName).First().Value;
        }
    }
}