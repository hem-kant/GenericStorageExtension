using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.Services.DataContracts
{
    /// <summary>
    /// </summary>
    /// <typeparam name="T">Service specific request payload data contact</typeparam>
    [DataContract]
    public class GenericStorageExtensionServiceRequest<T>
    {

        public GenericStorageExtensionServiceRequest()
        {
        }

        private T servicePayload;

        /// <summary>
        /// The service payload is a generic implementation for any service which would hold the service
        /// request data send by the consumer
        /// </summary>
        [DataMember]
        public T ServicePayload
        {
            get { return servicePayload; }
            set { servicePayload = value; }
        }

    }
}
