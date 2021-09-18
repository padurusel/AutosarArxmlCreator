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
    public static class AppSWComponentType
    {
        private static string topLevelCompName;

        public static APPLICATIONSOFTWARECOMPONENTTYPE createAppSWType(SysWLDCDefinition ldcItem, bool add2TopLvl, bool crtPrts)
        {
            var appSW = Extract_3_0.getObjFromPkg("ComponentType", ldcItem.Itemproperties.ItemName);
            if (appSW != null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.WARNING, $"{ldcItem.Itemproperties.ItemName} already EXISTS as a component in extract");
                return null;
            }

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"creating APPLICATIONSOFTWARETYPE for {ldcItem.Itemproperties.ItemName}....");
            autosar.APPLICATIONSOFTWARECOMPONENTTYPE appSWComp = new autosar.APPLICATIONSOFTWARECOMPONENTTYPE();
            appSWComp.SHORTNAME = ldcItem.Itemproperties.ItemName;
            createSWCImplementation(ldcItem);
            if (crtPrts) {appSWComp.PORTS = createComponentPorts(ldcItem); }
            if (add2TopLvl) { addToTopLvl(ldcItem); }
            return appSWComp;
        }
        private static string createInternBehavior(SysWLDCDefinition ldcItem)
        {
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "creating internal behaviour of component...");
            var internBehavior = new INTERNALBEHAVIOR();
            internBehavior.SHORTNAME = $"{ldcItem.Itemproperties.ItemName}_InternalBehavior";
            var internBehaviorCompRef = new INTERNALBEHAVIORCOMPONENTREF();
            internBehaviorCompRef.DEST = ATOMICSOFTWARECOMPONENTTYPESUBTYPESENUM.APPLICATIONSOFTWARECOMPONENTTYPE;
            internBehaviorCompRef.Value = $"/ComponentType/{ldcItem.Itemproperties.ItemName}";
            internBehavior.PORTAPIOPTIONS = createPortApis(ldcItem);
            internBehavior.COMPONENTREF = internBehaviorCompRef;
            Extract_3_0.addObjToPkg( "ComponentType", internBehavior);
            return internBehavior.SHORTNAME;
            //throw new NotImplementedException();
        }

        public static string createSWCImplementation(SysWLDCDefinition ldcItem)
        {
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "creating swc implement of component...");
            var swcImplement = new SWCIMPLEMENTATION();
            swcImplement.SHORTNAME = $"{ldcItem.Itemproperties.ItemName}_Implementation";
            swcImplement.CODEDESCRIPTORS = new CODE[1];
            var swcCode = new CODE();
            swcCode.SHORTNAME = "Code";
            swcCode.TYPE = CODETYPEENUM.SRC;
            swcImplement.CODEDESCRIPTORS[0] = swcCode;
            string internBehavior = createInternBehavior(ldcItem);
            swcImplement.BEHAVIORREF = new SWCIMPLEMENTATIONBEHAVIORREF();
            swcImplement.BEHAVIORREF.DEST = INTERNALBEHAVIORSUBTYPESENUM.INTERNALBEHAVIOR;
            swcImplement.BEHAVIORREF.Value = $"/ComponentType/{internBehavior}";
            //ExtractorUtilities.addDefToNode(swcImplement, "ComponentType");
            Extract_3_0.addObjToPkg("ComponentType", swcImplement);
            return swcImplement.SHORTNAME;

        }
        private static PORTAPIOPTION[] createPortApis(SysWLDCDefinition ldcItem)
        {
            List<PORTAPIOPTION> portApis = new List<PORTAPIOPTION>();
            foreach (SysWSignalDefinition signal in ldcItem.SignalParts) {
                var portApi = new PORTAPIOPTION();
                portApi.ENABLETAKEADDRESS = false;
                portApi.INDIRECTAPI = false;
                portApi.ENABLETAKEADDRESSSpecified = true;
                portApi.INDIRECTAPISpecified = true;
                var portApiRef = new PORTAPIOPTIONPORTREF();
                portApiRef.DEST = signal.PortDir == "SendPort" ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                portApiRef.Value = $"/Component/{ldcItem.Itemproperties.ItemName}/{signal.Itemproperties.ItemName}";
                portApi.PORTREF = portApiRef;
                portApis.Add(portApi);
            }
            return portApis.ToArray();
        }

        public static void addToTopLvl(SysWLDCDefinition ldcItem)
        {
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Adding component {ldcItem.Itemproperties.ItemName} to TopLevelComposition...");
            var compProtoType = new COMPONENTPROTOTYPE();
            compProtoType.SHORTNAME = ldcItem.Itemproperties.ItemName;
            compProtoType.TYPETREF = new COMPONENTPROTOTYPETYPETREF();
            compProtoType.TYPETREF.DEST = COMPONENTTYPESUBTYPESENUM.APPLICATIONSOFTWARECOMPONENTTYPE;
            compProtoType.TYPETREF.Value = $"/ComponentType/{ldcItem.Itemproperties.ItemName}";

            var assemblyConsCollect = new List<ASSEMBLYCONNECTORPROTOTYPE>(); 
            foreach(SysWSignalDefinition signal in ldcItem.SignalParts)
            {
                List<string> compSigMaps = getCompSigCons(signal.Itemproperties.ItemName, ldcItem.Itemproperties.ItemName, signal.PortDir);
                if(compSigMaps.Count > 0)
                {
                    var assemblyConnector = ComponentHelper.createAssemblyCon(ldcItem.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, compSigMaps);
                    assemblyConsCollect.AddRange(assemblyConnector);
                }
            }
            ComponentHelper.addToTopLvlComp(compProtoType, assemblyConsCollect.ToArray());
            //CompositeComponent_3_0.sortComponentType();
        }


        private static object[] createComponentPorts(SysWItemDefinition component)
        {
            var ldcComp = component as SysWLDCDefinition;
            List<object> portsNode = new List<object>();
            object portDefObj;
            foreach (SysWSignalDefinition signal in ldcComp.SignalParts)
            {
                var signalType = signal.getSignalType();
                portDefObj = PortProtoTypeCreator.CreateComponentPortDef(signal);
                //createSignalDataType( signal);
                if (portDefObj != null)
                {
                    if (signal.getSignalType() == "RecordType")
                    {
                        var recElemNames = PortElementsCreator.recElemsName;
                        Autosar3x.DataMapping.crtAppSWSndrRcvrToSigGrpMapg(ldcComp.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, recElemNames);
                    }
                    else
                    {
                        Autosar3x.DataMapping.crtAppSWSndrRcvrToSigMapg(ldcComp.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir);
                    }
                    portsNode.Add(portDefObj);
                }
            }

            return portsNode.ToArray();
        }
        internal static List<string> getCompSigCons(string signalName, string ldcItemName, string portDir)
        {
            List<object> compSignals = new List<object>();
            List<string> compSigMaps = new List<string>();
            string compName = "";
            int pportCount = 0;
            if (portDir == "SendPort")
                pportCount = 1; // init pportcount to 1 to account for already existing pport from signal
            var topLvlComponent = ComponentHelper.GetCOMPOSITIONTYPE("TopLevelComposition");
            foreach(var compProto in topLvlComponent.COMPONENTS)
            {
                var compProtoName = compProto.SHORTNAME;
                if(!compProto.SHORTNAME.Equals(ldcItemName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (compProto.TYPETREF.DEST == COMPONENTTYPESUBTYPESENUM.APPLICATIONSOFTWARECOMPONENTTYPE)
                    {
                        var appsw = GetAPPSWTYPE(compProto.SHORTNAME);
                        if(appsw != null)
                        {
                            if(appsw.PORTS != null)
                            {
                                compSignals = appsw.PORTS.ToList();
                                compName = appsw.SHORTNAME;
                            }
                            
                        }
                        
                    }
                    else
                    {
                        var compsiteType = ComponentHelper.GetCOMPOSITIONTYPE(compProto.SHORTNAME);
                        if(compsiteType != null)
                        {
                            if (compsiteType.PORTS != null)
                            {
                                compSignals = compsiteType.PORTS.ToList();
                                compName = compsiteType.SHORTNAME;
                            }
                            
                        }
                       
                    }
                }
                else
                {
                    return null;
                }
                
                if (compSignals.Count > 0)
                {
                    foreach (object sigObj in compSignals)
                    {
                        //CompSigMap_Struct compSigMap = new CompSigMap_Struct();
                        if (sigObj.GetType().Name == "PPORTPROTOTYPE")
                        {
                            var portType = sigObj as PPORTPROTOTYPE;
                            if (portType.SHORTNAME.Equals(signalName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                pportCount += 1;
                                if(pportCount > 1)
                                {
                                    GlobalDefs.ConsoleUpdater(LOGMSGCLASS.WARNING, $"found more than one provide port for signal{signalName} in {compName}, not adding to TopLevelComposition");
                                    GlobalDefs.logMessageToFile(LOGMSGCLASS.WARNING, $"signal{signalName} in component {compName} is defined as a provide signal else where, skipping");
                                    break;
                                }
                                else
                                {
                                    compSigMaps.Add(compName);
                                }
                            }
                        }
                        else
                        {
                            if(portDir == "SendPort")
                            {
                                var portType = sigObj as RPORTPROTOTYPE;
                                if (portType.SHORTNAME.Equals(signalName, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    compSigMaps.Add(compName);
                                }
                            }
                            
                        }
                    }
                    compSignals.Clear();
                }
            }
            return compSigMaps;
        }

        //private 
        public static autosar.APPLICATIONSOFTWARECOMPONENTTYPE GetAPPSWTYPE(string appSWTypeName)
        {
            autosar.ARPACKAGE compType =Extract_3_0.getARPackage("ComponentType");

            foreach (object obj in compType.ELEMENTS)
            {
                if (obj.GetType().Name == "APPLICATIONSOFTWARECOMPONENTTYPE")
                {
                    var swComp = obj as autosar.APPLICATIONSOFTWARECOMPONENTTYPE;
                    if (swComp.SHORTNAME == appSWTypeName)
                    {
                        return swComp;
                    }
                }
            }
            return null;
        }

        public static void removeComponentFromExtract(string appSWCompName, ExtractInfo extractInfo)
        {
            var compObj = Extract_3_0.getObjTypeFromPkg("ComponentType", "APPLICATIONSOFTWARECOMPONENTTYPE", appSWCompName);
            if (compObj == null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"cannot find any definition for {appSWCompName} in extract");
                return;
            }
                
            APPLICATIONSOFTWARECOMPONENTTYPE appSWComp = compObj as APPLICATIONSOFTWARECOMPONENTTYPE;
            if (appSWComp == null)
            { // if casting failed
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"failed to cast {appSWComp.SHORTNAME} to applicationsoftwarecomponent type... ");
                return;
            }
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"deleting {appSWComp.SHORTNAME} PortInterfaces... ");
            PortProtoTypeCreator.deleteReferencedPIs(appSWComp.PORTS, appSWCompName, extractInfo);
            
            
            //TODO remove from toplevel definition
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"cleaning up {appSWComp.SHORTNAME} other definitions... ");
            DataMapping.removeComponentSndrRcvrRefs(appSWComp.SHORTNAME);
            DataMapping.removeSWCImplMaps(appSWComp.SHORTNAME);
            DataMapping.removeSWCToECUMaps(appSWComp.SHORTNAME);
            
            Extract_3_0.removeObjFromPkg("ComponentType", $"{appSWCompName}_Implementation");
            Extract_3_0.removeObjFromPkg("ComponentType", $"{appSWCompName}_InternalBehavior");
            Extract_3_0.removeComponentFromToplvl(appSWCompName);
            Extract_3_0.removeObjFromPkg("ComponentType", appSWCompName);
        }

        public static bool checkForLDCUpdate(SysWLDCDefinition ldcDef, APPLICATIONSOFTWARECOMPONENTTYPE appSWComp)
        {
            if(ldcDef.SignalParts.Count != appSWComp.PORTS.Length)
                return true;
            foreach(SysWSignalDefinition signals in ldcDef.SignalParts)
            {
                //object 
                foreach (object obj in appSWComp.PORTS)
                {
                    string portInterfaceName;
                    string objShortName = obj.GetType().GetProperty("SHORTNAME").GetValue(obj).ToString();
                    if(signals.Itemproperties.ItemName.Equals(objShortName, StringComparison.CurrentCultureIgnoreCase))
                    {
                        if (obj.GetType().Name == "PPORTPROTOTYPE")
                        {
                            var portType = obj as PPORTPROTOTYPE;
                            portInterfaceName = portType.PROVIDEDINTERFACETREF.Value;
                        }
                        else {
                            var portType = obj as RPORTPROTOTYPE;
                            portInterfaceName = portType.REQUIREDINTERFACETREF.Value;
                        }
                        object portIntObj = Extract_3_0.getObjFromPkg("PortInterface", portInterfaceName);
                        SENDERRECEIVERINTERFACE sndrRcvrInt = portIntObj as SENDERRECEIVERINTERFACE;
                        var dtName = sndrRcvrInt.DATAELEMENTS[0].TYPETREF.Value;
                        object dtIntObj = Extract_3_0.getObjFromPkg("DataType", dtName);
                        if (dtIntObj.GetType().Name == "INTEGERTYPE")
                        {
                            var intDTType = dtIntObj as INTEGERTYPE;
                            var dtSemanticsName = intDTType.SWDATADEFPROPS.COMPUMETHODREF.Value;
                            object dtSemDef = Extract_3_0.getObjFromPkg("DataTypeSemantics", dtSemanticsName);

                        }
                        else
                        {
                            var recDTType = dtIntObj as RECORDTYPE;
                            foreach(RECORDELEMENT recElems in recDTType.ELEMENTS)
                            {
                                var dtSemanticsName = recElems.SWDATADEFPROPS.COMPUMETHODREF.Value;
                                object dtSemDef = Extract_3_0.getObjFromPkg("DataTypeSemantics", dtSemanticsName);
                            }
                        }
                    }

                }
            }
            
            return false;
        }

        public static SysWLCCDefinition convertToComposite(SysWLDCDefinition ldcItem)
        {
            var ldcToLcc = new SysWLCCDefinition(ldcItem.Itemproperties);
            //ldcToLcc.LCParts.Add(ldcItem);
            ldcToLcc.SignalParts.AddRange(ldcItem.SignalParts);
            return ldcToLcc;
        }
        private static void compareCompuSemantics(COMPUMETHOD compuMethod1, COMPUMETHOD compuMethod2) { }
        struct CompSigMap_Struct {
            public string portDir { get; set; }
            public string componentName { get; set; }
        }
    }
}
