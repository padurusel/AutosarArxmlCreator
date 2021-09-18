using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autosar;
using Common.SysWItems;
using Common;
namespace EcuExtractCreator.Autosar3x
{
    public static class RecordDataType_3_0
    {
        static RecordDataType _recordDataType;
        public static List<string> createRecordTypeDT( string[] recElemDTNames, string signalName, out string recDTName)
        {

            List<string> extnRecDTDef = new List<string>();
            var recPrimDT = _recordDataType.PrimitiveDataTypeCollection.Select(x => x.Itemproperties.ItemName).ToArray();
            bool foundRecDT = findExistingRecDT($"{signalName}_T", recPrimDT, ref extnRecDTDef, out recDTName);
            if ( foundRecDT == true && extnRecDTDef == null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"found same record datatype with different definition for signal {signalName}");
                GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"found same record datatype with different definition for signal {signalName}");
                throw new BadReferenceException($"found same record datatype with different name for record type {signalName}");
                
            }
            if (foundRecDT == true && extnRecDTDef != null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"found same datatype with different name for record type {signalName}");
                return extnRecDTDef;
            }
            List<string> elemNames = new List<string>();
            var recDT = new RECORDTYPE();
            recDT.SHORTNAME = $"{signalName}_T"; ;
            //recDTName = signalName;
            var recElemList = new List<autosar.RECORDELEMENT>();
            for (int i = 0; i < recElemDTNames.Length; i++)
            {
                var elemName = _recordDataType.PrimitiveDataTypeCollection[i].Itemproperties.ItemName;
                var recElem = new autosar.RECORDELEMENT();
                recElem.SHORTNAME = elemName.EndsWith("_RE")? elemName: elemName + "_RE";
                var recElemTRef = new autosar.RECORDELEMENTTYPETREF();
                recElemTRef.DEST = autosar.DATATYPESUBTYPESENUM.INTEGERTYPE;
                elemNames.Add(recElemDTNames[i]);
                recElemTRef.Value = "/DataType/" + recElemDTNames[i];
                recElem.TYPETREF = recElemTRef;
                recElemList.Add(recElem);
            }
            recDT.ELEMENTS = recElemList.ToArray();
            Extract_3_0.addObjToPkg("DataType", recDT);
            recDTName = recDT.SHORTNAME;
            return elemNames;

        }
        private static bool findExistingRecDT(string dtName, string[] dtSemName, ref List<string> exstnElemName, out string recDTName)
        {
            List<string> reElemNames = new List<string>();
           
            var recDTTypeObj = Extract_3_0.getObjFromPkg("DataType", dtName);
            if (recDTTypeObj != null)
            {
                var recDTType = recDTTypeObj as RECORDTYPE;
                exstnElemName = (recDTType.ELEMENTS.Select(x => x.SHORTNAME)).ToList();
                
                if (validateRecElems(exstnElemName, dtSemName))
                {
                    recDTName = recDTType.SHORTNAME;
                    return true;
                }
                else
                {
                    exstnElemName = null;
                    recDTName = recDTType.SHORTNAME;
                    return true;
                }
            }
            recDTName = null;
            return false;
        }

        private static bool validateRecElems(List<string> reElemNames, string[] dtSemNames)
        {
            if(reElemNames.Count == dtSemNames.Length)
            {
                //TODO : add this to the validation checks
                bool nameFound = false;
                foreach(string reElem in reElemNames)
                {
                    nameFound = false;
                    foreach (string dtSemName in dtSemNames)
                    {
                        var tempName = reElem.EndsWith("_RE") ? $"{dtSemName}_RE" : dtSemName;
                        if (tempName.Equals(reElem))
                        {
                            nameFound = true;
                            break;
                        }
                    }
                    if (nameFound != true)
                        return false;
                }
                 return true;
            }
            else
            {
                return false;
            }
            //throw new NotImplementedException();
        }

        internal static void setRecDT(RecordDataType recordDataType)
        {
            
            _recordDataType = recordDataType;
            //throw new NotImplementedException();
        }

       
        public static RecordDataType convertCBDSRecordType(SysWSignalDefinition cbdsRecSig)
        {
            var recDT = new RecordDataType(cbdsRecSig.Itemproperties);
           
            foreach (SysWSignalDefinition subsignal in cbdsRecSig.SubSignals)
            {
                var primDT = IntegerDataType_3_0.convertCBDSSignal(subsignal);
                recDT.PrimitiveDataTypeCollection.Add(primDT);
            }
            return recDT;
        }
    }
}
