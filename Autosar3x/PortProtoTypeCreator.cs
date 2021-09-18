using System;
using System.Collections.Generic;
using Common.SysWItems;
using autosar;
using System.Linq;
using Common;
using System.Windows;

namespace EcuExtractCreator.Autosar3x
{
    public static class PortProtoTypeCreator
    {
        public static object CreateComponentPortDef(SysWSignalDefinition signal)
        {
            object portObj;
            PortElementRefs portPaths = new PortElementRefs();
            
            var extnPort = PortElementsCreator.findExistingPortInt(signal.PartDefObjName); // checks if its a reused port
            if (extnPort != null)
            {
                if (!validatePortDTs(extnPort, signal))
                {
                    //TODO add this to snippet as option in checkbox
                    GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"found port interface with same name but different datatype for signal {signal.Itemproperties.ItemName}");
                    if (!ExtractEngineProps.OvrWrtExstnPIDefs)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"reuse port interface {extnPort.SHORTNAME} for signal {signal.Itemproperties.ItemName}",
                             "confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        if (result == MessageBoxResult.No)
                        {
                            GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"found port interface with same name but different definition for signal {signal.Itemproperties.ItemName}");
                            throw new BadReferenceException($"found port interface with same name but different definition for signal {signal.Itemproperties.ItemName}");
                        }
                    }
                    else
                    {
                        GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"overwriting port interface definition {signal.Itemproperties.ItemName} with latest");
                        GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"overwriting port interface definition {signal.Itemproperties.ItemName} with latest");
                        PortElementsCreator.updatePITypeRef(extnPort.SHORTNAME, signal.getDTName());
                    }

                }
                string PIDTName = extnPort.DATAELEMENTS[0].TYPETREF.Value.Split('/').Last(); //signal.GetType().Name == "SysWSignalDefinition"? signal.PartDefObjName : (signal as SysWSignalDefinitionARSI).DatatypeDefs[0].PartDefObjName;

                var extnConst = new CONSTANTSPECIFICATION();
                bool constFound = ConstantCreator.findExistingConst(signal.Itemproperties.ItemName, PIDTName, out extnConst);

                // TODO this needs to be rewritten to check for existing constant and creating constant based on it
                if (constFound == true && extnConst != null)
                {
                    GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"found existing constant and port definition for {signal.Itemproperties.ItemName}");
                    GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing constant definition for {signal.Itemproperties.ItemName}");
                    portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                    portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";
                    crtExstnPortSysSig(signal, extnPort, extnConst);
                    //if(signal)
                }
                /*
                 * Some cbds signals have PIs and Constants defined with their DTNames
                 */
                if (constFound == false && extnConst != null)
                {
                    if (!ExtractEngineProps.OvrWrtExstnPIDefs)
                    {
                        MessageBoxResult result = MessageBox.Show(
                            $"reuse constant definition {extnConst.SHORTNAME} for signal {signal.Itemproperties.ItemName}",
                             "confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
                        
                        if (result == MessageBoxResult.No)
                        {
                            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"found existing constant with different definition for constant for signal {signal.Itemproperties.ItemName}");
                            throw new BadReferenceException($"found existing constant with different definition for signal {signal.Itemproperties.ItemName}");
                        }
                        portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                        portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";

                    }
                    else
                    {
                        ConstantCreator.updateConstTypeRef(extnConst.SHORTNAME, signal.getDTName());
                        portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                        portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";
                    }
                }

                if(constFound == false && extnConst == null)
                {
                    //in 
                    var constName = PIDTName.EndsWith("_T") ? PIDTName.Substring(0, PIDTName.Length - 2) : PIDTName;
                    constFound = ConstantCreator.findExistingConst(constName, PIDTName, out extnConst);
                    if (constFound == true && extnConst != null)
                    {
                        GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"found existing constant and port definition for {signal.Itemproperties.ItemName}");
                        GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing constant definition for {signal.Itemproperties.ItemName}");
                        portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                        portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";
                        crtExstnPortSysSig(signal, extnPort, extnConst);
                        //if(signal)
                    }
                    else
                    {
                        // try with partdefobjname
                        constName = signal.PartDefObjName.EndsWith("_T") ? signal.PartDefObjName.Substring(0, signal.PartDefObjName.Length - 2) : signal.PartDefObjName;
                        constFound = ConstantCreator.findExistingConst(constName, PIDTName, out extnConst);
                        if (constFound == true && extnConst != null)
                        {
                            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.WARNING, $"found existing constant defined with a partdefObj {signal.Itemproperties.ItemName}");
                            GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"found existing constant defined with a partdefObj {signal.Itemproperties.ItemName}");
                            GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing constant definition for {signal.Itemproperties.ItemName}");
                            portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                            portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";
                            crtExstnPortSysSig(signal, extnPort, extnConst);
                            //if(signal)
                        }
                        else
                        {
                            constName = signal.PartDefObjName.EndsWith("_T") ? signal.PartDefObjName.Substring(0, signal.PartDefObjName.Length - 2) : signal.PartDefObjName;
                            var pdoAsDTName = signal.PartDefObjName.EndsWith("_T") ? signal.PartDefObjName  : $"{signal.PartDefObjName}_T";

                            constFound = ConstantCreator.findExistingConst(constName, pdoAsDTName, out extnConst);
                            if (constFound == true && extnConst != null)
                            {
                                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.WARNING, $"found existing constant defined with a partdefObj {signal.Itemproperties.ItemName}");
                                GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"found existing constant defined with a partdefObj {signal.Itemproperties.ItemName}");
                                GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing constant definition for {signal.Itemproperties.ItemName}");
                                portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                                portPaths.constPath = $"{extnConst.SHORTNAME}/{ExtractorUtilities.getObjPropStringVal(extnConst.VALUE.Item, "SHORTNAME")}";
                                crtExstnPortSysSig(signal, extnPort, extnConst);
                                //if(signal)
                            }
                            else
                            {
                                // now create new constant if all option are exhausted
                                portPaths.portIntPath = $"{extnPort.SHORTNAME}/{extnPort.DATAELEMENTS[0].SHORTNAME}";
                                portPaths.constPath = PortProtoHelper.crtExstnConstant(signal, PIDTName);
                                
                            }
                            

                        }
                    }
                    
                }
                
            }
            else
            {
                portPaths = PortElementsCreator.createPortElementRefs(signal);
            }

            portObj = createXPortPrototype(signal, portPaths);
            return portObj;
        }

        private static object createXPortPrototype(SysWSignalDefinition signal, PortElementRefs portPaths)
        {
            object portObj;
            VALUESPECIFICATIONSUBTYPESENUM initValRefDef;
            if (signal.getSignalType() == "RecordType")
            {
                initValRefDef = VALUESPECIFICATIONSUBTYPESENUM.RECORDSPECIFICATION;
            }
            else
            {
                initValRefDef = VALUESPECIFICATIONSUBTYPESENUM.INTEGERLITERAL;
            }
            if (signal.PortDir.Equals("SendPort"))
            {

                PPORTPROTOTYPE pport = new PPORTPROTOTYPE();
                pport.SHORTNAME = signal.Itemproperties.ItemName;
                autosar.UNQUEUEDSENDERCOMSPEC comSpec = new autosar.UNQUEUEDSENDERCOMSPEC();
                autosar.UNQUEUEDSENDERCOMSPECDATAELEMENTREF comSpecRef = new autosar.UNQUEUEDSENDERCOMSPECDATAELEMENTREF();
                comSpecRef.DEST = autosar.DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                comSpecRef.Value = $"/PortInterface/{portPaths.portIntPath}";
                comSpec.CANINVALIDATE = false;
                autosar.UNQUEUEDSENDERCOMSPECINITVALUEREF initValRef = new autosar.UNQUEUEDSENDERCOMSPECINITVALUEREF();
                initValRef.Value = $"/Constant/{portPaths.constPath}";
                initValRef.DEST = initValRefDef;
                autosar.PPORTPROTOTYPEPROVIDEDINTERFACETREF pportInterface = new autosar.PPORTPROTOTYPEPROVIDEDINTERFACETREF();
                pportInterface.DEST = PORTINTERFACESUBTYPESENUM.SENDERRECEIVERINTERFACE;
                pportInterface.Value = $"/PortInterface/{portPaths.portIntPath.Split('/')[0]}";
                comSpec.DATAELEMENTREF = comSpecRef;
                comSpec.INITVALUEREF = initValRef;
                pport.PROVIDEDCOMSPECS = new object[1];
                pport.PROVIDEDCOMSPECS[0] = comSpec;
                pport.PROVIDEDINTERFACETREF = pportInterface;
                portObj = pport;
                //portsNode[portsCount++] = pport;

            }
            else
            {
                autosar.RPORTPROTOTYPE rport = new autosar.RPORTPROTOTYPE();
                rport.SHORTNAME = signal.Itemproperties.ItemName;
                autosar.UNQUEUEDRECEIVERCOMSPEC rComSpec = new autosar.UNQUEUEDRECEIVERCOMSPEC();
                rComSpec.ALIVETIMEOUT = 0;
                rComSpec.RESYNCTIME = 0;
                autosar.UNQUEUEDRECEIVERCOMSPECDATAELEMENTREF rComSpecRef = new autosar.UNQUEUEDRECEIVERCOMSPECDATAELEMENTREF();
                rComSpecRef.DEST = autosar.DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                rComSpecRef.Value = $"/PortInterface/{portPaths.portIntPath}";
                autosar.UNQUEUEDRECEIVERCOMSPECINITVALUEREF initValRef = new autosar.UNQUEUEDRECEIVERCOMSPECINITVALUEREF();
                initValRef.DEST = initValRefDef;
                initValRef.Value = $"/Constant/{portPaths.constPath}";
                rComSpec.DATAELEMENTREF = rComSpecRef;
                rComSpec.INITVALUEREF = initValRef;
                var rportInterface = new RPORTPROTOTYPEREQUIREDINTERFACETREF();
                rportInterface.DEST = PORTINTERFACESUBTYPESENUM.SENDERRECEIVERINTERFACE;
                rportInterface.Value = $"/PortInterface/{portPaths.portIntPath.Split('/')[0]}";
                rport.REQUIREDINTERFACETREF = rportInterface;
                rport.REQUIREDCOMSPECS = new object[1];
                rport.REQUIREDCOMSPECS[0] = rComSpec;
                //portsNode[portsCount++] 
                portObj = rport;
            }

            return portObj;
        }


        private static bool validatePortDTs(SENDERRECEIVERINTERFACE extnPort, SysWSignalDefinition signal)
        {
            var exstnPortDTTypref = extnPort.DATAELEMENTS[0].TYPETREF.Value;

            var exstnPortDTName = exstnPortDTTypref.Split('/').Last();
            exstnPortDTName = exstnPortDTName.EndsWith("_T") ? exstnPortDTName : $"{exstnPortDTName}_T";
            string dtName;
            if(signal.GetType().Name == "SysWSignalDefinition")
            {
                return true;
                //dtName = signal.PartDefObjName.EndsWith("_T") ? signal.PartDefObjName : $"{signal.PartDefObjName}_T";
                //if(exstnPortDTName.Contains(extnPort.DATAELEMENTS[0].SHORTNAME))
            }
            else
            {
                SysWSignalDefinitionARSI arsiSig = signal as SysWSignalDefinitionARSI;
                Autosar4xDatatypeDef autDtType = arsiSig.DatatypeDefs[0];// as PrimitiveDataType;
                dtName = autDtType.PartDefObjName.EndsWith("_T")? autDtType.PartDefObjName : $"{autDtType.PartDefObjName}_T";
                return dtName.Equals(exstnPortDTName, StringComparison.CurrentCultureIgnoreCase);
            }

            
            //throw new NotImplementedException();
        }

        public static SENDERRECEIVERINTERFACE getPortInterfaceDef(string portInterfaceName)
        {
            var arPkg = Extract_3_0.getARPackage("PortInterface");
            foreach(SENDERRECEIVERINTERFACE sndrRcvrInt in arPkg.ELEMENTS)
            {
                if (sndrRcvrInt.SHORTNAME.Equals(portInterfaceName, StringComparison.CurrentCultureIgnoreCase)) { }
                    return sndrRcvrInt;
            }
            return null;
        }

        public static object getPortDataType(string portInterfaceName)
        {
            var sndrRcvrInt = getPortInterfaceDef(portInterfaceName);
            var dtName = sndrRcvrInt.DATAELEMENTS[0].TYPETREF.Value;
            var dtArPkg = Extract_3_0.getARPackage("DataType");
            foreach(object obj in dtArPkg.ELEMENTS)
            {
                string dtShortName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                if(dtShortName.Equals(dtName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return obj;
                }
            }
            return null;
        }

        
        public static void deleteReferencedPIs(IEnumerable<object> ports, string compName, ExtractInfo extractInfo)
        {
            if (!ExtractEngineProps.exstnPIOpts)
            {
                foreach (object obj in ports)
                {
                    string objShortName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                    var PIRefs = PortProtoHelper.getPortPIReference(obj);
                    bool remove = PortProtoHelper.checkPortUsage(PIRefs.portIntPath, compName);

                    if (remove)
                    {
                        deletePortReferences(PIRefs.portIntPath, PIRefs.constPath);
                    }
                    else
                    {
                        GlobalDefs.logMessageToFile(LOGMSGCLASS.WARNING, $"portsinterface { PIRefs.portIntPath} referenced by {compName} is not deleted");
                    }
                    SystemSignal_3_0.removeSystemSignal(objShortName);
                    //DataMapping.removeDataMapping(objShortName);
                }
            }
            
            
        }

        
        private static void deletePortReferences(string portName, string constRef)
        {
            autosar.ARPACKAGE portARPkg = Extract_3_0.getARPackage("PortInterface");
            foreach (object pObj in portARPkg.ELEMENTS)
            {
                string pObjShortName = pObj.GetType().GetProperty("SHORTNAME").GetValue(pObj).ToString();
                if (pObjShortName.Equals($"{portName}"))
                {
                    var sndrRcvrInt = pObj as SENDERRECEIVERINTERFACE;
                    var dtName = sndrRcvrInt.DATAELEMENTS[0].SHORTNAME;
                    var tempDTSemName = sndrRcvrInt.DATAELEMENTS[0].TYPETREF.Value.Split('/')[2];
                    if (!DataTypes_3_0.genericDTNames.Contains(tempDTSemName))
                        DataTypes_3_0.removeDataType(dtName, tempDTSemName, portName);
                    Extract_3_0.removeObjFromPkg("Constant", $"{constRef}"); // this is making an assumption on constant name
                    Extract_3_0.removeObjFromPkg("PortInterface", pObjShortName);
                    break;
                }
            }
        }

        private static void crtExstnPortSysSig(SysWSignalDefinition signal, SENDERRECEIVERINTERFACE extnPort, CONSTANTSPECIFICATION extnConst)
        {
            //string DTName = extnPort.DATAELEMENTS[0].TYPETREF.Value.Split('/').Last();
            if (signal.getSignalType() == "PrimitiveType")
            {
                //string DTName = extnPort.DATAELEMENTS[0].TYPETREF.Value.Split('/').Last();
                var primDT = signal.GetType().Name == "SysWSignalDefinition"?IntegerDataType_3_0.convertCBDSSignal(signal)
                                                       : (signal as SysWSignalDefinitionARSI).DatatypeDefs[0] as PrimitiveDataType;
                
                SystemSignal_3_0.createSystemSignal(primDT.PartDefObjName, extnConst.SHORTNAME, primDT.DtCompuMethod.SignalLength, signal.Itemproperties.ItemName, primDT.DTType);
            }
            else
            {
                string DTName = extnPort.DATAELEMENTS[0].TYPETREF.Value.Split('/').Last();
                var recDT = RecordDataType_3_0.convertCBDSRecordType(signal);
                var dtDef = Extract_3_0.getObjFromPkg("DataType", DTName) as RECORDTYPE;
               
                List<string> recConstElemNames = new List<string>();
                List<string> recElemDTNames = new List<string>();

                foreach (RECORDELEMENT reElem in dtDef.ELEMENTS)
                {
                    //recElemNames.Add(reElem.SHORTNAME);
                    recElemDTNames.Add(reElem.TYPETREF.Value.Split('/').Last());
                }
                var recConstElems = extnConst.VALUE.Item as RECORDSPECIFICATION;
                foreach (object reElem in recConstElems.ELEMENTS)
                {
                    recConstElemNames.Add(reElem.GetType().GetProperty("SHORTNAME").GetValue(reElem).ToString());
                    //recElemDTNames.Add(reElem.TYPETREF.Value.Split('/').Last());
                }

                //List<string> constDefNames = new List<string>();
                
                SystemSignal_3_0.createSystemSignalGroup(signal.Itemproperties.ItemName,  recElemDTNames.ToArray(), recConstElemNames.ToArray(), recDT);
            }
            //throw new NotImplementedException();
        }

    }
    public struct PortElementRefs
    {
        public string constPath { get; set; }
        public string portIntPath { get; set; }
    }
}
