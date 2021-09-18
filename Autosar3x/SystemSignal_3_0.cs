using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autosar;
using Common;
using Common.SysWItems;
namespace EcuExtractCreator.Autosar3x
{
    public static class SystemSignal_3_0
    {
        public static string createSystemSignal(string dtName, string constantRef,  string dtLength, string signalName, ARSIDataTypeDef dtDef)
        {
           var exstnSignalObj = Extract_3_0.getObjFromPkg("Signal", signalName) ;
            if(exstnSignalObj != null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"reusing existing system signal group definition for for signal{signalName}...");
                var exstnSignal= exstnSignalObj as SYSTEMSIGNAL;
                return exstnSignal.SHORTNAME;
            }
                
            SYSTEMSIGNAL sysSignal = new SYSTEMSIGNAL();
            sysSignal.SHORTNAME = $"{signalName}";
            //string dtLength = getSignalLength(signal);
            var dTypeRef = new SYSTEMSIGNALDATATYPEREF();
            var initValRef = new SYSTEMSIGNALINITVALUEREF();
            
            dTypeRef.DEST = (dtDef == ARSIDataTypeDef.PRIMITIVE)? DATATYPESUBTYPESENUM.INTEGERTYPE: DATATYPESUBTYPESENUM.STRINGTYPE;
            dTypeRef.Value = $"/DataType/{dtName}";
            
            initValRef.DEST = (dtDef == ARSIDataTypeDef.PRIMITIVE) ? VALUESPECIFICATIONSUBTYPESENUM.INTEGERLITERAL: VALUESPECIFICATIONSUBTYPESENUM.STRINGLITERAL;
            initValRef.Value = $"/Constant/{constantRef}";
            sysSignal.DATATYPEREF = dTypeRef;
            sysSignal.INITVALUEREF = initValRef;
            sysSignal.LENGTH = dtLength;
            //ExtractorUtilities.addDefToNode(sysSignal, "Signal");
            Extract_3_0.addObjToPkg("Signal", sysSignal);
            return sysSignal.SHORTNAME;
        }

        public static string createSystemSignalGroup(string recDTName, string []dtName, string []constantDefName, RecordDataType recDT)
        {
            var exstnSignalGrpObj = Extract_3_0.getObjFromPkg("SignalGroup", $"{recDTName}_sg");
            if (exstnSignalGrpObj != null)
            {
                var exstnSignalGrp = exstnSignalGrpObj as SYSTEMSIGNALGROUP;
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"reusing existing system signal group definition for for signal{recDT.Itemproperties.ItemName}...");
                return exstnSignalGrp.SHORTNAME;
            }
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"creating system signal group definition for for signal{recDT.Itemproperties.ItemName}...");
            var sysSignalGroup = new SYSTEMSIGNALGROUP();
            
            SYSTEMSIGNALGROUPSYSTEMSIGNALREF [] sysSigGroupRef = new SYSTEMSIGNALGROUPSYSTEMSIGNALREF[recDT.PrimitiveDataTypeCollection.Count];
            sysSignalGroup.SHORTNAME = $"{recDTName}_sg";
            for (int i = 0; i< recDT.PrimitiveDataTypeCollection.Count; i++) {
                var primDT = recDT.PrimitiveDataTypeCollection[i];
                var subSigLength = primDT.DTType == ARSIDataTypeDef.PRIMITIVE? primDT.DtCompuMethod.SignalLength: primDT.getAttrVal("ARSR");
                var subSigName = primDT.Itemproperties.ItemName;
                var recSubSigName = $"{recDTName}_{subSigName}";
                var constRefName = constantDefName[i];//$"C_{recDTName}_IV/{}";
                var sysSignal = createSystemSignal(dtName[i], constRefName, subSigLength, recSubSigName, primDT.DTType) ;
                var subSysSigRef = new SYSTEMSIGNALGROUPSYSTEMSIGNALREF();
                subSysSigRef.DEST = SYSTEMSIGNALSUBTYPESENUM.SYSTEMSIGNAL;
                subSysSigRef.Value = $"/Signal/{sysSignal}";
                sysSigGroupRef[i] = subSysSigRef;
             }
            sysSignalGroup.SYSTEMSIGNALREFS = sysSigGroupRef;
            //ExtractorUtilities.addDefToNode(sysSignalGroup, "SignalGroup");
            Extract_3_0.addObjToPkg("SignalGroup", sysSignalGroup);
            return sysSignalGroup.SHORTNAME;
            //private DATATYPESUBTYPESENUM getTypeRefDest
        }
       

        /**private static ARSIDataTypeDef[] getSigTypes(SysWSignalDefinition recSignal)
        {
            ARSIDataTypeDef[] subSigLengths;
            if (recSignal.GetType().Name == "SysWSignalDefinition")
            {
                subSigLengths = new ARSIDataTypeDef[recSignal.SubSignals.Count];
                for (int i = 0; i < recSignal.SubSignals.Count; i++)
                {
                    subSigLengths[i] = recSignal.SubSignals[i].;
                }
            }
            else
            {
                var arsiSig = recSignal as SysWSignalDefinitionARSI;
                var recDT = arsiSig.DatatypeDefs[0] as RecordDataType;
                subSigLengths = new string[recDT.PrimitiveDataTypeCollection.Count];
                for (int i = 0; i < recDT.PrimitiveDataTypeCollection.Count; i++)
                {
                    subSigLengths[i] = recDT.PrimitiveDataTypeCollection[i].DtCompuMethod.SignalLength;
                }

            }
            return subSigLengths;
        }

 
        private static string[] getSubsignalNames(SysWSignalDefinition recSignal)
        {
            string[] subSigNames;
            if (recSignal.GetType().Name == "SysWSignalDefinition")
            {
                subSigNames = new string[recSignal.SubSignals.Count];
                for (int i = 0; i < recSignal.SubSignals.Count; i++)
                {
                    subSigNames[i] = recSignal.SubSignals[i].Itemproperties.ItemName;
                }
            }
            else
            {
                var arsiSig = recSignal as SysWSignalDefinitionARSI;
                var recDT = arsiSig.DatatypeDefs[0] as RecordDataType;
                subSigNames = new string[recDT.PrimitiveDataTypeCollection.Count];
                for (int i = 0; i < recDT.PrimitiveDataTypeCollection.Count; i++)
                {
                    subSigNames[i] = recDT.PrimitiveDataTypeCollection[i].Itemproperties.ItemName;
                }
            }
            return subSigNames;
        }**/
        public static bool checkIsSystemSignal(string signalName, string portDir)
        {
            var compPkg = Extract_3_0.getARPackage("ComponentType");
            foreach(var compObj in compPkg.ELEMENTS)
            {
                var compPorts = compObj.GetType().GetProperty("PORTS").GetValue(compObj) as object[]; //get all ports in the component
                foreach(var portObj in compPorts)
                {
                    //if()
                }
            }
            return false;
        }

        public static void removeSystemSignal(string sysSigShrtName)
        {
            var topPkg = Extract_3_0.getARPackage("Signal");
            foreach (var sigObj in topPkg.ELEMENTS)
            {
                string sigShortName = sigObj.GetType().GetProperty("SHORTNAME").GetValue(sigObj).ToString();
                if (sigShortName.Equals(sysSigShrtName))
                {
                    Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"deleting system signal definition for {sigShortName}");
                    Extract_3_0.removeObjFromPkg("Signal", sysSigShrtName);
                    return;
                }
            }
            Common.GlobalDefs.logMessageToFile(Common.LOGMSGCLASS.INFO, $"no system signal definition for {sysSigShrtName}");
            removeSystemSignalGroup(sysSigShrtName);
        }

        private static void removeSystemSignalGroup(string sysSigShrtName)
        {
            var topPkg = Extract_3_0.getARPackage("SignalGroup");
            foreach(var sigGrpObj in topPkg.ELEMENTS)
            {
                string sigGrpShortName = sigGrpObj.GetType().GetProperty("SHORTNAME").GetValue(sigGrpObj).ToString();
                if (sigGrpShortName.Equals($"{sysSigShrtName}_sg"))
                {
                    var sysSigGrp = sigGrpObj as autosar.SYSTEMSIGNALGROUP;
                    var subSigs = sysSigGrp.SYSTEMSIGNALREFS;
                    Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"deleting group system signal definition for {sysSigShrtName}");
                    foreach (var subSig in subSigs)
                    {
                        Extract_3_0.removeObjFromPkg("Signal", subSig.Value.Split('/')[2]);
                    }
                    Extract_3_0.removeObjFromPkg("SignalGroup", sysSigShrtName);
                    break;
                }
            }
        }

       
    }
}
