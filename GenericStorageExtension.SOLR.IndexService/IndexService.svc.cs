using GenericStorageExtension.Common.ExceptionManagement;
using GenericStorageExtension.Common.IndexService.DataContracts;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.DataContracts;
using GenericStorageExtension.Common.Services.Helper;
using GenericStorageExtension.SOLR.IndexService.BAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Activation;
using System.ServiceModel.Web;
using System.Text;

namespace GenericStorageExtension.SOLR.IndexService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class Service1 : IIndexService
    {
        [WebInvoke(UriTemplate = "/AddDocument/", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public GenericStorageExtensionServiceResponse<IndexResponse> AddDocument(GenericStorageExtensionServiceRequest<IndexRequest> query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Enter into method IndexService.AddDocumnet()");
            GenericStorageExtensionServiceResponse<IndexResponse> serviceResponse = new GenericStorageExtensionServiceResponse<IndexResponse>();
            try
            {

                string language = query.ServicePayload.LanguageInRequest;
                IndexResponse resultValue;

                SolrIndexManager indexManager = new SolrIndexManager(language);
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

        [WebInvoke(UriTemplate = "/RemoveDocument/", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public GenericStorageExtensionServiceResponse<IndexResponse> RemoveDocument(GenericStorageExtensionServiceRequest<IndexRequest> query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering into method IndexService.RemoveDocument");
            GenericStorageExtensionServiceResponse<IndexResponse> serviceResponse = new GenericStorageExtensionServiceResponse<IndexResponse>();
            string result = string.Empty;

            try
            {
                IndexResponse resultValue;
                SolrIndexManager indexManager = new SolrIndexManager(query.ServicePayload.LanguageInRequest);
                resultValue = indexManager.RemoveDocument(query.ServicePayload);
                serviceResponse.ServicePayload = resultValue;
            }
            catch (Exception ex)
            {
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
