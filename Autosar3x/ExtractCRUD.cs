using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EcuExtractCreator.Autosar3x
{
    public class ExtractCRUD
    {
        private static string _xmlFileLocation;
        private static XmlDocument _xtractARXML, _template_arxml;
        private static XmlNamespaceManager _ns;
        public static string XmlFileLocation
        {
            get { return _xmlFileLocation; }
            set { _xmlFileLocation = value; }
        }

        public static XmlDocument XtractARXML { get { return _xtractARXML; } }
        public static XmlNamespaceManager NS { get { return _ns; } }
        public static XmlDocument TemplateARXML { get { return _template_arxml; } }

        public ExtractCRUD() { }
    }
}
