using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GenericStorageExtension.MongoDB.SearchService.BAL.Model
{
    [Serializable()]
    [XmlRoot(ElementName = "Content")]
    public class MongoDBModelSearch
    {
        [XmlElement("id")]
        [BsonId]
        public object id { get; set; }

        [XmlElement("title")]
        public string title { get; set; }

        [XmlElement("description")]
        public string description { get; set; }

        [XmlElement("imageurl")]
        public string imageurl { get; set; }

        [XmlElement("ItemURI")]
        public string ItemURI { get; set; }

        [XmlElement("publicationID")]
        public string publicationID { get; set; }

    }
}
