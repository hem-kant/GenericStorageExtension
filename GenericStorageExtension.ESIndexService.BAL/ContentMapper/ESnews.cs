using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace GenericStorageExtension.ESIndexService.BAL.ContentMapper
{
    [Serializable()]
    [XmlRoot(ElementName = "esnews")]
    public class Esnews
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("publicationID")]
        public string publicationID { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("shortTitle")]
        public string ShortTitle { get; set; }

        [XmlElement("authorName")]
        public string AuthorName { get; set; }

        [XmlElement("aboutAuthor")]
        public string AboutAuthor { get; set; }

        [XmlElement("Category")]
        public string Category { get; set; }

        [XmlElement("relasedDate")]
        public DateTime? RelasedDate { get; set; }

        [XmlElement("imageUrl")]
        public string ImageUrl { get; set; }

        [XmlAttribute("xmlns")]
        public string Xmlns { get; set; }
    }
}
