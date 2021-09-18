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
    public static class ComponentHelper
    {
        //public CompositeComponent_3_0(autosar.COMPOSITIONTYPE wrappedCompositeType) { }
        public static void addToTopLvl(SysWLCCDefinition lccItem)
        {
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Adding component {lccItem.Itemproperties.ItemName} to TopLevelComposition...");
            var compProtoType = new COMPONENTPROTOTYPE();
            compProtoType.SHORTNAME = lccItem.Itemproperties.ItemName;
            compProtoType.TYPETREF = new COMPONENTPROTOTYPETYPETREF();
            compProtoType.TYPETREF.DEST = COMPONENTTYPESUBTYPESENUM.COMPOSITIONTYPE;
            compProtoType.TYPETREF.Value = $"/ComponentType/{lccItem.Itemproperties.ItemName}";

            var assemblyConsCollect = new List<ASSEMBLYCONNECTORPROTOTYPE>();
            foreach (SysWSignalDefinition signal in lccItem.SignalParts)
            {
                List<string> compSigMaps = AppSWComponentType.getCompSigCons(signal.Itemproperties.ItemName, lccItem.Itemproperties.ItemName, signal.PortDir);
                   
                if (compSigMaps.Count > 0)
                {
                    var assemblyConnector = createAssemblyCon(lccItem.Itemproperties.ItemName, signal.Itemproperties.ItemName, signal.PortDir, compSigMaps);
                    assemblyConsCollect.AddRange(assemblyConnector);
                }
            }
            addToTopLvlComp(compProtoType, assemblyConsCollect.ToArray());
        }


        public static void addToTopLvlComp(autosar.COMPONENTPROTOTYPE compProtoType, autosar.ASSEMBLYCONNECTORPROTOTYPE[] assemblyCons)
        {
            var compType = GetCOMPOSITIONTYPE("TopLevelComposition");

            if (compType != null)
            {
                var compTypeComps = compType.COMPONENTS.ToList();
                compTypeComps.Add(compProtoType);
                compType.COMPONENTS = compTypeComps.ToArray();
                var compTypeCons = compType.CONNECTORS.ToList();
                compTypeCons.AddRange(assemblyCons);
                compType.CONNECTORS = compTypeCons.ToArray();
                //addDefToNode(compType, "ComponentType");
                Extract_3_0.editComponentTypeNode(compType, "TopLevelComposition");
                //sortComponentType();
            }
        }
        public static void removeComponentFromExtract(string compSWCompName, ExtractInfo extractInfo)
        {
            var compObj = Extract_3_0.getObjTypeFromPkg("ComponentType", "COMPOSITIONTYPE", compSWCompName);
            if (compObj == null)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"no COMPOSITIONTYPE type {compSWCompName} found in extract");
                GlobalDefs.StatusUpdater(STATUSCODE.ERROR, $"error");
                return;
            }
                
            var compSWComp = compObj as COMPOSITIONTYPE;
            if (compSWComp == null)
            { // if casting failed
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"failed to cast {compSWCompName} to COMPOSITIONTYPE");
                GlobalDefs.StatusUpdater(STATUSCODE.ERROR, $"error");
                return;
            }
            foreach (object appSWCompObj in compSWComp.COMPONENTS)
            {
                string objShortName = appSWCompObj.GetType().GetProperty("SHORTNAME").GetValue(appSWCompObj).ToString();
                AppSWComponentType.removeComponentFromExtract(objShortName, extractInfo);
            }
            PortProtoTypeCreator.deleteReferencedPIs(compSWComp.PORTS, compSWCompName, extractInfo);

            //TODO remove from toplevel definition
            DataMapping.removeSWCImplMaps(compSWCompName);
            DataMapping.removeSWCToECUMaps(compSWCompName);
            Extract_3_0.removeComponentFromToplvl(compSWCompName);
            Extract_3_0.removeObjFromPkg("ComponentType", compSWCompName);
        }

        internal static ASSEMBLYCONNECTORPROTOTYPE[] createAssemblyCon(string currentComponent, string signalItemName, string portDir, List<string> curComponents)
        {
            var assemblyCons = new List<ASSEMBLYCONNECTORPROTOTYPE>();//[compSigMaps.Count];
            //assemblyCon.
            foreach (string compSigMap in curComponents)
            {
                var assemblyCon = new ASSEMBLYCONNECTORPROTOTYPE();
                assemblyCon.SHORTNAME = portDir == "SendPort" ? $"{currentComponent}_{signalItemName}_{compSigMap}_{signalItemName}" : $"{compSigMap}_{signalItemName}_{currentComponent}_{signalItemName}";
                assemblyCon.PROVIDERIREF = new ASSEMBLYCONNECTORPROTOTYPEPROVIDERIREF();

                assemblyCon.PROVIDERIREF.COMPONENTPROTOTYPEREF = new ASSEMBLYCONNECTORPROTOTYPEPROVIDERIREFCOMPONENTPROTOTYPEREF();
                assemblyCon.PROVIDERIREF.COMPONENTPROTOTYPEREF.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                assemblyCon.PROVIDERIREF.COMPONENTPROTOTYPEREF.Value = portDir == "SendPort" ? $"/ComponentType/TopLevelComposition/{currentComponent}" : $"/ComponentType/TopLevelComposition/{compSigMap}";

                assemblyCon.PROVIDERIREF.PPORTPROTOTYPEREF = new ASSEMBLYCONNECTORPROTOTYPEPROVIDERIREFPPORTPROTOTYPEREF();
                assemblyCon.PROVIDERIREF.PPORTPROTOTYPEREF.DEST = PPORTPROTOTYPESUBTYPESENUM.PPORTPROTOTYPE;
                assemblyCon.PROVIDERIREF.PPORTPROTOTYPEREF.Value = portDir == "SendPort" ? $"/ComponentType/{currentComponent}/{signalItemName}" : $"/ComponentType/{compSigMap}/{signalItemName}";

                assemblyCon.REQUESTERIREF = new ASSEMBLYCONNECTORPROTOTYPEREQUESTERIREF();

                assemblyCon.REQUESTERIREF.COMPONENTPROTOTYPEREF = new ASSEMBLYCONNECTORPROTOTYPEREQUESTERIREFCOMPONENTPROTOTYPEREF();
                assemblyCon.REQUESTERIREF.COMPONENTPROTOTYPEREF.DEST = COMPONENTPROTOTYPESUBTYPESENUM.COMPONENTPROTOTYPE;
                assemblyCon.REQUESTERIREF.COMPONENTPROTOTYPEREF.Value = portDir == "SendPort" ? $"/ComponentType/TopLevelComposition/{compSigMap}" : $"/ComponentType/TopLevelComposition/{currentComponent}";

                assemblyCon.REQUESTERIREF.RPORTPROTOTYPEREF = new ASSEMBLYCONNECTORPROTOTYPEREQUESTERIREFRPORTPROTOTYPEREF();
                assemblyCon.REQUESTERIREF.RPORTPROTOTYPEREF.DEST = RPORTPROTOTYPESUBTYPESENUM.RPORTPROTOTYPE;
                assemblyCon.REQUESTERIREF.RPORTPROTOTYPEREF.Value = portDir == "SendPort" ? $"/ComponentType/{compSigMap}/{signalItemName}" : $"/ComponentType/{currentComponent}/{signalItemName}";

                assemblyCons.Add(assemblyCon);

            }
            return assemblyCons.ToArray();
            //throw new NotImplementedException();
        }

        public static COMPOSITIONTYPE GetCOMPOSITIONTYPE(string compositionTypeName)
        {
            ARPACKAGE compType = Extract_3_0.getARPackage("ComponentType");

            foreach (object obj in compType.ELEMENTS)
            {
                if (obj.GetType().Name == "COMPOSITIONTYPE")
                {
                    var swComp = obj as autosar.COMPOSITIONTYPE;
                    if (swComp.SHORTNAME == compositionTypeName)
                    {
                        return swComp;
                    }
                }
            }
            return null;
        }

        public static bool validateComponentRefs()
        {
            return false;
        }
    }
}
