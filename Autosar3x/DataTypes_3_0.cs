using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.SysWItems;
using autosar;
namespace EcuExtractCreator.Autosar3x
{
    public static class DataTypes_3_0
    {
        public static List<string> genericDTNames { get { return ExtractorUtilities.GenericDTNames; } }
        public static List<COMPUMETHOD> genericDTs { get { return ExtractorUtilities.genericDTs; } }
        //public void init

        public static bool checkGenericCompus(string compuName)
        {
            foreach (var genericCompu in genericDTs)
                if (genericCompu.SHORTNAME.Equals(compuName, StringComparison.CurrentCultureIgnoreCase))
                    return true;
            return false;
        }
        public static void addToGenericDT(COMPUMETHOD compuMthd)
        {
            if (!checkGenericCompus(compuMthd.SHORTNAME))
                genericDTs.Add(compuMthd);
        }
        public static COMPUMETHOD getGenericDT(string shrtName)
        {
            foreach(var genericCompuMethod in genericDTs)
            {
                if(genericCompuMethod.SHORTNAME.Equals(shrtName, StringComparison.CurrentCultureIgnoreCase))
                {
                    return genericCompuMethod;
                }
                return null;
            }
            return null;
        }
        public static void removeDataType(string dtName, string dtSemanticsName,  string portIntRequesterName)
        {
            var portARPkg = Extract_3_0.getARPackage("PortInterface");
            bool remove = true;
            
            foreach(object pObj in portARPkg.ELEMENTS)
            {
                string pObjShortName = pObj.GetType().GetProperty("SHORTNAME").GetValue(pObj).ToString();
                if (!pObjShortName.Equals(portIntRequesterName))
                {
                    var sndrRcvrInt = pObj as SENDERRECEIVERINTERFACE;
                    var otherDTName = sndrRcvrInt.DATAELEMENTS[0].TYPETREF.Value.Split('/')[2];
                    if (dtSemanticsName.Equals(otherDTName))
                    {
                        remove = false;
                    }
                }
            }
            if (remove)
            {
                if (!genericDTNames.Contains(dtSemanticsName)) 
                { Extract_3_0.removeObjFromPkg("DataTypeSemantics", dtSemanticsName); }
                Extract_3_0.removeObjFromPkg("DataType", dtSemanticsName);
            }
        }

        public static bool compareDataTypes(autosar.COMPUMETHOD compuMethod1, autosar.COMPUMETHOD compuMethod2)
        {
            autosar.COMPUCOMPUSCALES compuscales1 = compuMethod1.COMPUINTERNALTOPHYS.Item as autosar.COMPUCOMPUSCALES;
            autosar.COMPUCOMPUSCALES compuscales2 = compuMethod1.COMPUINTERNALTOPHYS.Item as autosar.COMPUCOMPUSCALES;
            int matchPercent = 0;
            if (compuscales1.Items.Length == compuscales2.Items.Length && compuscales1.Items.Length > 1)
            { // other check removes scalar values scenario
                //autosar.COMPU compuIntToPhys2 = compuMethod.COMPUINTERNALTOPHYS as autosar.COMPU;
                if (compareCompuConst(compuMethod1.COMPUINTERNALTOPHYS, compuMethod2.COMPUINTERNALTOPHYS, compuMethod1.SHORTNAME, compuMethod2.SHORTNAME, out matchPercent))
                {
                    return true;
                }
                return false;
            }
            
            return false;
        }
        public static bool compareCompuConst(autosar.COMPU compu1, autosar.COMPU compu2, string compu1ShrtMane, string compu2ShrtMane, out int matchPercent)
        {
            autosar.COMPUCOMPUSCALES compuScales1 = compu1.Item as autosar.COMPUCOMPUSCALES;
            autosar.COMPUCOMPUSCALES compuScales2 = compu2.Item as autosar.COMPUCOMPUSCALES;
            int equalsCount = 0;
            foreach (object item1 in compuScales1.Items)
            {
                autosar.COMPUSCALE compuScale1 = (autosar.COMPUSCALE)item1;
                autosar.COMPUCONST compuConst1 = compuScale1.Item as autosar.COMPUCONST;
                autosar.LIMIT upperLimit1 = compuScale1.UPPERLIMIT;
                autosar.VT vt1 = compuConst1.Item as autosar.VT;
                string vt1Text = getVTText(compu1ShrtMane, vt1.Text[0]);
                //string vt1Sub = vt1.Text[0].Split('_').Last();
                int curEqCount = equalsCount;
                foreach (object item2 in compuScales2.Items)
                {
                    autosar.COMPUSCALE compuScale2 = (autosar.COMPUSCALE)item2;
                    autosar.COMPUCONST compuConst2 = compuScale2.Item as autosar.COMPUCONST;
                    autosar.LIMIT upperLimit2 = compuScale2.UPPERLIMIT;
                    autosar.VT vt2 = compuConst2.Item as autosar.VT;
                    string vt2Text = getVTText(compu2ShrtMane, vt2.Text[0]);
                    if (vt1Text.Equals(vt2Text, StringComparison.CurrentCultureIgnoreCase) &&
                        upperLimit1.Text[0].Equals(upperLimit2.Text[0], StringComparison.CurrentCultureIgnoreCase))
                    {
                        equalsCount += 1;
                        break;
                    }

                }
                if (equalsCount == curEqCount)
                {
                    break; // if no match for the first
                }
                else { curEqCount = equalsCount; }

            }
            if (equalsCount == compuScales1.Items.Length) // more than 50% similarities exists, alert the user
            {
                matchPercent = 100;
                return true;
            }
            matchPercent = 0;
            return false;
        }
        public static void renameDTSemanticsCompuMethods(COMPUMETHOD extnCompu, string newFullDTName)
        {
            autosar.ARPACKAGE dTPkg = Extract_3_0.getARPackage("DataType");
            autosar.ARPACKAGE dTSemantics = dTPkg.SUBPACKAGES[0];
            foreach (autosar.COMPUMETHOD compuMethod in dTSemantics.ELEMENTS)
            {
                if (extnCompu.SHORTNAME.Equals(compuMethod.SHORTNAME, StringComparison.CurrentCultureIgnoreCase))
                {
                    // compuMethod.SHORTNAME = newFullDTName;  //return compuMethod; 
                    autosar.COMPUCOMPUSCALES dTCompuscales = compuMethod.COMPUINTERNALTOPHYS.Item as autosar.COMPUCOMPUSCALES;
                    if (dTCompuscales.Items.Length > 1)
                    {
                        foreach (object item in dTCompuscales.Items)
                        {
                            autosar.COMPUSCALE compuScale1 = (autosar.COMPUSCALE)item;
                            autosar.COMPUCONST compuConst1 = compuScale1.Item as autosar.COMPUCONST;
                            autosar.VT vt1 = compuConst1.Item as autosar.VT;
                            string vt1Text = getVTText(compuMethod.SHORTNAME, vt1.Text[0]);
                            vt1.Text[0] = $"{newFullDTName.Substring(0, newFullDTName.Length - 1)}{vt1Text}";
                        }
                    }

                    compuMethod.SHORTNAME = newFullDTName;
                    Extract_3_0.editTopNode(dTPkg);

                    //return newFullDTName;
                }
            }
        }
        private static string getVTText(string compu1ShrtName, string vtText)
        {
            string compuShrtName; //Remove _T from name
            if (compu1ShrtName.Length > 2)
            {
                var dtPrefix = compu1ShrtName.Split('_').ToList().Last();
                if (dtPrefix.Equals("dt", StringComparison.CurrentCultureIgnoreCase))
                {
                    compuShrtName = compu1ShrtName.Substring(0, (compu1ShrtName.Length - 3));
                }
                else if (dtPrefix.Equals("T", StringComparison.CurrentCultureIgnoreCase))
                {
                    compuShrtName = compu1ShrtName.Substring(0, (compu1ShrtName.Length - 2));
                }
                else
                {
                    compuShrtName = compu1ShrtName;
                }

            }
            else
            {
                compuShrtName = compu1ShrtName;
            }
            if (vtText.Contains(compuShrtName))
            {
                string newVTText = vtText.Replace(compuShrtName + "_", "");
                return newVTText;
            }
            return vtText;
        }
        public static bool compareGenericDT(COMPU compu1, COMPU exstnCompu, out int matchPercent)
        {
            autosar.COMPUCOMPUSCALES compuScales1 = compu1.Item as autosar.COMPUCOMPUSCALES;
            autosar.COMPUCOMPUSCALES exstnCompuScales = exstnCompu.Item as autosar.COMPUCOMPUSCALES;
            int equalsCount = 0;
            int curEqCount = 0;

            foreach (object item1 in compuScales1.Items)
            {
                autosar.COMPUSCALE compuScale1 = (autosar.COMPUSCALE)item1;
                autosar.COMPUCONST compuConst1 = (autosar.COMPUCONST)compuScale1.Item;
                autosar.LIMIT upperLimit1 = compuScale1.UPPERLIMIT;
                autosar.VT vt1 = compuConst1.Item as autosar.VT;
                string vt1Sub = vt1.Text[0].Split('_').Last(); // break off the signal name from the datatype enum element definition
                vt1Sub = ExtractorUtilities.SplitCamelCase(vt1Sub); // seperate based on camel case
                curEqCount = equalsCount;
                foreach (object item2 in exstnCompuScales.Items)
                {
                    autosar.COMPUSCALE compuScale2 = (autosar.COMPUSCALE)item2;
                    autosar.COMPUCONST compuConst2 = compuScale2.Item as autosar.COMPUCONST;
                    autosar.LIMIT upperLimit2 = compuScale2.UPPERLIMIT;
                    autosar.VT vt2 = compuConst2.Item as autosar.VT;
                    string vt2Sub = vt2.Text[0].Split('_').Last(); // break off the signal name from the datatype enum element definition
                    string[] vt2Subarry = ExtractorUtilities.SplitCamelCase(vt2Sub).Split(); // seperate based on camel case
                    //bool = vt1Sub.Contains()
                    foreach (string temp in vt2Subarry) // loop through all word contained in the array
                    {
                        if (vt1Sub.Contains(temp) &&
                        upperLimit1.Text[0].Equals(upperLimit2.Text[0], StringComparison.CurrentCultureIgnoreCase))
                        {
                            equalsCount += 1;
                            break;
                        }
                    }

                    if (equalsCount > curEqCount)
                        break; // if something is already found break looking for same vt
                }
                if (equalsCount == curEqCount)
                {
                    break; // if no match for the first
                }
                else { curEqCount = equalsCount; }
            }
            if (equalsCount == compuScales1.Items.Length) // more than 50% similarities exists, alert the user
            {
                matchPercent = 100;
                return true;
            }
            matchPercent = 0;
            return false;
            //return false;
        }
        public static bool updateDataTypes(autosar.COMPU compu1, autosar.COMPU compu2)
        {
            return false;
        }
    }
}
