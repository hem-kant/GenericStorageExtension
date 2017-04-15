using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.Services.DataContracts
{
    /// <summary>
    /// SecurityToken class defines a data contract that populates all the information 
    /// about the security used by the calling method.
    /// </summary>
    [DataContract]
    public class GenericIndexingSecurityToken
    {
        /// <summary>
        /// Get/Set the identifier to identify the tenant
        /// </summary>
        [DataMember]
        public string TenantID { get; set; }

        /// <summary>
        /// Get/Set the name of the tenant
        /// </summary>
        [DataMember]
        public string TenantName { get; set; }

        /// <summary>
        /// Get/Set the identifier to identify the current User
        /// </summary>
        [DataMember]
        public string UserID { get; set; }

        /// <summary>
        /// Get/Set security token string
        /// </summary>
        [DataMember]
        public string TokenString { get; set; }
    }
}
