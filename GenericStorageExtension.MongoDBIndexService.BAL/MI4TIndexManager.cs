using GenericStorageExtension.Common.Configuration;
using GenericStorageExtension.Common.Configuration.Interface;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.Helper;
using GenericStorageExtension.Common.IndexService.DataContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;
using GenericStorageExtension.MongoDBIndexService.BAL.ContentMapper;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Xml.Serialization;
using System.IO;
using MongoDB.Driver.Builders;

namespace GenericStorageExtension.MongoDBIndexService.BAL
{
    public class MI4TIndexManager
    {
        private IPropertyConfiguration propertyConfiguration;
        private static IPropertyConfiguration propConfiguration;
        private static object containerLock;

        public object ServiceConstants { get; private set; }

        /// <summary>
        /// Singleton MI4TIndexManager static constructor
        /// </summary>
        static MI4TIndexManager()
        {
            try
            {
                string MongoDBIndexConfigPath = Utility.GetConfigurationValue("SearchIndexServiceConfig");
                propConfiguration = ConfigurationManager.GetInstance().GetConfiguration(MongoDBIndexConfigPath)
                    as IPropertyConfiguration;
                containerLock = new object();
                GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Config Path: " + MongoDBIndexConfigPath);
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw ex;
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting MI4TIndexManager.MI4TIndexManager()");
        }
        public MI4TIndexManager(string Langauge)
        {

            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering MI4TIndexManager:" +
            Langauge);
            try
            {
                string MongoDBURL = propConfiguration.GetString(GenericStorageExtensionServicesConstants.Search_URL);
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Mongo URL: " + MongoDBURL);
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw;
            }
        }
        public IndexResponse AddDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
           "Entering MI4TIndexManager.AddDocument for TCM URI: " +
           query.ItemURI);

            IndexResponse response = new IndexResponse();

            OperationResult result = OperationResult.Failure;
            try
            {
                XmlDocument doc = new XmlDocument();
                string ID = string.Empty;
                doc.LoadXml(Utility.UpdateContentTypeXML(Regex.Replace(query.DCP.ToString(), @"\b'\b", "")));
                var bln = Deserialize<MongoDBModel>(doc);

                var conString = propConfiguration.GetString(GenericStorageExtensionServicesConstants.Search_URL);
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "conString: " +
           conString);
                /// creating MongoClient
                var client = new MongoClient(conString);

                ///Get the database
                var DB = client.GetDatabase(propConfiguration.GetString(GenericStorageExtensionServicesConstants.dbName));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "dbName: " +
         propConfiguration.GetString(GenericStorageExtensionServicesConstants.dbName));

                ///Get the collcetion from the DB in which you want to insert the data                
                var collection = DB.GetCollection<MongoDBModel>(propConfiguration.GetString(GenericStorageExtensionServicesConstants.tableName));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "tableName: " +
                    propConfiguration.GetString(GenericStorageExtensionServicesConstants.tableName));
                //var filter = Builders<MongoDBModel>.Filter.Eq("ItemURI", bln.ItemURI);
                //var result11 =  collection.Find(filter).ToListAsync();

                ///Insert data in to MongoDB
                collection.InsertOne(bln);
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
                                  "Exiting MI4TIndexManager.AddDocument, Result: " +
                                   result.ToString());

            return response;
        }

        /// <summary>
        /// This method removes an index from Mongo 
        /// </summary>
        /// <param name="query">IndexRequest containing delete criteria</param>
        /// <returns>IndexResponse indicating success or failure</returns>
        public IndexResponse RemoveDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering MI4TIndexManager.RemoveDocument for TCM URI: " +
                                 query.ItemURI);
            IndexResponse response = new IndexResponse();

            OperationResult result = OperationResult.Failure;
            try
            {
                
                MongoServerSettings settings = new MongoServerSettings();
                settings.Server = new MongoServerAddress("localhost", 27017);
                // Create server object to communicate with our server
                MongoServer server = new MongoServer(settings);
                
                MongoDatabase myDB = server.GetDatabase("customerDatabase");

                MongoCollection<BsonDocument> records = myDB.GetCollection<BsonDocument>("article");
                var query1 = Query.EQ("ItemURI", query.ItemURI);
                records.Remove(query1);

                result = OperationResult.Success;
                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Exit MI4TIndexManager.RemoveDocument for TCM URI: " +
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
                                  "Exiting MI4TIndexManager.RemoveDocument, Result: " +
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
