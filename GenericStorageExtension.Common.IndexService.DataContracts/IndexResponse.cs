using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.IndexService.DataContracts
{
    [DataContract]
    public class IndexResponse
    {
        [DataMember]
        public int Result { get; set; }

        [DataMember]
        public string ErrorMessage { get; set; }
    }
}
