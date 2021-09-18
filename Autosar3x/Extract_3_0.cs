using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Common;
namespace EcuExtractCreator.Autosar3x
{
    public static class Extract_3_0
    {
        private static List<autosar.ARPACKAGE> _TopNodes;
        private static void _initTopNodes()
        {
            _TopNodes = new List<autosar.ARPACKAGE>();
            XmlNode toplvlPkgs = ExtractorUtilities.TemplateARXML.SelectSingleNode("x:AUTOSAR/x:TOP-LEVEL-PACKAGES", ExtractorUtilities.NS);
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "deserializing extract AR packages...");
            foreach (XmlNode childNode in toplvlPkgs.ChildNodes)
            {
                //dataTypeNode = Utilities.getXtractTopNode("DataType", "xtract"); // get exixting datatype node from extract as xmlnode
                XmlNode dataTypeNode = ExtractorUtilities.clearNameSpaceAttrr(childNode as XmlElement);
                _TopNodes.Add (ExtractorUtilities.ConvertNode<autosar.ARPACKAGE>(dataTypeNode));
            }
        }
        public static void SortNodeByShortName()
        {

        }
        public static void ReadExistingExtract()
        {
            _initTopNodes();
        }
        public static void SetupExtractBase()
        {
            _initTopNodes();
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "configuring extract base for new extract....");
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "configuring ECU project in extract....");
            var arPkg = getARPackage("Davinci");
            var ecuConfig = arPkg.ELEMENTS[0] as autosar.ECUCONFIGURATION;
            ecuConfig.SHORTNAME = Common.GlobalDefs.ECU;
            ecuConfig.ECUEXTRACTREF.Value = $"/VehicleProject/{Common.GlobalDefs.ECU}";
            var subArpkg = arPkg.SUBPACKAGES[0];
            var ecuInstance = subArpkg.ELEMENTS[0] as autosar.ECUINSTANCE;
            ecuInstance.SHORTNAME = Common.GlobalDefs.ECU;
            editTopNode(arPkg);

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "configuring TopLevelComposition in extract....");
            //var compArPkg = getPackage("ComponentType");
            var topLevelComp = new autosar.COMPOSITIONTYPE();
            topLevelComp.SHORTNAME = "TopLevelComposition";
            topLevelComp.PORTS = new List<object>().ToArray();
            topLevelComp.CONNECTORS = new List<object>().ToArray();
            topLevelComp.COMPONENTS = new List<autosar.COMPONENTPROTOTYPE>().ToArray();
            addObjToPkg("ComponentType", topLevelComp);

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "configuring ECU Composition in extract....");
            var ecuCompArPkg = getARPackage("ECUComposition");
            var ecuSWComp = ecuCompArPkg.ELEMENTS[0] as autosar.ECUSWCOMPOSITION;
            ecuSWComp.ECUEXTRACTREF.Value= $"/VehicleProject/{Common.GlobalDefs.ECU}";
            editTopNode(ecuCompArPkg);

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "configuring Vehicle Project and Systems in extract....");
            var vehicleProjArPkg = getARPackage("VehicleProject");
            var sysElement = vehicleProjArPkg.ELEMENTS[0] as autosar.SYSTEM;
            sysElement.SHORTNAME = $"{Common.GlobalDefs.ECU}";
            sysElement.FIBEXELEMENTREFS[0].Value = $"/DaVinci/PKG_ECU/{Common.GlobalDefs.ECU}";
            sysElement.MAPPING.SHORTNAME = $"{Common.GlobalDefs.ECU}_MPPNG";
            editTopNode(vehicleProjArPkg);

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "done configuring extract base....");
            //string exstnShrtName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
        }
        private static autosar.ARPACKAGE _getTopNode(string topNodeName)
        {
            foreach(var topNode in _TopNodes)
            {
                if (topNode.SHORTNAME.Equals(topNodeName, StringComparison.CurrentCultureIgnoreCase)) 
                    return topNode;
            }
            GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"search for AR package {topNodeName} returned null");
            return null;
        } 
        public static autosar.ARPACKAGE getARPackage(string package)
        {
            if (package.Equals("DataTypeSemantics") || package.Equals("DataTypeUnits"))
            {
                if (package.Equals("DataTypeSemantics"))
                    return _getTopNode("DataType").SUBPACKAGES[0];
                else
                    return _getTopNode("DataType").SUBPACKAGES[1];
            }
            else
            {
                return _getTopNode(package);
            }
        }
        public static void addObjToPkg(string arPkg, object obj)
        {
            autosar.ARPACKAGE CompTypePkg = getARPackage(arPkg);

            List<object> CompTypePkgElements = CompTypePkg.ELEMENTS.ToList();
            CompTypePkgElements.Add(obj);
            CompTypePkg.ELEMENTS = CompTypePkgElements.ToArray();
            if (arPkg.Equals("DataTypeSemantics") || arPkg.Equals("DataTypeUnits"))
            {
                var tempTopPkg = getARPackage("DataType");
                if (arPkg.Equals("DataTypeSemantics"))
                {
                    tempTopPkg.SUBPACKAGES[0].ELEMENTS = CompTypePkg.ELEMENTS;
                }
                else { tempTopPkg.SUBPACKAGES[1].ELEMENTS = CompTypePkg.ELEMENTS; }

                editTopNode(tempTopPkg);
            }
            else { editTopNode(CompTypePkg); }
            //ExtractorUtilities.addDefToNode(obj, arPkg);
            //ExtractorUtilities.sortTopLvlPkg(arPkg);
        }
        public static void removeObjFromPkg(string arPkg, string shortName) {

            autosar.ARPACKAGE topPkg = getARPackage(arPkg);
            /**if (arPkg.Equals("DataTypeSemantics") || arPkg.Equals("DataTypeUnits"))
            {
                if (arPkg.Equals("DataTypeSemantics"))
                  topPkg = ExtractorUtilities.getXtractTopNode("DataType", "xtract").SUBPACKAGES[0];
                else
                 topPkg = ExtractorUtilities.getXtractTopNode("DataType", "xtract").SUBPACKAGES[1];
            }
            else
            {
                topPkg = ExtractorUtilities.getXtractTopNode(arPkg, "xtract");
            }**/

            foreach(object obj in topPkg.ELEMENTS)
            {
                string exstnShrtName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                if(exstnShrtName.Equals(shortName, StringComparison.CurrentCultureIgnoreCase))
                {
                    var elementsList = topPkg.ELEMENTS.ToList();
                    elementsList.Remove(obj);
                    topPkg.ELEMENTS = elementsList.ToArray();
                    if (arPkg.Equals("DataTypeSemantics") || arPkg.Equals("DataTypeUnits"))
                    {
                        var tempTopPkg = getARPackage("DataType");
                        if (arPkg.Equals("DataTypeSemantics"))
                        {
                            tempTopPkg.SUBPACKAGES[0].ELEMENTS = topPkg.ELEMENTS;
                        }
                        else { tempTopPkg.SUBPACKAGES[1].ELEMENTS = topPkg.ELEMENTS; }

                        editTopNode(tempTopPkg);
                    }
                    else { editTopNode(topPkg); }
                    
                    //break;
                }
            }
        }
        public static object getObjFromPkg(string arPkg, string shortName)
        {
            autosar.ARPACKAGE topPkg = getARPackage(arPkg);
            /**if (arPkg.Equals("DataTypeSemantics") || arPkg.Equals("DataTypeUnits"))
            {
                autosar.ARPACKAGE dTPkg = ExtractorUtilities.getXtractTopNode("DataType", "xtract");
                // autosar.ARPACKAGE dTSubPkg;
                if (arPkg.Equals("DataTypeSemantics"))
                    topPkg = dTPkg.SUBPACKAGES[0];
                else
                    topPkg = dTPkg.SUBPACKAGES[1];
            }
            else
            {
                topPkg = ExtractorUtilities.getXtractTopNode(arPkg, "xtract");
            }**/
            
            foreach (object obj in topPkg.ELEMENTS)
            {
                string exstnShrtName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                if (exstnShrtName.Equals(shortName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return obj;
                }
            }
            return null;
        }
        public static object getObjTypeFromPkg(string arPkg, string objTypeName, string shortName)
        {
            autosar.ARPACKAGE topPkg = getARPackage(arPkg);
            /**if (arPkg.Equals("DataTypeSemantics") || arPkg.Equals("DataTypeUnits"))
            {
                autosar.ARPACKAGE dTPkg = ExtractorUtilities.getXtractTopNode("DataType", "xtract");
                // autosar.ARPACKAGE dTSubPkg;
                if (arPkg.Equals("DataTypeSemantics"))
                    topPkg = dTPkg.SUBPACKAGES[0];
                else
                    topPkg = dTPkg.SUBPACKAGES[1];
            }
            else
            {
                topPkg = ExtractorUtilities.getXtractTopNode(arPkg, "xtract");
            }**/

            foreach (object obj in topPkg.ELEMENTS)
            {
                string exstnShrtName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                if (exstnShrtName.Equals(shortName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if(obj.GetType().Name.Equals(objTypeName))
                        return obj;
                }
            }
            return null;
        }
        
        public static void removeComponentFromToplvl(string compName)
        {
            var topPkg = getARPackage("ComponentType");
            foreach (object obj in topPkg.ELEMENTS)
            {
                string objShrtName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                if (objShrtName.Equals("TopLevelComposition", StringComparison.CurrentCultureIgnoreCase))
                {
                    var topLevelComp = obj as autosar.COMPOSITIONTYPE;
                    foreach(object compObj in topLevelComp.COMPONENTS)
                    {
                        string compObjShrtName = compObj.GetType().GetProperty("SHORTNAME").GetValue(compObj).ToString();
                        if (compName.Equals(compObjShrtName))
                        {
                            var compList = topLevelComp.COMPONENTS.ToList();
                            compList.Remove(compObj as autosar.COMPONENTPROTOTYPE);
                            topLevelComp.COMPONENTS = compList.ToArray();
                            break;
                        }
                    }
                    var conList = topLevelComp.CONNECTORS.ToList();
                    
                    foreach (var conObj in topLevelComp.CONNECTORS)
                    {
                        string conObjShrtName = conObj.GetType().GetProperty("SHORTNAME").GetValue(conObj).ToString();
                        if (conObjShrtName.Contains(compName))
                        {
                            conList.Remove(conObj);
                            topLevelComp.CONNECTORS = conList.ToArray();
                        }
                    }
                    topLevelComp.CONNECTORS = conList.ToArray();
                    editTopNode(topPkg);
                    break;

                }
            }

        }
         public static void editTopNode(autosar.ARPACKAGE arPkg){

            for (int i = 0; i < _TopNodes.Count; i++ )
            {
                if (_TopNodes[i].SHORTNAME.Equals(arPkg.SHORTNAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    //_TopNodes[_TopNodes.FindIndex(indx => indx.Equals(topNode))] = arPkg;
                    _TopNodes[i] = arPkg;
                    return;
                }
                  
            }
         }
        public static void editTopNodeXml()
        {
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "writing packages to extract file...");
            foreach (var topNode in _TopNodes)
            {
                ExtractorUtilities.editTopNodeXml(topNode);
            }
        }
        internal static void sortAllTopNodes()
        {
            for(int i = 0; i < _TopNodes.Count; i++)
            {
                Common.GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"sorting package {_TopNodes[i].SHORTNAME}...");
                sortTopNode(_TopNodes[i].SHORTNAME);
            }
            Common.GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"sorting package DataTypeSemantics...");
            sortTopNode("DataTypeSemantics");
            Common.GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"sorting package DataTypeUnits...");
            sortTopNode("DataTypeUnits");
        }
        internal static void sortTopNode(string arPkgName)
        {
            autosar.ARPACKAGE arPkg = getARPackage(arPkgName);
            int min_idx;
            for (int i = 0; i < arPkg.ELEMENTS.Length; i++)
            {
                min_idx = i;
                for (int j = i + 1; j < arPkg.ELEMENTS.Length; j++)
                {
                    if (string.Compare(arPkg.ELEMENTS[min_idx].GetType().Name, arPkg.ELEMENTS[j].GetType().Name) > 0)
                    {
                        min_idx = j;
                    }
                    else
                    {
                        if (string.Compare(arPkg.ELEMENTS[min_idx].GetType().Name, arPkg.ELEMENTS[j].GetType().Name) == 0)
                        {
                            object iObj = arPkg.ELEMENTS[min_idx];
                            object jObj = arPkg.ELEMENTS[j];
                            string iShortName = iObj.GetType().GetProperty("SHORTNAME").GetValue(iObj).ToString();
                            string jShortName = jObj.GetType().GetProperty("SHORTNAME").GetValue(jObj).ToString();
                            if (string.Compare(iShortName, jShortName, StringComparison.CurrentCultureIgnoreCase) > 0)
                            {
                                min_idx = j;
                            }
                        }
                    }

                }
                if (i != min_idx)
                {
                    object tmpObj = arPkg.ELEMENTS[i];
                    arPkg.ELEMENTS[i] = arPkg.ELEMENTS[min_idx];
                    arPkg.ELEMENTS[min_idx] = tmpObj;
                }

            }
            editTopNode(arPkg);
        }

        public static void editComponentTypeNode(object comp, string compName)
        {
            autosar.ARPACKAGE compType = getARPackage("ComponentType");
            string shortName = "";
            var compTypeElemList = compType.ELEMENTS.ToList();
            foreach (object obj in compTypeElemList)
            {
                if (obj.GetType().Name == "COMPOSITIONTYPE")
                {
                    var swComp = obj as autosar.COMPOSITIONTYPE;
                    shortName = swComp.SHORTNAME;
                }
                if (obj.GetType().Name == "APPLICATIONSOFTWARECOMPONENTTYPE")
                {
                    var swComp = obj as autosar.APPLICATIONSOFTWARECOMPONENTTYPE;
                    shortName = swComp.SHORTNAME;
                }
                if (obj.GetType().Name == "INTERNALBEHAVIOR")
                {
                    var swComp = obj as autosar.INTERNALBEHAVIOR;
                    shortName = swComp.SHORTNAME;
                }
                if (obj.GetType().Name == "SWCIMPLEMENTATION")
                {
                    var swComp = obj as autosar.SWCIMPLEMENTATION;
                    shortName = swComp.SHORTNAME;
                }
                if (shortName.Equals(compName, StringComparison.CurrentCultureIgnoreCase))
                {
                    compTypeElemList[compTypeElemList.IndexOf(obj)] = comp;
                    compType.ELEMENTS = compTypeElemList.ToArray();
                    editTopNode(compType);
                    break;
                }
            }
        }

    }
}
