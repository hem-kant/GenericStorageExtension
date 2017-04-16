using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.SOLR.IndexService.BAL.ContentMapper
{
    public enum FieldDataType
    {
        StringField,
        IntegerField,
        BinaryField,
        DateField,
        TextField,
        LinkField,
        ComplexLinkField,
        BinarySerializedField,
        CurrentNodePosition,
        ParentNodePosition,
        Multimedia,
        Multivalued
    }
}
