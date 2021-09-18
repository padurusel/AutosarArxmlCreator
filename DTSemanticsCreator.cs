using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using Common;

namespace EcuExtractCreator
{
    using autosar;
    using Common.SysWItems;
    using Autosar3x;
    public static class DTSemanticsCreator
    {

        static List<string> reUseSigs = new List<string>();
        static List<string> reNamedSigs = new List<string>();
        

        
        public static string createDataTypeSemantics(CompuInternWrapper compuWraper, string signalName) {
              bool createNewDT = false;
              int matchPercent;
             COMPUMETHOD extnCompu = findExistingEnumDTSemantics(compuWraper, out matchPercent );
            if (extnCompu != null) // if no existing datatype matches then create one.
            {
                if (matchPercent == 100)
                    createNewDT = false;
                else
                    if(ExtractEngineProps.RenameDTs)
                        createNewDT = CheckReUseExistingDTSemantics(compuWraper,extnCompu, signalName);
            }
            if (extnCompu == null || createNewDT == true)
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Creating dataType semantics for  {compuWraper.ShortName}");
                autosar.COMPUMETHOD compuMethod = new autosar.COMPUMETHOD();
                compuMethod.COMPUINTERNALTOPHYS = compuWraper.CompuToPhys;
                compuMethod.SHORTNAME = compuWraper.ShortName;
                Extract_3_0.addObjToPkg("DataTypeSemantics", compuMethod);
                return compuWraper.ShortName;
            }
            else
            {
                GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"reusing existing type { extnCompu.SHORTNAME} for {signalName} ");
                GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing type { extnCompu.SHORTNAME} for {signalName} ");
                //reNameDT(extnCompu);
                if (ExtractEngineProps.RenameDTs)
                    return extnCompu.SHORTNAME.Equals(signalName, StringComparison.CurrentCultureIgnoreCase) ? extnCompu.SHORTNAME : reNameDT(extnCompu, signalName);
                else return extnCompu.SHORTNAME;
            }
        }
        
        private static string reNameDT(COMPUMETHOD extnCompu,string replacedCompu)
        {
            if (reNamedSigs.Contains(extnCompu.SHORTNAME) || isGenericDT(extnCompu.SHORTNAME)) // check if datatype has been renamed before. 
                return extnCompu.SHORTNAME;

            string inp = ExtractorUtilities.ManageUserInput($"reusing datatype {extnCompu.SHORTNAME} for {replacedCompu} do you want to rename {extnCompu.SHORTNAME} to a generic name",  Common.GlobalDefs.yesNoQ);
            if (inp.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
            {
                string newDTName = GlobalDefs.getUserInput($"please enter new generic name for {extnCompu.SHORTNAME} (end with _T): ", "change datatype name");
                if (string.IsNullOrEmpty(newDTName) || string.IsNullOrWhiteSpace(newDTName))
                {
                    reNamedSigs.Add(extnCompu.SHORTNAME);
                    GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"DataType name {extnCompu.SHORTNAME} not changed!");
                    return extnCompu.SHORTNAME;
                }
                string newFullDTName = newDTName.Substring((newDTName.Length - 2)).Equals("_T") ? newDTName : $"{newDTName}_T";
                autosar.ARPACKAGE dTPkg = Extract_3_0.getARPackage("DataType");
                reNamedSigs.Add(newFullDTName);
                GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"renamed {extnCompu.SHORTNAME} to {newFullDTName} ");
                foreach (object obj in dTPkg.ELEMENTS)
                {
                    if (obj.GetType().Name == "INTEGERTYPE")
                    {
                        var intDTType = obj as INTEGERTYPE;
                        if (intDTType.SHORTNAME.Equals(extnCompu.SHORTNAME))
                        {
                            intDTType.SHORTNAME = newFullDTName;
                            intDTType.SWDATADEFPROPS.COMPUMETHODREF.Value = $"/DataType/DataTypeSemantics/{newFullDTName}";
                            Extract_3_0.editTopNode(dTPkg);
                            DataTypes_3_0.renameDTSemanticsCompuMethods(extnCompu, newFullDTName);
                            return newFullDTName;
                        }
                    }
                }
                
                //GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "DataType name not changed!");
                //return extnCompu.SHORTNAME;
            }
            reNamedSigs.Add(extnCompu.SHORTNAME);
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, "DataType name not changed!");
            return extnCompu.SHORTNAME;
        }

        public static bool CheckReUseExistingDTSemantics(CompuInternWrapper compuWraper,  COMPUMETHOD extnCompu, string signalName)
        {
            if (reUseSigs.Contains(extnCompu.SHORTNAME)) // if architect already accepted to reuse this DT then just go ahead and reuse it
                return false;

            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Found a close matching dataType for signal {signalName}");
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.INFO, $"Existing data type name: {extnCompu.SHORTNAME}" );
            var sb = new System.Text.StringBuilder();
            autosar.COMPU compuIntToPhys = extnCompu.COMPUINTERNALTOPHYS as autosar.COMPU;
            autosar.COMPUCOMPUSCALES compuScales = compuIntToPhys.Item as autosar.COMPUCOMPUSCALES;
            autosar.COMPU compuIntToPhys2 = compuWraper.CompuToPhys as autosar.COMPU;
            autosar.COMPUCOMPUSCALES compuScales2 = compuIntToPhys2.Item as autosar.COMPUCOMPUSCALES;
            sb.Append(string.Format("\n{0, -50}:{1,-50}:\n\n", extnCompu.SHORTNAME, signalName));

            if (compuScales.Items.Length > 1)
            { // print this only for enum datatypes
                for (int i = 0; i < compuScales.Items.Length; i++)//)
                {
                    object item = compuScales.Items[i];
                    object item2 = compuScales2.Items[i];
                    autosar.COMPUSCALE compuScale = (autosar.COMPUSCALE)item;
                    autosar.COMPUCONST compuConst = compuScale.Item as autosar.COMPUCONST;
                    autosar.VT vt1 = compuConst.Item as autosar.VT;
                    autosar.COMPUSCALE compuScale2 = (autosar.COMPUSCALE)item2;
                    autosar.COMPUCONST compuConst2 = compuScale2.Item as autosar.COMPUCONST;
                    autosar.VT vt2 = compuConst2.Item as autosar.VT;
                    sb.Append(string.Format("{0, -50}{1,-50:N0}\n", vt1.Text[0], vt2.Text[0]));
                }

            }
            else
            {
                object item = compuScales.Items[0];
                object item2 = compuScales2.Items[0];
                var compuScale = (autosar.COMPUSCALE)item;
                var compuRatCoeff = compuScale.Item as autosar.COMPURATIONALCOEFFS;
                var compuScale2 = (autosar.COMPUSCALE)item2;
                var compuRatCoeff2 = compuScale2.Item as autosar.COMPURATIONALCOEFFS;
                var vNumerator = compuRatCoeff.COMPUNUMERATOR.Items[0] as string;
                var vDenominator = compuRatCoeff.COMPUDENOMINATOR.Items[0] as string;
                var vNumerator2 = compuRatCoeff2.COMPUNUMERATOR.Items[0] as string;
                var vDenominator2 = compuRatCoeff2.COMPUDENOMINATOR.Items[0] as string;
                sb.Append(string.Format("{0, -10}{1,-10:N0}\n", vNumerator, vNumerator2));
                sb.Append(string.Format("{0, -10}{1,-10:N0}\n\n", vDenominator, vDenominator2));
            }
            GlobalDefs.ConsoleUpdater(LOGMSGCLASS.GENERAL, sb.ToString());
            string question = "reuse DataType Definition for Signal?";
            string inp = ExtractorUtilities.ManageUserInput(question,  Common.GlobalDefs.yesNoQ);

            if (inp.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
            {
                string question2 = "reuse this DataType definition when found for other Signal without asking?";
                //Console.Write("reuse DataType Definition for Signal?((y)es or (n)o): ");
                string inp2 = ExtractorUtilities.ManageUserInput(question2,  Common.GlobalDefs.yesNoQ);
                GlobalDefs.logMessageToFile(LOGMSGCLASS.INFO, $"reusing existing type { extnCompu.SHORTNAME} for {signalName} ");
                if (inp2.Equals("yes", StringComparison.CurrentCultureIgnoreCase))
                {
                    reUseSigs.Add(extnCompu.SHORTNAME);
                }
                return false;
            }

            else
            {
                return true;
            }
            //throw new NotImplementedException();
        }

        public static autosar.COMPUMETHOD findExistingEnumDTSemantics(CompuInternWrapper newCompuIntToPhys, out int matchPercent)
        {
            //autosar.ARPACKAGE dTPkg = Utilities.getXtractTopNode("DataType", "xtract");
            autosar.ARPACKAGE dTSemantics = Extract_3_0.getARPackage("DataTypeSemantics");//dTPkg.SUBPACKAGES[0];
            autosar.COMPUCOMPUSCALES compuscales = newCompuIntToPhys.CompuToPhys.Item as autosar.COMPUCOMPUSCALES;
            string compuShrtNm = newCompuIntToPhys.ShortName;

            //first check if this is a genericdatatype
            if (isGenericDT(newCompuIntToPhys.ShortName))
            {
                // check if generic datatype semantics already exists in extract.
                var dTObj = Extract_3_0.getObjFromPkg("DataTypeSemantics", newCompuIntToPhys.ShortName);
                if (dTObj == null)
                {
                    // check if datatype semantics already exists in generic DT array.
                    var compuMethod = DataTypes_3_0.getGenericDT(newCompuIntToPhys.ShortName);
                    if(compuMethod == null)
                    {
                        //if it doesnt exist already in generic dt array then create it and add it to both generic dt array and extract
                        var newGenericCompuMthd = new COMPUMETHOD();
                        newGenericCompuMthd.COMPUINTERNALTOPHYS = newCompuIntToPhys.CompuToPhys;
                        newGenericCompuMthd.SHORTNAME = compuShrtNm;
                        DataTypes_3_0.genericDTs.Add(newGenericCompuMthd);
                        Extract_3_0.addObjToPkg("DataTypeSemantics", newGenericCompuMthd);
                        matchPercent = 100;
                        return newGenericCompuMthd;
                    }
                    else
                    {
                        //if it already exists in generic datatype array just copy it to extract
                        Extract_3_0.addObjToPkg("DataTypeSemantics", compuMethod);
                        matchPercent = 100;
                        return compuMethod;
                    }
                           
                }
                else
                {
                    //if it exists in extract then simply return it
                    matchPercent = 100;
                    return dTObj as COMPUMETHOD;
                        
                }
             
            }
            //if it is not a generic datatype then check if its a datatype that exists in extract already.
            var extractDTObj = Extract_3_0.getObjFromPkg("DataTypeSemantics", newCompuIntToPhys.ShortName);
            if(extractDTObj == null){
                //first try to compare with compuconsts of other datatypes
                foreach (object obj in dTSemantics.ELEMENTS)
                {
                    autosar.COMPUMETHOD compuMethod = obj as autosar.COMPUMETHOD;
                    autosar.COMPUCOMPUSCALES dTCompuscales = (autosar.COMPUCOMPUSCALES)compuMethod.COMPUINTERNALTOPHYS.Item;
                    if (dTCompuscales.Items.Length == compuscales.Items.Length && compuscales.Items.Length > 1)
                    { // other check removes scalar values scenario
                        autosar.COMPU compuIntToPhys2 = compuMethod.COMPUINTERNALTOPHYS as autosar.COMPU;
                        if (DataTypes_3_0.compareCompuConst(newCompuIntToPhys.CompuToPhys, compuIntToPhys2, compuShrtNm, compuMethod.SHORTNAME, out matchPercent))
                        {
                            return compuMethod;
                        }
                    }
                }
                //try a close match search in generic datatypes
                //Console.WriteLine("");
                foreach (COMPUMETHOD compuMethod in DataTypes_3_0.genericDTs)
                {
                    autosar.COMPU exstnCompuIntToPhys = compuMethod.COMPUINTERNALTOPHYS as COMPU;
                    autosar.COMPUCOMPUSCALES dTCompuscales = compuMethod.COMPUINTERNALTOPHYS.Item as COMPUCOMPUSCALES;
                    if (dTCompuscales.Items.Length == compuscales.Items.Length && compuscales.Items.Length > 1)
                    { // other check removes scalar values scenario

                        if (DataTypes_3_0.compareGenericDT(newCompuIntToPhys.CompuToPhys, exstnCompuIntToPhys, out matchPercent))
                        {
                            var closeMatchedExtractDTObj = Extract_3_0.getObjFromPkg("DataTypeSemantics", compuMethod.SHORTNAME);
                            if (closeMatchedExtractDTObj == null)
                                Extract_3_0.addObjToPkg("DataTypeSemantics", compuMethod);
                            return compuMethod;
                        }
                            
                    }
                }
            }
            else
            {
                //if it exists already just return it
                matchPercent = 100;
                return extractDTObj as COMPUMETHOD;
            }
            matchPercent = 0;
            return null;
        }

        //private static COMPUMETHOD  
        public static bool isGenericDT(string compuMethodShrtName)
        {
            //string shortName = compuMethod.SHORTNAME;

            if (DataTypes_3_0.genericDTNames != null  && DataTypes_3_0.genericDTNames.Contains(compuMethodShrtName))
                return true;
            return false;
            //throw new NotImplementedException();
        }

        
    }
}
