using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common;
namespace EcuExtractCreator
{
    using autosar;
    using Common.SysWItems;
    using Autosar3x;
    using System.Globalization;

    public static class PortElementsCreator
    {
       public static List<string> recElemsName = new List<string>();
        /// <summary>
        /// this function will check if a port definition already exists for cases where the port is reused 
        /// or a send and recieve port using same definition.
        /// </summary>
        /// <param name="signal"></param>
        /// <returns></returns>
        public static PortElementRefs createPortElementRefs(SysWSignalDefinition signal)
        {
            
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Creating new port definition for {signal.Itemproperties.ItemName}");
            string constDefName = ""; // hold constant def name
            string dtName = ""; // hold data type name
            var signalName = signal.Itemproperties.ItemName;
            SENDERRECEIVERINTERFACE sndRcvInt = new SENDERRECEIVERINTERFACE();
            sndRcvInt.SHORTNAME = signal.PartDefObjName.EndsWith("_I")? signal.PartDefObjName: $"{signal.PartDefObjName}_I";
            sndRcvInt.ISSERVICE = false;
            sndRcvInt.ISSERVICESpecified = false;
            DATAELEMENTPROTOTYPE[] dtElems = new DATAELEMENTPROTOTYPE[1];
            DATAELEMENTPROTOTYPE dtElem = new DATAELEMENTPROTOTYPE();
            dtElem.ISQUEUED = false;
            dtElem.ISQUEUEDSpecified = true;
            dtElem.SHORTNAME = signal.PartDefObjName;
            DATAELEMENTPROTOTYPETYPETREF dtElemTypeRef = new DATAELEMENTPROTOTYPETYPETREF();
                 
            if (signal.GetType().Name == "SysWSignalDefinitionARSI")
            {
                SysWSignalDefinitionARSI arsiSig = signal as SysWSignalDefinitionARSI;
                Autosar4xDatatypeDef autDtType = arsiSig.DatatypeDefs[0];// as PrimitiveDataType;
                switch (autDtType.GetType().Name)
                {
                    case "RecordDataType":
                        dtElemTypeRef.DEST = DATATYPESUBTYPESENUM.RECORDTYPE;
                        var recDT = autDtType as RecordDataType;
                        List<string> recElemsDTNames = createPortRecordDT(recDT, out dtName);
                        List<string> subConstDefName = ConstantCreator.createRecordConstDef(recDT, recElemsDTNames, signalName, dtName, out constDefName);
                        SystemSignal_3_0.createSystemSignalGroup(signal.Itemproperties.ItemName, recElemsDTNames.ToArray(), subConstDefName.ToArray(), recDT);
                        break;
                    case "PrimitiveDataType":
                        var primDT = arsiSig.DatatypeDefs[0] as PrimitiveDataType;
                        dtElemTypeRef.DEST = DATATYPESUBTYPESENUM.INTEGERTYPE;
                        dtName = createPortIntegerDT(primDT);
                        constDefName = ConstantCreator.createIntegerConstantDef(signal, dtName, primDT.DTType);
                        var constRef = $"{constDefName}";
                        //var primDT = arsiSig.DatatypeDefs[0] as PrimitiveDataType;
                        var signalLength = primDT.DTType == ARSIDataTypeDef.STRING ? primDT.dataTypeAttr[0].AtributeValue : primDT.DtCompuMethod.SignalLength;
                        SystemSignal_3_0.createSystemSignal(dtName, constRef, signalLength, arsiSig.Itemproperties.ItemName, primDT.DTType);
                        break;
                }
            }
            else
            {
                if (signal.SubSignals.Count > 0)
                {
                    var recDT = RecordDataType_3_0.convertCBDSRecordType(signal);
                    List<string> recElemDTName = createPortRecordDT(recDT, out dtName);
                    List<string> subConstDefName = ConstantCreator.createRecordConstDef(recDT, recElemDTName, signalName, dtName, out constDefName);
                    SystemSignal_3_0.createSystemSignalGroup(signal.Itemproperties.ItemName,  recElemDTName.ToArray(), subConstDefName.ToArray(), recDT);
                    dtElemTypeRef.DEST = DATATYPESUBTYPESENUM.RECORDTYPE;
                }
                else
                {
                    dtElemTypeRef.DEST = DATATYPESUBTYPESENUM.INTEGERTYPE;
                    var primDT = IntegerDataType_3_0.convertCBDSSignal(signal);
                    dtName = createPortIntegerDT(primDT);
                    constDefName = ConstantCreator.createIntegerConstantDef(signal, dtName, primDT.DTType);
                    var constRef = $"{constDefName}";
                    var signalLength = primDT.DTType == ARSIDataTypeDef.STRING ? primDT.dataTypeAttr[0].AtributeValue : primDT.DtCompuMethod.SignalLength;
                    SystemSignal_3_0.createSystemSignal(dtName, constRef, signalLength, signal.Itemproperties.ItemName, primDT.DTType);
                }
            }
            dtElemTypeRef.Value = $"/DataType/{dtName}";
            dtElem.TYPETREF = dtElemTypeRef;
            dtElems[0] = dtElem;
            sndRcvInt.DATAELEMENTS = dtElems;
            Extract_3_0.addObjToPkg("PortInterface", sndRcvInt);
            var portPath = new PortElementRefs();
            portPath.portIntPath = $"{sndRcvInt.SHORTNAME}/{dtElem.SHORTNAME}";
            portPath.constPath = constDefName;
            if(portPath.portIntPath == null || portPath.constPath == null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, "port references are empty");
            }
            return portPath;
        }

        internal static List<string> createPortRecordDT(RecordDataType recDT, out string recDTName)
        {
            
            int i = 0;
            string[] recElemDTNames = new string[recDT.PrimitiveDataTypeCollection.Count]; // collection to hold names of created datatypesemantics for record signal 
                    //string[] dtTypesNames = new string[recDT.PrimitiveDataTypeCollection.Count];
            foreach (PrimitiveDataType prmDT in recDT.PrimitiveDataTypeCollection)
            {
                ///compuWraper = prmDT.DtCompuMethod;
                //compuShortName = compuWraper.ShortName; //Itemproperties.ItemName ;
                var elemDtName = createPortIntegerDT(prmDT); //DTSemanticsCreator.createDataTypeSemantics(compuWraper, compuShortName);
                recElemDTNames[i++] = elemDtName;
                if (prmDT.DTType == ARSIDataTypeDef.PRIMITIVE)
                {
                    //var dtMemNames = IntegerDataType_3_0.createIntegerTypeDT(dtSemName[i], shrtName, upperLimit, lowerLimit);
                    
                }
                
            }
            RecordDataType_3_0.setRecDT(recDT);
            var reElemNames = RecordDataType_3_0.createRecordTypeDT(recElemDTNames, recDT.Itemproperties.ItemName,  out recDTName);
            return reElemNames;
        }
        private static string createPortIntegerDT(PrimitiveDataType prmDT)
        {
            if (prmDT.DTType == ARSIDataTypeDef.STRING)
            {
                var strDTName = prmDT.PartDefObjName.EndsWith("_T") ? prmDT.PartDefObjName : prmDT.PartDefObjName + "_T";
                var strDTObj = Extract_3_0.getObjFromPkg("DataType", strDTName);
                if (strDTObj == null)
                {
                    var stringLiteral = new STRINGTYPE();
                    stringLiteral.SHORTNAME = $"{prmDT.PartDefObjName}";
                    stringLiteral.ENCODING = "ISO-8859-1";
                    stringLiteral.MAXNUMBEROFCHARS = prmDT.getAttrVal("ARSR") != null ? prmDT.getAttrVal("ARSR") : "";
                    Extract_3_0.addObjToPkg("DataType", stringLiteral);
                    return stringLiteral.SHORTNAME;
                    
                }
                else
                {
                    return ((STRINGTYPE)strDTObj).SHORTNAME;
                }
            }
            else
            {
                string compuShortName;
                compuShortName = DTSemanticsCreator.createDataTypeSemantics(prmDT.DtCompuMethod, prmDT.DtCompuMethod.ShortName);
                return IntegerDataType_3_0.createIntegerTypeDT(compuShortName, prmDT.PartDefObjName, prmDT.DtCompuMethod.CompuUpperLimit, prmDT.DtCompuMethod.CompuLowerLimit);
            }

        }

        internal static SENDERRECEIVERINTERFACE findExistingPortInt(string exstnPIName)
        {
            autosar.ARPACKAGE PortIntefacePkg = Extract_3_0.getARPackage("PortInterface");
            foreach (object item in PortIntefacePkg.ELEMENTS)
            {
                SENDERRECEIVERINTERFACE srInt = item as SENDERRECEIVERINTERFACE;
                string portName = srInt.SHORTNAME;
                portName = portName.EndsWith("_I") ? portName : $"{portName}_I";
                if ((!string.IsNullOrEmpty(portName)) && portName.Equals($"{exstnPIName}_I"))
                {
                    return srInt;
                }
            }
            return null;
            //
        }

        internal static string updatePITypeRef(string exstnPIName, string newTypeRef)
        {
            autosar.ARPACKAGE PortIntefacePkg = Extract_3_0.getARPackage("PortInterface");
            foreach (object item in PortIntefacePkg.ELEMENTS)
            {
                SENDERRECEIVERINTERFACE srInt = item as SENDERRECEIVERINTERFACE;
                string portName = srInt.SHORTNAME;
                if ((!string.IsNullOrEmpty(portName)) && portName.Equals($"{exstnPIName}"))
                {
                    srInt.DATAELEMENTS[0].TYPETREF.Value = $"/DataType/{newTypeRef}";
                }
            }
            return null;
        }
    }
}
