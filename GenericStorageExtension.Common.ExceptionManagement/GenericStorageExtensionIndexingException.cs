using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.ExceptionManagement
{
    public class GenericStorageExtensionIndexingException:ApplicationException
    {
        public GenericStorageExtensionIndexingException() : base()
        {
        }
        public string Code { get; set; }

        public GenericStorageExtensionIndexingException(string code, string errorMessage)
			: base(errorMessage)
		{
            Code = code;
        }

        public GenericStorageExtensionIndexingException(string msg, Exception ex)
            : base(msg, ex)
        {
        }

        public GenericStorageExtensionIndexingException(string msg)
            : base(msg)
        {
        }
    }
}
