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
    /// <typeparam name="T">Service specific response payload data contact</typeparam>
    [DataContract]
    public class GenericStorageExtensionServiceResponse<T>
    {
        public GenericStorageExtensionServiceResponse()
        {
            ResponseContext = new GenericStorageExtensionResponseContext();
        }
        private T servicePayload;


        /// <summary>
        /// The service payload is a generic implementation for any service which would hold the service
        /// response data send to the consumer
        /// </summary>
        [DataMember]
        public T ServicePayload
        {
            get { return servicePayload; }
            set { servicePayload = value; }
        }

        private GenericStorageExtensionResponseContext responseContext;


        /// <summary>
        /// The response context would hold the data related to response which would be populated by  
        /// underlying service/service client
        /// </summary>
        [DataMember]
        public GenericStorageExtensionResponseContext ResponseContext
        {
            get { return responseContext; }
            set { responseContext = value; }
        }
    }
}
