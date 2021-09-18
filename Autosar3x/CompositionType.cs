using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.SysWItems;
using ParseEngine.DAO;
using System.Xml.Linq;
using autosar;
using EcuExtractCreator.Autosar3x;
using Common;

namespace EcuExtractCreator.Autosar3x
{
    public static class CompositionType
    {
        public static COMPOSITIONTYPE createCompositionType(SysWItemDefinition aSysWItemDef)
        {
            var swComp = Extract_3_0.getObjFromPkg("ComponentType", aSysWItemDef.Itemproperties.ItemName);
            if (swComp != null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"{aSysWItemDef.Itemproperties.ItemName} already EXISTS as a component in extract");
                return null;
            }
            IEnumerable<SysWItemDefinition> allParts = aSysWItemDef.getAllParts();
            List<SysWSignalDefinition> signals = new List<SysWSignalDefinition>();
            string compName = aSysWItemDef.Itemproperties.ItemName;
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"creating COMPOSITIONTYPE for {compName}....");

            var compositionItem = new COMPOSITIONTYPE();
            List<COMPONENTPROTOTYPE> compProtoTypes = new List<COMPONENTPROTOTYPE>();
            foreach (SysWItemDefinition sysItemDef in allParts)
            {
                switch (sysItemDef.GetType().Name)
                {
                    case "SysWLDCDefinition":
                        var ldcItem = sysItemDef as SysWLDCDefinition;
                        COMPONENTPROTOTYPE ldcCompProtoType = new COMPONENTPROTOTYPE();
                        ldcCompProtoType.TYPETREF = new COMPONENTPROTOTYPETYPETREF();
                        ldcCompProtoType.TYPETREF.DEST = COMPONENTTYPESUBTYPESENUM.APPLICATIONSOFTWARECOMPONENTTYPE;
                        var appSWComp = AppSWComponentType.createAppSWType(ldcItem, false, false);
                        /**
                         * if addLDCToExtract returned null due to applicationswcomponent exists in extract already, 
                         * then create the component reference with the ldccomponent itemproperties.
                         */
                        if (appSWComp != null)
                        {
                            appSWComp.PORTS = createLCCPortsDef(ldcItem.SignalParts, compName, ldcItem.Itemproperties.ItemName);
                            //ExtractorUtilities.addDefToNode(appSWComp, "ComponentType");
                            Extract_3_0.addObjToPkg("ComponentType", appSWComp);
                            ldcCompProtoType.SHORTNAME = appSWComp.SHORTNAME;
                            ldcCompProtoType.TYPETREF.Value = $"/ComponentType/{appSWComp.SHORTNAME}";
                        }
                        else
                        {
                            var inp = ExtractorUtilities.ManageUserInput($"do you want to update {ldcItem.Itemproperties.ItemName}?: ", Common.GlobalDefs.yesNoQ);
                            if (inp.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
                            {
                                Autosar3x.Extract_3_0.removeObjFromPkg("ComponentType", ldcItem.Itemproperties.ItemName);
                                appSWComp = AppSWComponentType.createAppSWType(ldcItem, false, false);
                                appSWComp.PORTS = createLCCPortsDef(ldcItem.SignalParts, compName, ldcItem.Itemproperties.ItemName);
                                Extract_3_0.addObjToPkg("ComponentType", appSWComp);
                                ldcCompProtoType.SHORTNAME = appSWComp.SHORTNAME;
                                ldcCompProtoType.TYPETREF.Value = $"/ComponentType/{appSWComp.SHORTNAME}";

                            }
                        }
                        compProtoTypes.Add(ldcCompProtoType);
                        break;
                    case "SysWLCCDefinition":
                        var lccSysWItem = sysItemDef as SysWLCCDefinition;
                        var lccCompType = createCompositionType(lccSysWItem);
                        Extract_3_0.addObjToPkg("ComponentType", lccCompType);
                        COMPONENTPROTOTYPE lccCompProtoType = new COMPONENTPROTOTYPE();
                        lccCompProtoType.SHORTNAME = lccCompType.SHORTNAME;
                        lccCompProtoType.TYPETREF = new COMPONENTPROTOTYPETYPETREF();
                        lccCompProtoType.TYPETREF.DEST = COMPONENTTYPESUBTYPESENUM.APPLICATIONSOFTWARECOMPONENTTYPE;
                        lccCompProtoType.TYPETREF.Value = $"/ComponentType/{lccCompType.SHORTNAME}";
                        compProtoTypes.Add(lccCompProtoType);
                        compositionItem.PORTS = createComponentPorts(sysItemDef);
                        compositionItem.CONNECTORS = createCompositeConectors(sysItemDef);
                        compositionItem.COMPONENTS = compProtoTypes.ToArray();
                        compositionItem.SHORTNAME = compName;

                        break;
                    case "SysWSignalDefinition":
                    case "SysWSignalDefinitionARSI":
                        signals.Add(sysItemDef as SysWSignalDefinition);
                        break;
                }
            }
            compositionItem.PORTS = createComponentPorts(aSysWItemDef);
            compositionItem.CONNECTORS = createCompositeConectors(aSysWItemDef);
            compositionItem.COMPONENTS = compProtoTypes.ToArray();
            compositionItem.SHORTNAME = compName;
            return compositionItem;
        }

        public static object[] createComponentPorts(SysWItemDefinition component)
        {
            List<SysWSignalDefinition> signals;
          
            var lccComp = component as SysWLCCDefinition;
            signals = lccComp.SignalParts;
            List<object> portObjs = new List<object>();
            foreach (SysWSignalDefinition signal in signals)
            {
                var portObj = PortProtoTypeCreator.CreateComponentPortDef(signal);
                if (portObj != null)
                    portObjs.Add(portObj);
            }
            return portObjs.ToArray();
            
        }

        private static object[] createLCCPortsDef(List<SysWSignalDefinition> signals, string LCCName, string LDCName)
        {
            List<object> portsNode = new List<object>();
            object portDefObj;
            foreach (SysWSignalDefinition signal in signals)
            {
                portDefObj = PortProtoTypeCreator.CreateComponentPortDef(signal);

                if (portDefObj != null)
                {
                    if (signal.getSignalType() == "RecordType")
                    {
                        var recElemNames = PortElementsCreator.recElemsName;
                        Autosar3x.DataMapping.crtCompositeSndrRcvrToSigGrpMapg(LCCName, LDCName, signal.Itemproperties.ItemName, signal.PortDir, recElemNames);
                    }
                    else
                    {
                        Autosar3x.DataMapping.crtCompositeSndrRcvrToSigMapg(LCCName, LDCName, signal.Itemproperties.ItemName, signal.PortDir);
                    }
                    portsNode.Add(portDefObj);
                }
            }

            return portsNode.ToArray();
        }

        private static DELEGATIONCONNECTORPROTOTYPE[] createCompositeConectors(SysWItemDefinition compositeItem)
        {
            List<DELEGATIONCONNECTORPROTOTYPE> delegateCons = new List<DELEGATIONCONNECTORPROTOTYPE>();
            var sigComparer = Common.Utilities.EqualityCompareFactory.Create<SysWSignalDefinition>(sig1 => sig1.Itemproperties.ItemName.GetHashCode(),
                                                                                                   (sig1, sig2) => sig1.Itemproperties.ItemName.Equals(sig2.Itemproperties.ItemName));
            if (compositeItem.GetType().Name == "SysWLCCDefinition")
            {
                var lccComp = compositeItem as SysWLCCDefinition;
                lccComp.LCParts.ForEach(ldcPart => {
                    ldcPart.SignalParts.ForEach(sigPart => {
                        if (lccComp.SignalParts.Contains(sigPart, sigComparer)) // check if LDC signals are contained in 
                        {

                            var delegateCon = createDelegateConnectorDef(sigPart, lccComp.Itemproperties.ItemName, ldcPart.Itemproperties.ItemName);
                            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"created connector port for {sigPart.Itemproperties.ItemName}");
                            delegateCons.Add(delegateCon);
                        }
                    });
                });
            }
            else if (compositeItem.GetType().Name == "SysWWrapperItemDefinition")
            {
                var wrapperComp = compositeItem as SysWWrapperItemDefinition;
                wrapperComp.LCParts.ForEach(ldcPart => {

                    ldcPart.SignalParts.ForEach(sigPart => {
                        if (wrapperComp.SignalParts.Contains(sigPart, sigComparer)) // check if LDC signals are contained in 
                        {
                            var delegateCon = createDelegateConnectorDef(sigPart, wrapperComp.Itemproperties.ItemName, ldcPart.Itemproperties.ItemName);
                            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"created connector port for {sigPart.Itemproperties.ItemName}");
                            delegateCons.Add(delegateCon);
                        }
                    });
                });
                /**wrapperComp.LCCParts.ForEach(lccPart => {
                    lccPart.SignalParts.ForEach(sigPart => {
                        if (wrapperComp.SignalParts.Contains(sigPart, sigComparer)) // check if LDC signals are contained in 
                        {
                            var delegateCon = createDelegateConnectorDef(sigPart, wrapperComp.Itemproperties.ItemName, lccPart.Itemproperties.ItemName);
                            delegateCons.Add(delegateCon);
                        }
                    });
                });**/
            }
            return delegateCons.ToArray();
        }
        private static DELEGATIONCONNECTORPROTOTYPE createDelegateConnectorDef(SysWSignalDefinition sigPart, string topLvlCompName, string subCompName)
        {
            var delegateCon = new DELEGATIONCONNECTORPROTOTYPE();
            delegateCon.SHORTNAME = $"{sigPart.Itemproperties.ItemName}_{subCompName}_{sigPart.Itemproperties.ItemName}";
            var innerPortRef = new DELEGATIONCONNECTORPROTOTYPEINNERPORTIREF();
            var compProtRef = new DELEGATIONCONNECTORPROTOTYPEINNERPORTIREFCOMPONENTPROTOTYPEREF();
            compProtRef.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
            compProtRef.Value = $"/ComponentType/{topLvlCompName}/{subCompName}";
            var portProtoRef = new DELEGATIONCONNECTORPROTOTYPEINNERPORTIREFPORTPROTOTYPEREF();
            portProtoRef.DEST = sigPart.PortDir.Equals("SendPort") ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
            portProtoRef.Value = $"/ComponentType/{subCompName}/{sigPart.Itemproperties.ItemName}";
            innerPortRef.COMPONENTPROTOTYPEREF = compProtRef;
            innerPortRef.PORTPROTOTYPEREF = portProtoRef;
            var outerPortRef = new DELEGATIONCONNECTORPROTOTYPEOUTERPORTREF();
            outerPortRef.DEST = sigPart.PortDir.Equals("SendPort") ? PORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE : PORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
            outerPortRef.Value = $"/Component/{topLvlCompName}/{sigPart.Itemproperties.ItemName}";
            delegateCon.INNERPORTIREF = innerPortRef;
            delegateCon.OUTERPORTREF = outerPortRef;
            return delegateCon;
        }
    }
}
