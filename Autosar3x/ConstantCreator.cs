using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using autosar;
using Common;
namespace EcuExtractCreator.Autosar3x
{
    using Common.SysWItems;
    using Autosar3x;
    using System.Globalization;
    public static class ConstantCreator
    {
       public static bool removeConstantDef(string constName, string compName){

            return false;
       }
        internal static bool findExistingConst(string signalName, string PIDTName, out CONSTANTSPECIFICATION exstConstDef)
        {
            ARPACKAGE constantPkg = Extract_3_0.getARPackage("Constant");
            foreach (object item in constantPkg.ELEMENTS)
            {
                var exstnConstSpec = item as CONSTANTSPECIFICATION;
                string curShortName = $"C_{signalName}_IV";
                if (curShortName.Equals(exstnConstSpec.SHORTNAME))
                {
                    string exstnConstDTName = "";
                    if (exstnConstSpec.VALUE.Item.GetType().Name == "INTEGERLITERAL")
                    {
                        var constIntLit = exstnConstSpec.VALUE.Item as INTEGERLITERAL;
                        exstnConstDTName = constIntLit.TYPETREF.Value.Split('/').Last();
                    }
                    if (exstnConstSpec.VALUE.Item.GetType().Name == "RECORDSPECIFICATION")
                    {
                        var constIntLit = exstnConstSpec.VALUE.Item as RECORDSPECIFICATION;
                        exstnConstDTName = constIntLit.TYPETREF.Value.Split('/').Last();
                    }
                    PIDTName = PIDTName.EndsWith("_T") ? PIDTName : $"{PIDTName}_T";
                    if (exstnConstDTName.Equals(PIDTName))
                    {
                        exstConstDef = exstnConstSpec;
                        return true;
                    }
                    else
                    {
                        exstConstDef = exstnConstSpec;
                        return false;
                    }
                }
            }
            exstConstDef = null;
            return false;
        }

        internal static string updateConstTypeRef(string constDefName, string newTypeRef)
        {
            ARPACKAGE constantPkg = Extract_3_0.getARPackage("Constant");
            foreach (object item in constantPkg.ELEMENTS)
            {
                var exstnConstSpec = item as CONSTANTSPECIFICATION;
               
                if (constDefName.Equals(exstnConstSpec.SHORTNAME))
                {
                    if (exstnConstSpec.VALUE.Item.GetType().Name == "INTEGERLITERAL")
                    {
                        var constIntLit = exstnConstSpec.VALUE.Item as INTEGERLITERAL;
                        constIntLit.TYPETREF.Value = $"/DataType/{newTypeRef}";
                        return newTypeRef;
                        //exstnDTName = constIntLit.TYPETREF.Value.Split('/').Last();
                    }
                    if (exstnConstSpec.VALUE.Item.GetType().Name == "RECORDSPECIFICATION")
                    {
                        var constIntLit = exstnConstSpec.VALUE.Item as RECORDSPECIFICATION;
                        constIntLit.TYPETREF.Value = $"/DataType/{newTypeRef}";
                        return newTypeRef;
                    }
                }
            }
            return null;
        }

        internal static string createIntegerConstantDef(SysWSignalDefinition signal, string DTName, ARSIDataTypeDef dtTypeDef)
        {
            CONSTANTSPECIFICATION constSpec = new CONSTANTSPECIFICATION();
            constSpec.SHORTNAME = $"C_{signal.Itemproperties.ItemName}_IV";
            CONSTANTSPECIFICATIONVALUE constSpecVal = new CONSTANTSPECIFICATIONVALUE();
            string constTypeLiteral;
            if(dtTypeDef == ARSIDataTypeDef.PRIMITIVE)
            {
                INTEGERLITERAL constIntLiteral = new INTEGERLITERAL();
                constIntLiteral.SHORTNAME = constSpec.SHORTNAME;
                INTEGERLITERALTYPETREF constIntLiteralTypeRef = new INTEGERLITERALTYPETREF();
                constIntLiteralTypeRef.DEST = DATATYPESUBTYPESENUM.INTEGERTYPE;
                constIntLiteralTypeRef.Value = $"/DataType/{DTName}";
                if (signal.GetType().Name == "SysWSignalDefinition")
                {
                    constIntLiteral.VALUE = signal.InitVal; //getConstInitVal(signal.DtCompuMethod.CompuToPhys);
                }
                else
                {
                    var arsiSig = signal as SysWSignalDefinitionARSI;
                    PrimitiveDataType dtType = arsiSig.DatatypeDefs[0] as PrimitiveDataType;
                    constIntLiteral.VALUE = signal.InitVal; //getConstInitVal(dtType.DtCompuMethod.CompuToPhys);
                }
                constIntLiteral.TYPETREF = constIntLiteralTypeRef;
                constSpecVal.Item = constIntLiteral;
                constTypeLiteral = constIntLiteral.SHORTNAME;
            }
            else
            {
                var strLiteral = new STRINGLITERAL();
                strLiteral.SHORTNAME = $"{ signal.Itemproperties.ItemName}";
                var strLiteralTypeRef = new STRINGLITERALTYPETREF();
                strLiteralTypeRef.DEST = DATATYPESUBTYPESENUM.STRINGTYPE;
                strLiteralTypeRef.Value = $"/DataType/{DTName}";
                strLiteral.TYPETREF = strLiteralTypeRef;
                constTypeLiteral = strLiteral.SHORTNAME;
                constSpecVal.Item = strLiteral;
            }
            
            constSpec.VALUE = constSpecVal;
            Extract_3_0.addObjToPkg("Constant", constSpec);

            return $"{constSpec.SHORTNAME}/{constTypeLiteral}";
        }

        internal static List<string> createRecordConstDef(RecordDataType recDT, List<string> DTNames, string signalName, string recDTName, out string constDefName)
        {

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"creating constant definition for for signal {recDTName}...");
            CONSTANTSPECIFICATION constSpec = new CONSTANTSPECIFICATION();
            constSpec.SHORTNAME = $"C_{signalName}_IV";
            CONSTANTSPECIFICATIONVALUE constSpecVal = new CONSTANTSPECIFICATIONVALUE();
            var recSpec = new RECORDSPECIFICATION();
            recSpec.SHORTNAME = $"C_{signalName}_IV";
            var recSpecRef = new RECORDSPECIFICATIONTYPETREF();
            recSpecRef.DEST = DATATYPESUBTYPESENUM.RECORDTYPE;
            recSpecRef.Value = $"/DataType/{recDTName}";
            recSpec.TYPETREF = recSpecRef;
            List<object> recConstElems = new List<object>();
            
            List<string> recConstElemNames = new List<string>();

            for (int i = 0; i < recDT.PrimitiveDataTypeCollection.Count; i++)
            {
                var primDT = recDT.PrimitiveDataTypeCollection[i];

                if (primDT.DTType == ARSIDataTypeDef.PRIMITIVE)
                {
                    var intLiteral = new INTEGERLITERAL();
                    intLiteral.SHORTNAME = $"{ primDT.Itemproperties.ItemName}_RE";
                    intLiteral.VALUE = primDT.DtCompuMethod.CompuUpperLimit.Value;
                    var intLitTypeRef = new INTEGERLITERALTYPETREF();
                    intLitTypeRef.DEST = DATATYPESUBTYPESENUM.INTEGERTYPE;
                    intLitTypeRef.Value = $"/DataType/{DTNames[i]}";
                    intLiteral.TYPETREF = intLitTypeRef;
                    recConstElemNames.Add(intLiteral.SHORTNAME);
                    recConstElems.Add(intLiteral);
                }
                else
                {
                    var strLiteral = new STRINGLITERAL();
                    strLiteral.SHORTNAME = $"{ primDT.Itemproperties.ItemName}_RE";
                    var strLiteralTypeRef = new STRINGLITERALTYPETREF();
                    strLiteralTypeRef.DEST = DATATYPESUBTYPESENUM.STRINGTYPE;
                    strLiteralTypeRef.Value = $"/DataType/{DTNames[i]}";
                    strLiteral.TYPETREF = strLiteralTypeRef;
                    recConstElemNames.Add(strLiteral.SHORTNAME);
                    recConstElems.Add(strLiteral);
                }
            }
            recSpec.ELEMENTS = recConstElems.ToArray();
            constSpecVal.Item = recSpec;
            constSpec.VALUE = constSpecVal;
            ARPACKAGE ConstantPkg = Extract_3_0.getARPackage("Constant");
            List<object> ConstantPkgElements = ConstantPkg.ELEMENTS.ToList();
            ConstantPkgElements.Add(constSpec);
            ConstantPkg.ELEMENTS = ConstantPkgElements.ToArray();
            Extract_3_0.editTopNode(ConstantPkg);
            constDefName = $"{constSpec.SHORTNAME}/{recSpec.SHORTNAME}";
            return recConstElemNames;

            //throw new NotImplementedException();
        }
    }
}
