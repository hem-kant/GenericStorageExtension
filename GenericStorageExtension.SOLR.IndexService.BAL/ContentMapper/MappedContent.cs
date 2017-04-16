using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.SOLR.IndexService.BAL.ContentMapper
{
    public class MappedContent
    {
        public List<Dictionary<string, object>> MappedDocuments { get; set; }

        public List<KeyValuePair<string, object>> DeleteCriteria { get; set; }

        public MappedContent(List<Dictionary<string, object>> docs, List<KeyValuePair<string, object>> dels)
        {
            this.MappedDocuments = docs;
            this.DeleteCriteria = dels;
        }
    }
}
