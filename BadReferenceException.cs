using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EcuExtractCreator
{
    public class BadReferenceException : Exception
    {
        private string _errMsg;

        public string ErrMsg
        {
            get { return _errMsg; }
            set { _errMsg = value; }
        }

        public BadReferenceException(string errMsg) : base(errMsg)
        {
            _errMsg = errMsg;
        }
    }
}
