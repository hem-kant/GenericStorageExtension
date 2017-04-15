using GenericStorageExtension.Common.Configuration;
using GenericStorageExtension.Common.Configuration.Interface;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.ESIndexService.BAL.ContentMapper;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.Helper;
using GenericStorageExtension.Common.IndexService.DataContracts;
using Nest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using System.Web.Script.Serialization;
using System.Net;

namespace GenericStorageExtension.ESIndexService.BAL
{
    public class GenericStorageExtensionIndexManager
    {
        private IPropertyConfiguration propertyConfiguration;
        private static IPropertyConfiguration propConfiguration;
        private static object containerLock;
        public static Uri node;
        public static ConnectionSettings settings;
        public static ElasticClient client;
        public object ServiceConstants { get; private set; }

        /// <summary>
        /// Singleton ESI4TIndexManager static constructor
        /// </summary>
        static GenericStorageExtensionIndexManager()
        {
            try
            {
                string ElasticIndexConfigPath = Utility.GetConfigurationValue("SearchIndexServiceConfig");
                propConfiguration = ConfigurationManager.GetInstance().GetConfiguration(ElasticIndexConfigPath)
                    as IPropertyConfiguration;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Config Path: " + ElasticIndexConfigPath);
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw ex;
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting ESI4TIndexManager.ESI4TIndexManager()");
        }
        public GenericStorageExtensionIndexManager(string Langauge)
        {

            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering ESI4TIndexManager:" +
            Langauge);
            try
            {
                string elasticSearchURL = propConfiguration.GetString(GenericStorageExtensionServicesConstants.Search_URL);
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "elasticSearch URL: " + elasticSearchURL);
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw;
            }
        }
        public Common.IndexService.DataContracts.IndexResponse AddDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
           "Entering ESI4TIndexManager.AddDocument for TCM URI: " +
           query.ItemURI);

            Common.IndexService.DataContracts.IndexResponse response = new Common.IndexService.DataContracts.IndexResponse();

            OperationResult result = OperationResult.Failure;
            try
            {
                XmlDocument doc = new XmlDocument();
                string ID = string.Empty;
                doc.LoadXml(Utility.UpdateContentTypeXML(Regex.Replace(query.DCP.ToString(), @"\b'\b", "")));
                string jsonText = JsonConvert.SerializeXmlNode(doc);

                //     var bln = Deserialize<Esnews>(doc);

                var conString = GenericStorageExtensionServicesConstants.Search_URL;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "conString: " + conString);

                node = new Uri(conString);
                settings = new ConnectionSettings(node);
                settings.DefaultIndex("fromelasticstoweb8");
                var client = new Nest.ElasticClient(settings);
                var indexResponse = client.LowLevel.Index<string>("fromelasticstoweb8", "esnews", jsonText);
                //     var responseBool = client.Index(bln);
                result = OperationResult.Success;

            }
            catch (Exception ex)
            {
                string logString = GenericStorageExtensionServiceConstants.LOG_MESSAGE + Environment.NewLine;

                logString = string.Concat(logString,
                                          Environment.NewLine,
                                          string.Format("{0}{1}", ex.Message, ex.StackTrace));

                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                result = OperationResult.Failure;
            }
            response.Result = (int)result;
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                  "Exiting ESI4TIndexManager.AddDocument, Result: " +
                                   result.ToString());

            return response;
        }

        /// <summary>
        /// This method removes an index from Elastic 
        /// </summary>
        /// <param name="query">IndexRequest containing delete criteria</param>
        /// <returns>IndexResponse indicating success or failure</returns>
        public Common.IndexService.DataContracts.IndexResponse RemoveDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering ESI4TIndexManager.RemoveDocument for TCM URI: " +
                                 query.ItemURI);
            Common.IndexService.DataContracts.IndexResponse response = new Common.IndexService.DataContracts.IndexResponse();
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            serializer.MaxJsonLength = Int32.MaxValue;
            var webClient = new WebClient();
            OperationResult result = OperationResult.Failure;
            try
            {
                XmlDocument doc = new XmlDocument();
                string ID = query.ItemURI;
                string strId = "\"" + ID + "\"";
                var content = webClient.DownloadString(@"http://localhost:9200/fromelasticstoweb8/_search?q=" + strId + "");
                dynamic data = serializer.Deserialize(content, typeof(object));
                var da = serializer.Deserialize<dynamic>(content);
                string Id = string.Empty;
                string idValue = string.Empty;
                //doc.LoadXml(Utility.UpdateContentTypeXML(Regex.Replace(query.DCP.ToString(), @"\b'\b", "")));
                foreach (var item in data)
                {
                    var aa = item;
                    if (aa.Key == "hits")
                    {
                        foreach (var item2 in aa.Value)
                        {
                            var aaaa = item2;
                            if (aaaa.Key == "hits")
                            {
                                foreach (var item3 in aaaa.Value)
                                {
                                    foreach (var item4 in item3)
                                    {
                                        if (item4.Key == "_id")
                                        {
                                            Id = item4.Key;
                                            idValue = item4.Value;
                                        }
                                    }


                                }
                            }
                        }

                    }

                }
                //var bln = Deserialize<Esnews>(doc);
                node = new Uri("http://localhost:9200");

                settings = new ConnectionSettings(node);

                settings.DefaultIndex("fromelasticstoweb8");
                var client = new Nest.ElasticClient(settings);
                var responseReturn = client.Delete<Esnews>(idValue, d => d
                 .Index("fromelasticstoweb8")
                 .Type("esnews"));
                result = OperationResult.Success;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Exit ESI4TIndexManager.RemoveDocument for TCM URI: " +
                                 query.ItemURI + " result " + result);
            }
            catch (Exception ex)
            {
                string logString = GenericStorageExtensionServiceConstants.LOG_MESSAGE + Environment.NewLine;
                logString = string.Concat(logString,
                                          string.Format("Item URI : {0}", query.ItemURI),
                                          Environment.NewLine, string.Format("{0}{1}", ex.Message, ex.StackTrace));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                result = OperationResult.Failure;
            }

            response.Result = (int)result;

            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                  "Exiting ESI4TIndexManager.RemoveDocument, Result: " +
                                  result.ToString());

            return response;
        }
        public static T Deserialize<T>(XmlDocument xmlDocument)
        {
            XmlSerializer ser = new XmlSerializer(typeof(T));

            StringReader reader = new StringReader(xmlDocument.InnerXml);
            XmlReader xmlReader = new XmlTextReader(reader);
            //Deserialize the object.
            return (T)ser.Deserialize(xmlReader);
        }
    }
}
