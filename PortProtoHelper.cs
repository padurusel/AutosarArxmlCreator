using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autosar;
using Common.SysWItems;
using Common;


namespace EcuExtractCreator
{
    using Autosar3x;
    public static class PortProtoHelper
    {
        public static string crtExstnConstant(SysWSignalDefinition signal, string exstnDTName)
        {
            var dtDef = Extract_3_0.getObjFromPkg("DataType", exstnDTName);
            if(dtDef == null){
                GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"datatype referenced by existing port for signal {signal.Itemproperties.ItemName} does not exist");
                throw new BadReferenceException($"datatype referenced by existing port for signal {signal.Itemproperties.ItemName} does not exist");
            }
            var signalName = signal.Itemproperties.ItemName;
            string constDefName;
            if(signal.getSignalType() == "PrimitiveType")
            {
                var primDT = signal.GetType().Name == "SysWSignalDefinitionARSI" ? (signal as SysWSignalDefinitionARSI).DatatypeDefs[0] as PrimitiveDataType
                                                                                   : IntegerDataType_3_0.convertCBDSSignal(signal);
                constDefName = ConstantCreator.createIntegerConstantDef(signal, exstnDTName, primDT.DTType);
            }
            else
            {
                var recDT = signal.GetType().Name == "SysWSignalDefinitionARSI" ? (signal as SysWSignalDefinitionARSI).DatatypeDefs[0] as RecordDataType
                                                                                   : RecordDataType_3_0.convertCBDSRecordType(signal);
                List<string> recElemsDTNames = getRecDTElems(exstnDTName);
                ConstantCreator.createRecordConstDef(recDT, recElemsDTNames, signalName, exstnDTName, out constDefName);
            }
            return constDefName;
        }

        private static List<string> getRecDTElems(string exstnDTName)
        {
            var dtDef = Extract_3_0.getObjFromPkg("DataType", exstnDTName) as RECORDTYPE;
            List<string> recElemDTNames = new List<string>();

            foreach (RECORDELEMENT reElem in dtDef.ELEMENTS)
            {
                //recElemNames.Add(reElem.SHORTNAME);
                recElemDTNames.Add(reElem.TYPETREF.Value.Split('/').Last());
            }
            return recElemDTNames;
        }

        public static PortElementRefs getPortPIReference(object obj)
        {
            var portRefs = new PortElementRefs();
            if (obj.GetType().Name == "RPORTPROTOTYPE")
            {
                var rprtProto = obj as RPORTPROTOTYPE;
                var reqComSpecFld = rprtProto.REQUIREDCOMSPECS[0] as UNQUEUEDRECEIVERCOMSPEC;
                portRefs.constPath = reqComSpecFld.DATAELEMENTREF.Value.Split('/')[2];
                portRefs.portIntPath = rprtProto.REQUIREDINTERFACETREF.Value.Split('/').Last();
            }
            else
            {
                var prvdprtProto = obj as PPORTPROTOTYPE;
                var prvdComSpecFld = prvdprtProto.PROVIDEDCOMSPECS[0] as UNQUEUEDSENDERCOMSPEC;
                portRefs.constPath = prvdComSpecFld.DATAELEMENTREF.Value.Split('/')[2];
                portRefs.portIntPath = prvdprtProto.PROVIDEDINTERFACETREF.Value.Split('/')[2];
            }
            return portRefs;
        }

        public static bool checkPortUsage(string portName, string compName)
        {
            var compARPkg = Extract_3_0.getARPackage("ComponentType");
            // check if portInterface is used elsewhere before deleting.
            Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"searching for port {portName} usage instances in other components");
            foreach (object obj in compARPkg.ELEMENTS)
            {

                if (obj.GetType().Name == "APPLICATIONSOFTWARECOMPONENTTYPE")
                {
                    var comp = obj as APPLICATIONSOFTWARECOMPONENTTYPE;
                    //Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"searching in {comp.SHORTNAME}");
                    if (comp != null)
                    {
                        if (comp.PORTS != null)
                        {
                            foreach (object pObj in comp.PORTS)
                            {

                                string pObjShortName = getPortPIReference(pObj).portIntPath; //pObj.GetType().GetProperty("SHORTNAME").GetValue(pObj).ToString();
                                if (!string.IsNullOrEmpty(pObjShortName))
                                    if (!comp.SHORTNAME.Equals(compName) && !comp.SHORTNAME.Equals("ICHMIProxy_B2") && portName.Equals(pObjShortName))
                                    {
                                        Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"component {comp.SHORTNAME} references {portName} cannot delete");
                                        return false;
                                    }
                            }
                        }
                    }
                }
            }
            return true;
        }
    }
}
