using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.Services.DataContracts
{
    /// <summary>
    /// The service fault class is a data contract which would hold the error code and error messages
    /// corresponding to the error(s) occurred during service method execution
    /// </summary>
    [DataContract]
    public class GenericStorageExtensionServiceFault
    {
        private string code;
        /// <summary>
        /// Fault Code of the error
        /// </summary>
        [DataMember]
        public string Code
        {
            get { return code; }
            set { code = value; }
        }

        private string message;

        /// <summary>
        /// Fault Message corresponding to the fault code
        /// </summary>
        [DataMember]
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
    }
}
