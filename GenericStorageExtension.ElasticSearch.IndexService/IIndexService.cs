
using GenericStorageExtension.Common.IndexService.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

using GenericStorageExtension.Common.Services.DataContracts;

namespace GenericStorageExtension.ElasticSearch.IndexService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IIndexService
    {

        /// <summary>
        /// operation contract to create index
        /// </summary>
        /// <param name="query">An object of type IndexRequest need to be passed </param>
        /// <returns>IndexResponse object which will have Result as 1 for failure and 0 for success</returns>       
        [OperationContract]
        GenericStorageExtensionServiceResponse<IndexResponse> AddDocument(GenericStorageExtensionServiceRequest<IndexRequest> query);

        /// <summary>
        /// operation contract to delete index
        /// </summary>
        /// <param name="query">An object of type IndexRequest need to be passed  </param>
        /// <returns>IndexResponse object which will have Result as 0 for failure and 1 for success</returns>
        [OperationContract]
        GenericStorageExtensionServiceResponse<IndexResponse> RemoveDocument(GenericStorageExtensionServiceRequest<IndexRequest> query);


    }
}
