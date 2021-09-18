using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.SysWItems;
namespace EcuExtractCreator.Autosar3x
{
    using autosar;
    using Common;
    public static class IntegerDataType_3_0
    {
        
        public static string createIntegerTypeDT(string dtSemName, string intDTName, ARLIMIT upperLimit, ARLIMIT lowerLimit)
        {
            autosar.ARPACKAGE dTPkg = Extract_3_0.getARPackage("DataType");
            var intDTDefObj = Extract_3_0.getObjFromPkg("DataType", intDTName);
            if(intDTDefObj != null)
            {
                if(intDTDefObj.GetType().Name == "INTEGERTYPE")
                {
                    var extnIntDT = intDTDefObj as INTEGERTYPE;
                    return extnIntDT.SHORTNAME;
                    /**var exstnDTSemName = extnIntDT.SWDATADEFPROPS.COMPUMETHODREF.Value;
                    if (ExtractorUtilities.findStringInValue(exstnDTSemName, dtSemName))
                    {

                    }
                    else
                    {
                        GlobalDefs.ConsoleUpdater(LOGMSGCLASS.ERROR, $"Datatpye {intDTName} exists already but points to a different Semantics Name");
                        GlobalDefs.logMessageToFile(LOGMSGCLASS.ERROR, $"Datatpye {intDTName} exists already but points to a different Semantics Name");
                        throw new BadReferenceException($"Datatpye {intDTName} exists already but points to a different Semantics Name");
                    }**/
                }
                
            }
            var intDT = new autosar.INTEGERTYPE();
            if (upperLimit != null && lowerLimit != null)
            {
                intDT.UPPERLIMIT = upperLimit;
                intDT.LOWERLIMIT = lowerLimit;
            }
            //var newDTSemName = DTSemanticsCreator.createDataTypeSemantics(compuWrapper, dtSemName);
            intDT.SHORTNAME = intDTName.EndsWith("_T")?intDTName: $"{intDTName}_T";// $"{signalName}_T";
            //intDT.SWDATADEFPROPS
            var swDefProp = new autosar.SWDATADEFPROPS();
            var swDTDefPropCompuRef = new autosar.SWDATADEFPROPSCOMPUMETHODREF();
            swDTDefPropCompuRef.DEST = autosar.COMPUMETHODSUBTYPESENUM.COMPUMETHOD;
            swDTDefPropCompuRef.Value = $"/DataType/DataTypeSemantics/{dtSemName}";
            swDefProp.COMPUMETHODREF = swDTDefPropCompuRef;
            intDT.SWDATADEFPROPS = swDefProp;
            Extract_3_0.addObjToPkg("DataType", intDT);
            return intDT.SHORTNAME;
            //intDT.LOWERLIMIT = (compuWrapper.CompuToPhys.Item as autosar.COMPUCONST).
        }

        
        public static PrimitiveDataType convertCBDSSignal(SysWSignalDefinition cbdsSig)
        {
            var primDT = new PrimitiveDataType(cbdsSig.Itemproperties);
            primDT.PartDefObjName = cbdsSig.PartDefObjName;
            if (cbdsSig.SignalDataType.GetType().Name.Equals("StringDesignDataType"))
            {
                primDT.DTType = ARSIDataTypeDef.STRING;
                Autosar4xDatatypeDef.DataTypeAttribute dtAttr = new Autosar4xDatatypeDef.DataTypeAttribute();
                dtAttr.AtributeSID = "ARSR";

                dtAttr.AtributeValue = ((StringDesignDataType)cbdsSig.SignalDataType).StringDef.Length;
                primDT.dataTypeAttr = new List<Autosar4xDatatypeDef.DataTypeAttribute>();
                primDT.dataTypeAttr.Add(dtAttr);

            }
            else
            {
                primDT.DtCompuMethod = cbdsSig.DtCompuMethod;
                primDT.DTType = ARSIDataTypeDef.PRIMITIVE;

            }
            return primDT;
        }
    }
}
