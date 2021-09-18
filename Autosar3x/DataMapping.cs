using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using autosar;
using Common.SysWItems;

namespace EcuExtractCreator.Autosar3x
{
    public static class DataMapping
    {
        public static void createAppSWDataMapping(SysWLDCDefinition component)
        {
            var sndrRcvrLists =  new List<SENDERRECEIVERTOSIGNALMAPPING>();
            foreach (SysWSignalDefinition signal in component.SignalParts) 
            {
                if(signal.GetType().Name == "SysWSignalDefinition")
                {
                    if (signal.SubSignals.Count > 0)
                    {
                        var recElemNames = PortElementsCreator.recElemsName;
                        crtAppSWSndrRcvrToSigGrpMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, recElemNames);
                    }
                    else
                    {
                        var sndrRcvr = crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                        sndrRcvrLists.Add(sndrRcvr);
                    }
                }
                else
                {
                    SysWSignalDefinitionARSI arsiSig = signal as SysWSignalDefinitionARSI;
                    Autosar4xDatatypeDef autDtType = arsiSig.DatatypeDefs[0];// as PrimitiveDataType;
                    if (autDtType.GetType().Name == "RecordDataType")
                    {
                        var recElemNames = PortElementsCreator.recElemsName;
                         crtAppSWSndrRcvrToSigGrpMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, recElemNames);
                    }
                    else
                    {
                        var sndrRcvr = crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                        sndrRcvrLists.Add(sndrRcvr);
                    }
                    //crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                }
            }
        }

        public static void createCompositeDataMapping(SysWLCCDefinition component)
        {
            var sndrRcvrLists = new List<SENDERRECEIVERTOSIGNALMAPPING>();
            foreach(SysWLDCDefinition ldcs in component.LCParts)
            {
                foreach (SysWSignalDefinition signal in ldcs.SignalParts)
                {
                    if (signal.GetType().Name == "SysWSignalDefinition")
                    {
                        if (signal.SubSignals.Count > 0)
                        {
                            crtCompositeSndrRcvrToSigMapg(component.Itemproperties.ItemName, ldcs.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                        }
                        else
                        {
                            var sndrRcvr = crtCompositeSndrRcvrToSigMapg(component.Itemproperties.ItemName, ldcs.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                            sndrRcvrLists.Add(sndrRcvr);
                        }
                    }
                    else
                    {
                        SysWSignalDefinitionARSI arsiSig = signal as SysWSignalDefinitionARSI;
                        Autosar4xDatatypeDef autDtType = arsiSig.DatatypeDefs[0];// as PrimitiveDataType;
                        if (autDtType.GetType().Name == "RecordDataType")
                        {
                            //crtAppSWSndrRcvrToSigGrpMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, recElemNames);
                        }
                        else
                        {
                            var sndrRcvr = crtCompositeSndrRcvrToSigMapg(component.Itemproperties.ItemName, ldcs.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                            sndrRcvrLists.Add(sndrRcvr);
                        }
                        //crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                    }
                }
            }
            foreach (SysWSignalDefinition signal in component.SignalParts)
            {
                if (signal.GetType().Name == "SysWSignalDefinition")
                {
                    if (signal.SubSignals.Count > 0)
                    {
                        //crtAppSWSndrRcvrToSigGrpMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                    }
                    else
                    {
                        var sndrRcvr = crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                        sndrRcvrLists.Add(sndrRcvr);
                    }
                }
                else
                {
                    SysWSignalDefinitionARSI arsiSig = signal as SysWSignalDefinitionARSI;
                    Autosar4xDatatypeDef autDtType = arsiSig.DatatypeDefs[0];// as PrimitiveDataType;
                    if (autDtType.GetType().Name == "RecordDataType")
                    {
                        //crtAppSWSndrRcvrToSigGrpMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                    }
                    else
                    {
                        var sndrRcvr = crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                        sndrRcvrLists.Add(sndrRcvr);
                    }
                    //crtAppSWSndrRcvrToSigMapg(component.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                }
            }
        }
        public static SENDERRECEIVERTOSIGNALMAPPING crtAppSWSndrRcvrToSigMapg(string componentName, string signalName, string portDir)
        {
            var exstnSignalObj = Extract_3_0.getObjFromPkg("Signal", signalName);
            if (exstnSignalObj != null)
            {
                var senderRecSigMap = new SENDERRECEIVERTOSIGNALMAPPING();
                //string 
                var dataElemIref = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREF();

                dataElemIref.SOFTWARECOMPOSITIONREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFSOFTWARECOMPOSITIONREF();
                dataElemIref.SOFTWARECOMPOSITIONREF.DEST = SOFTWARECOMPOSITIONSUBTYPESENUM.SOFTWARECOMPOSITION;
                dataElemIref.SOFTWARECOMPOSITIONREF.Value = $"/VehicleProject/{ Common.GlobalDefs.ECU}/TopLevelComposition";
                dataElemIref.COMPONENTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF[1];
                var compProtoRef = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                compProtoRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                compProtoRef.Value = $"/ComponentType/TopLevelComposition/{componentName}";
                dataElemIref.COMPONENTPROTOTYPEREF[0] = compProtoRef;
                dataElemIref.PORTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFPORTPROTOTYPEREF();
                dataElemIref.PORTPROTOTYPEREF.DEST = portDir == "SendPort" ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                dataElemIref.PORTPROTOTYPEREF.Value = $"/ComponentType/{componentName}/{signalName}";
                dataElemIref.DATAELEMENTREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFDATAELEMENTREF();
                dataElemIref.DATAELEMENTREF.DEST = DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                dataElemIref.DATAELEMENTREF.Value = $"/PortInterface/{signalName}_I/{signalName}";
                //if(component.GetType().Name == "SysW")
                //SOFTWARECOMPOSITIONSOFTWARECOMPOSITIONTREF

                senderRecSigMap.DATAELEMENTIREF = dataElemIref;
                senderRecSigMap.SIGNALREF = new SENDERRECEIVERTOSIGNALMAPPINGSIGNALREF();
                senderRecSigMap.SIGNALREF.DEST = SYSTEMSIGNALSUBTYPESENUM.SYSTEMSIGNAL;
                senderRecSigMap.SIGNALREF.Value = $"/Signal/{signalName}";
                addDataMappingToXtract(senderRecSigMap);
                return senderRecSigMap;
            }
            return null;
        }
        public static SENDERRECEIVERTOSIGNALMAPPING crtCompositeSndrRcvrToSigMapg(string lcccompName,  string componentName, string signalName, string portDir)
        {
            var exstnSignalObj = Extract_3_0.getObjFromPkg("Signal", signalName);
            if (exstnSignalObj != null)
            {
                var senderRecSigMap = new SENDERRECEIVERTOSIGNALMAPPING();
                //string 
                var dataElemIref = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREF();

                dataElemIref.SOFTWARECOMPOSITIONREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFSOFTWARECOMPOSITIONREF();
                dataElemIref.SOFTWARECOMPOSITIONREF.DEST = SOFTWARECOMPOSITIONSUBTYPESENUM.SOFTWARECOMPOSITION;
                dataElemIref.SOFTWARECOMPOSITIONREF.Value = $"/VehicleProject/{ Common.GlobalDefs.ECU}/TopLevelComposition";
                dataElemIref.COMPONENTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF[2];
                var lccCompProtoRef = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                lccCompProtoRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                lccCompProtoRef.Value = $"/ComponentType/TopLevelComposition/{lcccompName}";
                dataElemIref.COMPONENTPROTOTYPEREF[0] = lccCompProtoRef;
                var compProtoRef = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                compProtoRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                compProtoRef.Value = $"/ComponentType/{lcccompName}/{componentName}";
                dataElemIref.COMPONENTPROTOTYPEREF[1] = compProtoRef;
                dataElemIref.PORTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFPORTPROTOTYPEREF();
                dataElemIref.PORTPROTOTYPEREF.DEST = portDir == "SendPort" ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                dataElemIref.PORTPROTOTYPEREF.Value = $"/ComponentType/{componentName}/{signalName}";
                dataElemIref.DATAELEMENTREF = new SENDERRECEIVERTOSIGNALMAPPINGDATAELEMENTIREFDATAELEMENTREF();
                dataElemIref.DATAELEMENTREF.DEST = DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                dataElemIref.DATAELEMENTREF.Value = $"/PortInterface/{signalName}_I/{signalName}";
                //if(component.GetType().Name == "SysW")
                //SOFTWARECOMPOSITIONSOFTWARECOMPOSITIONTREF

                senderRecSigMap.DATAELEMENTIREF = dataElemIref;
                senderRecSigMap.SIGNALREF = new SENDERRECEIVERTOSIGNALMAPPINGSIGNALREF();
                senderRecSigMap.SIGNALREF.DEST = SYSTEMSIGNALSUBTYPESENUM.SYSTEMSIGNAL;
                senderRecSigMap.SIGNALREF.Value = $"/Signal/{signalName}";
                addDataMappingToXtract(senderRecSigMap);
                return senderRecSigMap;
            }
            return null;
           
        }
        public static void crtAppSWSndrRcvrToSigGrpMapg( string componentName, string signalName, string portDir, List<string>recELemNames)
        {
            var exstnSignalGrpObj = Extract_3_0.getObjFromPkg("SignalGroup", $"{signalName}_sg");
            if (exstnSignalGrpObj != null)
            {
                var senderRecvrgrpSigMap = new SENDERRECEIVERTOSIGNALGROUPMAPPING();
                //string 
                var dataElemIref = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREF();

                dataElemIref.SOFTWARECOMPOSITIONREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFSOFTWARECOMPOSITIONREF();
                dataElemIref.SOFTWARECOMPOSITIONREF.DEST = SOFTWARECOMPOSITIONSUBTYPESENUM.SOFTWARECOMPOSITION;
                dataElemIref.SOFTWARECOMPOSITIONREF.Value = $"/VehicleProject/{ Common.GlobalDefs.ECU}/TopLevelComposition";
                dataElemIref.COMPONENTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF[1];
                var compProtoRef = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                compProtoRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                compProtoRef.Value = $"/ComponentType/TopLevelComposition/{componentName}";
                dataElemIref.COMPONENTPROTOTYPEREF[0] = compProtoRef;
                dataElemIref.PORTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFPORTPROTOTYPEREF();
                dataElemIref.PORTPROTOTYPEREF.DEST = portDir == "SendPort" ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                dataElemIref.PORTPROTOTYPEREF.Value = $"/ComponentType/{componentName}/{signalName}";
                dataElemIref.DATAELEMENTREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFDATAELEMENTREF();
                dataElemIref.DATAELEMENTREF.DEST = DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                dataElemIref.DATAELEMENTREF.Value = $"/PortInterface/{signalName}_I/{signalName}";
                senderRecvrgrpSigMap.DATAELEMENTIREF = dataElemIref;
                senderRecvrgrpSigMap.SIGNALGROUPREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGSIGNALGROUPREF();
                senderRecvrgrpSigMap.SIGNALGROUPREF.DEST = SYSTEMSIGNALGROUPSUBTYPESENUM.SYSTEMSIGNALGROUP;
                senderRecvrgrpSigMap.SIGNALGROUPREF.Value = $"/SignalGroup/{signalName}_sg";
                senderRecvrgrpSigMap.TYPEMAPPING = new SENDERRECEIVERTOSIGNALGROUPMAPPINGTYPEMAPPING();
                var sndrRecRecTypemapping = new SENDERRECRECORDTYPEMAPPING();
                var sndrRecRecTypeElems = new List<SENDERRECRECORDELEMENTMAPPING>();
                foreach (string recElems in recELemNames)
                {
                    var recElemTypeMapping = new SENDERRECRECORDELEMENTMAPPING();
                    recElemTypeMapping.RECORDELEMENTREF = new SENDERRECRECORDELEMENTMAPPINGRECORDELEMENTREF();
                    recElemTypeMapping.RECORDELEMENTREF.DEST = RECORDELEMENTSUBTYPESENUM.RECORDELEMENT;
                    recElemTypeMapping.RECORDELEMENTREF.Value = $"/Signal/{recElems}_RE";
                    recElemTypeMapping.SIGNALREF = new SENDERRECRECORDELEMENTMAPPINGSIGNALREF();
                    recElemTypeMapping.SIGNALREF.DEST = SYSTEMSIGNALSUBTYPESENUM.SYSTEMSIGNAL;
                    recElemTypeMapping.SIGNALREF.Value = $"/Signal/{signalName}_{recElems}";
                    sndrRecRecTypeElems.Add(recElemTypeMapping);
                }
                sndrRecRecTypemapping.RECORDELEMENTMAPPINGS = sndrRecRecTypeElems.ToArray();
                senderRecvrgrpSigMap.TYPEMAPPING.Item = sndrRecRecTypemapping;
                addDataMappingToXtract(senderRecvrgrpSigMap);
            }

        }

       

        public static void crtCompositeSndrRcvrToSigGrpMapg(string lcccompName, string componentName, string signalName, string portDir, List<string> recELemNames)
        {
            var exstnSignalGrpObj = Extract_3_0.getObjFromPkg("SignalGroup", $"{signalName}_sg");
            if (exstnSignalGrpObj != null)
            {
                var senderRecvrgrpSigMap = new SENDERRECEIVERTOSIGNALGROUPMAPPING();
                //string 
                var dataElemIref = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREF();

                dataElemIref.SOFTWARECOMPOSITIONREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFSOFTWARECOMPOSITIONREF();
                dataElemIref.SOFTWARECOMPOSITIONREF.DEST = SOFTWARECOMPOSITIONSUBTYPESENUM.SOFTWARECOMPOSITION;
                dataElemIref.SOFTWARECOMPOSITIONREF.Value = $"/VehicleProject/{ Common.GlobalDefs.ECU}/TopLevelComposition";
                dataElemIref.COMPONENTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF[2];
                var compProtoRef = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                compProtoRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                compProtoRef.Value = $"/ComponentType/TopLevelComposition/{lcccompName}";
                dataElemIref.COMPONENTPROTOTYPEREF[0] = compProtoRef;
                var compProtoRef2 = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFCOMPONENTPROTOTYPEREF();
                compProtoRef2.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                compProtoRef2.Value = $"/ComponentType/{componentName}";
                dataElemIref.COMPONENTPROTOTYPEREF[0] = compProtoRef2;
                dataElemIref.PORTPROTOTYPEREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFPORTPROTOTYPEREF();
                dataElemIref.PORTPROTOTYPEREF.DEST = portDir == "SendPort" ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                dataElemIref.PORTPROTOTYPEREF.Value = $"/ComponentType/{componentName}/{signalName}";
                dataElemIref.DATAELEMENTREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGDATAELEMENTIREFDATAELEMENTREF();
                dataElemIref.DATAELEMENTREF.DEST = DATAELEMENTPROTOTYPESUBTYPESENUM.DATAELEMENTPROTOTYPE;
                dataElemIref.DATAELEMENTREF.Value = $"/PortInterface/{signalName}_I/{signalName}";
                senderRecvrgrpSigMap.DATAELEMENTIREF = dataElemIref;
                senderRecvrgrpSigMap.SIGNALGROUPREF = new SENDERRECEIVERTOSIGNALGROUPMAPPINGSIGNALGROUPREF();
                senderRecvrgrpSigMap.SIGNALGROUPREF.DEST = SYSTEMSIGNALGROUPSUBTYPESENUM.SYSTEMSIGNALGROUP;
                senderRecvrgrpSigMap.SIGNALGROUPREF.Value = $"/SignalGroup/{signalName}_sg";
                senderRecvrgrpSigMap.TYPEMAPPING = new SENDERRECEIVERTOSIGNALGROUPMAPPINGTYPEMAPPING();
                var sndrRecRecTypemapping = new SENDERRECRECORDTYPEMAPPING();
                var sndrRecRecTypeElems = new List<SENDERRECRECORDELEMENTMAPPING>();
                foreach (string recElems in recELemNames)
                {
                    var recElemTypeMapping = new SENDERRECRECORDELEMENTMAPPING();
                    recElemTypeMapping.RECORDELEMENTREF = new SENDERRECRECORDELEMENTMAPPINGRECORDELEMENTREF();
                    recElemTypeMapping.RECORDELEMENTREF.DEST = RECORDELEMENTSUBTYPESENUM.RECORDELEMENT;
                    recElemTypeMapping.RECORDELEMENTREF.Value = $"/Signal/{recElems}_RE";
                    recElemTypeMapping.SIGNALREF = new SENDERRECRECORDELEMENTMAPPINGSIGNALREF();
                    recElemTypeMapping.SIGNALREF.DEST = SYSTEMSIGNALSUBTYPESENUM.SYSTEMSIGNAL;
                    recElemTypeMapping.SIGNALREF.Value = $"/Signal/{signalName}_{recElems}";
                    sndrRecRecTypeElems.Add(recElemTypeMapping);
                }
                sndrRecRecTypemapping.RECORDELEMENTMAPPINGS = sndrRecRecTypeElems.ToArray();
                senderRecvrgrpSigMap.TYPEMAPPING.Item = sndrRecRecTypemapping;
                addDataMappingToXtract(senderRecvrgrpSigMap);
            }
           
        }

        

        private static void addDataMappingToXtract(object senderRecvrgrpSigMap)
        {
            ARPACKAGE vehicleProj = Extract_3_0.getARPackage("VehicleProject");
            var vehiclePrjSystem = vehicleProj.ELEMENTS[0] as SYSTEM;
            var dataMapping = vehiclePrjSystem.MAPPING.DATAMAPPINGS;
            List<object> dataMappings = dataMapping.ToList();
            dataMappings.Add(senderRecvrgrpSigMap);
            dataMapping = dataMappings.ToArray();
            vehiclePrjSystem.MAPPING.DATAMAPPINGS = dataMapping.ToArray();
            Extract_3_0.editTopNode(vehicleProj);
            //Utilities.addDefToNode(system, "VehicleProject");
        }

        internal static void removeSWCImplMaps(string shortName)
        {
            ARPACKAGE vehicleProj = Extract_3_0.getARPackage("VehicleProject"); //old code
            var vehiclePrjSystem = vehicleProj.ELEMENTS[0] as SYSTEM;
            var swImplMapping = vehiclePrjSystem.MAPPING.SWIMPLMAPPINGS.ToList();
            Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"removing {shortName} definition from Data Mapping");

            foreach (var swImplMapObj in vehiclePrjSystem.MAPPING.SWIMPLMAPPINGS)
            {
                if(swImplMapObj.COMPONENTIMPLEMENTATIONREF != null)
                {
                    if (ExtractorUtilities.findStringInValue(swImplMapObj.COMPONENTIMPLEMENTATIONREF.Value, $"{shortName}_Implementation"))
                    {
                        swImplMapping.Remove(swImplMapObj);
                        continue;
                    }
                }
                if(swImplMapObj.COMPONENTIREFS != null)
                {
                    foreach(var compIRefs in swImplMapObj.COMPONENTIREFS)
                    {
                        bool breakOuterLoop = false;
                        if (compIRefs.TARGETCOMPONENTPROTOTYPEREF != null && ExtractorUtilities.findStringInValue(compIRefs.TARGETCOMPONENTPROTOTYPEREF.Value, shortName))
                        {
                            swImplMapping.Remove(swImplMapObj);
                            break;
                        }
                        if(compIRefs.SOFTWARECOMPOSITIONREF != null && ExtractorUtilities.findStringInValue(compIRefs.SOFTWARECOMPOSITIONREF.Value, shortName))
                        {
                            swImplMapping.Remove(swImplMapObj);
                            break;
                        }
                        if(compIRefs.COMPONENTPROTOTYPEREF != null)
                        {
                            foreach(var compProtoTypeRef in compIRefs.COMPONENTPROTOTYPEREF)
                            {
                                if(ExtractorUtilities.findStringInValue(compProtoTypeRef.Value, shortName))
                                {
                                    swImplMapping.Remove(swImplMapObj);
                                    breakOuterLoop = true; // the main object being looked into has been removed.
                                    break;
                                }
                            }
                        }
                        if (breakOuterLoop == true) break;
                    }
                }
            }
            vehiclePrjSystem.MAPPING.SWIMPLMAPPINGS = swImplMapping.ToArray();
            vehicleProj.ELEMENTS[0] = vehiclePrjSystem;
            Extract_3_0.editTopNode(vehicleProj);
        }

        internal static void removeSWCToECUMaps(string shortName)
        {
            ARPACKAGE vehicleProj = Extract_3_0.getARPackage("VehicleProject"); //old code
            var vehiclePrjSystem = vehicleProj.ELEMENTS[0] as SYSTEM;
            var swMapping = vehiclePrjSystem.MAPPING.SWMAPPINGS.ToList();
            Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"removing {shortName} definition from SWC Implemetation Mapping");
            foreach (var swMapObj in vehiclePrjSystem.MAPPING.SWMAPPINGS)
            {
                if(swMapObj.COMPONENTIREFS != null)
                {
                    bool breakOuterLoop = false;
                    foreach(var compIRefs in swMapObj.COMPONENTIREFS)
                    {
                        if (compIRefs.TARGETCOMPONENTPROTOTYPEREF != null && ExtractorUtilities.findStringInValue(compIRefs.TARGETCOMPONENTPROTOTYPEREF.Value, shortName))
                        {
                            swMapping.Remove(swMapObj);
                            break; ;
                        }
                        if (compIRefs.SOFTWARECOMPOSITIONREF != null && ExtractorUtilities.findStringInValue(compIRefs.SOFTWARECOMPOSITIONREF.Value, shortName))
                        {
                            swMapping.Remove(swMapObj);
                            break; ;
                        }
                        if (compIRefs.COMPONENTPROTOTYPEREF != null)
                        {
                            foreach (var compProtoTypeRef in compIRefs.COMPONENTPROTOTYPEREF)
                            {
                                if (ExtractorUtilities.findStringInValue(compProtoTypeRef.Value, shortName))
                                {
                                    swMapping.Remove(swMapObj);
                                    breakOuterLoop = true;
                                    break;
                                }
                            }
                            
                        }
                        if (breakOuterLoop == true) break;
                    }
                }
            }
            vehiclePrjSystem.MAPPING.SWMAPPINGS = swMapping.ToArray();
            vehicleProj.ELEMENTS[0] = vehiclePrjSystem;
            Extract_3_0.editTopNode(vehicleProj);
        }

        public static void removeDataMapping(string sysSigName)
        {
            ARPACKAGE vehicleProj = Extract_3_0.getARPackage("VehicleProject"); //old code
            var vehiclePrjSystem = vehicleProj.ELEMENTS[0] as SYSTEM;
            var dataMapping = vehiclePrjSystem.MAPPING.DATAMAPPINGS.ToList();
            Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"removing signal {sysSigName} definition from Sender Receiver Mapping");

            foreach (var sndrRcvrMappingObj in vehiclePrjSystem.MAPPING.DATAMAPPINGS)
            {
                if (sndrRcvrMappingObj.GetType().Name.Equals("SENDERRECEIVERTOSIGNALMAPPING")){
                    var sndrRcvrMapping = sndrRcvrMappingObj as SENDERRECEIVERTOSIGNALMAPPING;
                    if (ExtractorUtilities.findStringInValue(sndrRcvrMapping.SIGNALREF.Value, sysSigName))
                    {
                        dataMapping.Remove(sndrRcvrMappingObj);
                        continue;
                    }
                }
                else
                {
                    var sndrRcvrGrpMapping = sndrRcvrMappingObj as SENDERRECEIVERTOSIGNALGROUPMAPPING;
                    if (ExtractorUtilities.findStringInValue(sndrRcvrGrpMapping.SIGNALGROUPREF.Value, sysSigName))
                    {
                        dataMapping.Remove(sndrRcvrMappingObj);
                        continue;
                    }
                }
            }
            vehiclePrjSystem.MAPPING.DATAMAPPINGS = dataMapping.ToArray();
            vehicleProj.ELEMENTS[0] = vehiclePrjSystem;
            Extract_3_0.editTopNode(vehicleProj);
        }

        internal static void removeComponentSndrRcvrRefs(string applCompShortName)
        {
            ARPACKAGE vehicleProj = Extract_3_0.getARPackage("VehicleProject"); //old code
            var vehiclePrjSystem = vehicleProj.ELEMENTS[0] as SYSTEM;
            var dataMapping = vehiclePrjSystem.MAPPING.DATAMAPPINGS.ToList();
            Common.GlobalDefs.ConsoleUpdater(Common.LOGMSGCLASS.INFO, $"removing {applCompShortName} definition from Data Mapping");
            foreach (var sndrRcvrMappingObj in vehiclePrjSystem.MAPPING.DATAMAPPINGS)
            {
                if (sndrRcvrMappingObj.GetType().Name.Equals("SENDERRECEIVERTOSIGNALMAPPING"))
                {
                    var sndrRcvrMapping = sndrRcvrMappingObj as SENDERRECEIVERTOSIGNALMAPPING;
                    var dataElemRef = sndrRcvrMapping.DATAELEMENTIREF;
                    if(dataElemRef.COMPONENTPROTOTYPEREF != null)
                    {
                        var compProtoRefs = dataElemRef.COMPONENTPROTOTYPEREF;
                        foreach (var compProtoRef in compProtoRefs)
                        {
                            if (ExtractorUtilities.findStringInValue(compProtoRef.Value, applCompShortName))
                            {
                                dataMapping.Remove(sndrRcvrMappingObj);
                                break;
                            }
                        }
                    }
                    if(dataElemRef.PORTPROTOTYPEREF != null)
                    {
                        var portProtoRef = dataElemRef.PORTPROTOTYPEREF;
                        if (ExtractorUtilities.findStringInValue(portProtoRef.Value, applCompShortName))
                        {
                            dataMapping.Remove(sndrRcvrMappingObj);
                            continue;
                        }
                    }
                }
                else
                {
                    var sndrRcvrGrpMapping = sndrRcvrMappingObj as SENDERRECEIVERTOSIGNALGROUPMAPPING;
                    var dataElemRef = sndrRcvrGrpMapping.DATAELEMENTIREF;
                    if (dataElemRef.COMPONENTPROTOTYPEREF != null)
                    {
                        var compProtoRefs = dataElemRef.COMPONENTPROTOTYPEREF;
                        foreach (var compProtoRef in compProtoRefs)
                        {
                            if (ExtractorUtilities.findStringInValue(compProtoRef.Value, applCompShortName))
                            {
                                dataMapping.Remove(sndrRcvrMappingObj);
                                break;
                            }
                        }

                    }
                    if (dataElemRef.PORTPROTOTYPEREF != null)
                    {
                        var portProtoRef = dataElemRef.PORTPROTOTYPEREF;
                        if (ExtractorUtilities.findStringInValue(portProtoRef.Value, applCompShortName))
                        {
                            dataMapping.Remove(sndrRcvrMappingObj);
                            continue;
                        }
                    }
                }
            }
            vehiclePrjSystem.MAPPING.DATAMAPPINGS = dataMapping.ToArray();
            vehicleProj.ELEMENTS[0] = vehiclePrjSystem;
            Extract_3_0.editTopNode(vehicleProj);
            //throw new NotImplementedException();
        }
    }
}
