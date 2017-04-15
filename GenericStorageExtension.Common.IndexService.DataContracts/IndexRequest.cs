using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.IndexService.DataContracts
{
    [DataContract]
    public class IndexRequest
    {
        [DataMember]
        public string ItemURI { get; set; }

        [DataMember]
        public string DCP { get; set; }

        [DataMember]
        public string ContentType { get; set; }

        [DataMember]
        public string LanguageInRequest { get; set; }
    }
}
