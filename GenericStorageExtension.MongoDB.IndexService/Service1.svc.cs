using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;
using GenericStorageExtension.Common.ExceptionManagement;
using GenericStorageExtension.Common.IndexService.DataContracts;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.DataContracts;
using GenericStorageExtension.Common.Services.Helper;
using GenericStorageExtension.MongoDBIndexService.BAL;

namespace GenericStorageExtension.MongoDB.IndexService
{
    /// <summary>
    /// This class exposes the MongoDB Index Service methods
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]

    public class IndexService : IIndexService
    {
        /// <summary>
        /// This method creates an index in Mongo 
        /// </summary>
        /// <param name="query">An object of IndexRequest need to be passed</param>
        /// <returns>Object of type IndexResponse is returned which has field Result as 0 for success and 1 for failure</returns>
        [WebInvoke(UriTemplate = "/AddDocument/", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public GenericStorageExtensionServiceResponse<IndexResponse> AddDocument(GenericStorageExtensionServiceRequest<IndexRequest> query)
        {

            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Enter into method IndexService.AddDocumnet()");
            GenericStorageExtensionServiceResponse<IndexResponse> serviceResponse = new GenericStorageExtensionServiceResponse<IndexResponse>();
            try
            {

                string language = query.ServicePayload.LanguageInRequest;
                IndexResponse resultValue;

                MI4TIndexManager indexManager = new MI4TIndexManager(language);
                resultValue = indexManager.AddDocument(query.ServicePayload);
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "AddDocumnet is called publish is true");
                serviceResponse.ServicePayload = resultValue;
            }
            catch (Exception ex)
            {

                serviceResponse.ServicePayload = new IndexResponse();
                serviceResponse.ServicePayload.Result = 1;
                serviceResponse.ServicePayload.ErrorMessage = "AddDocumnet is not called ispublish is false";
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "AddDocumnet is not called ispublish is false" + ex.Message);
            }
            return serviceResponse;
        }

        /// <summary>
        /// This method removes an index from MongoDB
        /// </summary>
        /// <param name="query">An object of IndexRequest need to be passed</param>
        /// <returns>Object of type IndexResponse is returned which has field Result as 0 for success and 1 for failure</returns>
        [WebInvoke(UriTemplate = "/RemoveDocument/", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public GenericStorageExtensionServiceResponse<IndexResponse> RemoveDocument(GenericStorageExtensionServiceRequest<IndexRequest> query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering into method IndexService.RemoveDocument");
            GenericStorageExtensionServiceResponse<IndexResponse> serviceResponse = new GenericStorageExtensionServiceResponse<IndexResponse>();
            try
            {
                //serviceResponse.ServicePayload = new IndexResponse();
                //serviceResponse.ServicePayload.Result = 1;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "RemoveDocument is  called publish is true1 ");

                IndexResponse resultValue;
                MI4TIndexManager indexManager = new MI4TIndexManager(query.ServicePayload.LanguageInRequest);
                resultValue = indexManager.RemoveDocument(query.ServicePayload);
                serviceResponse.ServicePayload = resultValue;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "RemoveDocument is  called publish is true 2");
            }
            catch (Exception ex)
            {
                serviceResponse.ServicePayload = new IndexResponse();
                serviceResponse.ServicePayload.Result = 0;
                serviceResponse.ServicePayload.ErrorMessage = "RemoveDocument is not called ispublish is false";
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "RemoveDocument is not called ispublish is false");
                string logString = GenericStorageExtensionServiceConstants.LOG_MESSAGE + Environment.NewLine;
                string request = query != null ? query.ToJSONText() : "Request = NULL";
                logString = string.Concat(logString, string.Format("Service Request: {0}", request),
                                            Environment.NewLine, string.Format("{0}{1}", ex.Message, ex.StackTrace));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                CatchException<IndexResponse>(ex, serviceResponse);
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Exiting from method IndexService.RemoveDocument");
            return serviceResponse;
        }

        private void CatchException<T>(Exception ex, GenericStorageExtensionServiceResponse<T> serviceResponse)
        {
            GenericStorageExtensionServiceFault fault = new GenericStorageExtensionServiceFault();
            ExceptionHelper.HandleException(ex, out fault);
            serviceResponse.ResponseContext.FaultCollection.Add(fault);
        }
    }
}
