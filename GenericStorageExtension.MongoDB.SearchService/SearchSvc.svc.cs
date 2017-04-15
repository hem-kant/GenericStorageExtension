using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using GenericStorageExtension.Common.ExceptionManagement;
using GenericStorageExtension.Common.IndexService.DataContracts;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.DataContracts;
using GenericStorageExtension.Common.Services.Helper;
using System.ServiceModel.Activation;
using GenericStorageExtension.Common.Configuration.Interface;
using System.IO;
using GenericStorageExtension.Common.Configuration;
using GenericStorageExtension.MongoDB.SearchService.BAL.Model;
using System.Collections;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using MongoDB.Bson;
using GenericStorageExtension.MongoDBIndexService.BAL.ContentMapper;

namespace GenericStorageExtension.MongoDB.SearchService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    [ServiceBehavior(AddressFilterMode = AddressFilterMode.Any)]
    public class SearchSvc : IMongoDBSearchService
    {
        private IPropertyConfiguration propertyConfiguration;
        private static IPropertyConfiguration propConfiguration;
        private static object containerLock;

        [WebInvoke(UriTemplate = "/GetContentFromMongoDB/", Method = "POST", ResponseFormat = WebMessageFormat.Json)]
        public Stream GetContentFromMongoDB(GenericStorageExtensionServiceRequest<SearchRequest> request)
        {
            string resultJSON = string.Empty;
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering into GetContentFromMongoDB");
            try
            {
                if (request != null && request.ServicePayload != null)
                {
                    string MongoDBIndexConfigPath = Utility.GetConfigurationValue("IndexServiceConfig");
                    propConfiguration = ConfigurationManager.GetInstance().GetConfiguration(MongoDBIndexConfigPath)
                        as IPropertyConfiguration;
                    containerLock = new object();
                    string result = string.Empty;
                    var connectionString = propConfiguration.GetString(GenericStorageExtensionServicesConstants.Search_URL);
                    var client = new MongoClient(connectionString);
                    var server = client.GetServer();
                    var database = server.GetDatabase(propConfiguration.GetString(GenericStorageExtensionServicesConstants.dbName));
                    var collection = database.GetCollection<MongoDBModelSearch>(propConfiguration.GetString(GenericStorageExtensionServicesConstants.tableName));

                    var andList = new List<IMongoQuery>();
                    foreach (DictionaryEntry entry in request.ServicePayload.Filters)
                    {
                        GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Reading request.ServicePayload.Filters");
                        switch (request.ServicePayload.QueryType.ToUpper())
                        {
                            case "AND":
                                andList.Add(Query.EQ(entry.Key.ToString(), entry.Value.ToString()));
                                break;
                            case "OR":
                                andList.Add(Query.Or(Query.EQ(entry.Key.ToString(), entry.Value.ToString())));
                                break;
                            default:
                                andList.Add(Query.Not(Query.EQ(entry.Key.ToString(), entry.Value.ToString())));
                                break;
                        }

                    }
                    var query = Query.And(andList);
                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Query generated");
                    //Map/Reduce            
                    var map =
                        "function() {" +
                        "    for (var key in this) {" +
                        "        emit(key, { count : 1 });" +
                        "    }" +
                        "}";

                    var reduce =
                        "function(key, emits) {" +
                        "    total = 0;" +
                        "    for (var i in emits) {" +
                        "        total += emits[i].count;" +
                        "    }" +
                        "    return { count : total };" +
                        "}";
                    MapReduceArgs n = new MapReduceArgs();
                    n.MapFunction = map;
                    n.ReduceFunction = reduce;
                    var mr = collection.MapReduce(n);
                    foreach (var document in mr.GetResults())
                    {
                        document.ToJson();
                    }

                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Calling collection.FindOne(query)");
                    //  var entity = collection.Find(query).ToListAsync();
                    var result1 = collection.FindAs<MongoDBModelSearch>(query);
                    resultJSON = result1.ToJson();

                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "OUTPUT: " + resultJSON);
                }
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, "ERROR: " + ex.Message + ex.StackTrace);
            }
            return new MemoryStream(Encoding.UTF8.GetBytes(resultJSON));
        }


    }
}
