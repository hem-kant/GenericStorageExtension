using GenericStorageExtension.Common.Configuration;
using GenericStorageExtension.Common.Configuration.Interface;
using GenericStorageExtension.Common.IndexService.DataContracts;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services;
using GenericStorageExtension.Common.Services.Helper;
using GenericStorageExtension.SOLR.IndexService.BAL.ContentMapper;
using Microsoft.Practices.ServiceLocation;
using SolrNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using GenericStorageExtension.SOLR.IndexService.BAL.ContentMapper;
using GenericStorageExtension.Common.IndexService.DataHelper;
using System.Text.RegularExpressions;

namespace GenericStorageExtension.SOLR.IndexService.BAL
{
    public class SolrIndexManager
    {
        private IPropertyConfiguration propertyConfiguration;
        private static IPropertyConfiguration propConfiguration;
        private static object containerLock;

        /// <summary>
        /// Singleton SoleIndexManager static constructor
        /// </summary>
        static SolrIndexManager()
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering static SolrIndexManager.SolrIndexManager()");
            try
            {
                string solrIndexConfigPath = Utility.GetConfigurationValue("SearchIndexServiceConfig");

                GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Config Path: " + solrIndexConfigPath);
                propConfiguration = ConfigurationManager.GetInstance().GetConfiguration(solrIndexConfigPath)
                    as IPropertyConfiguration;
                containerLock = new object();
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw ex;
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.SolrIndexManager()");
        }

        /// <summary>
        /// Instantiate SolrIndexManager for a core
        /// </summary>
        /// <param name="language"></param>
        public SolrIndexManager(string solr_core)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering SolrIndexManager(core) with core:" +
                                   solr_core);
            try
            {
                Startup.Container.Clear();
                Startup.InitContainer();

                string solrURL = propConfiguration.GetString(GenericStorageExtensionServicesConstants.SOLR_URL);
                solrURL = string.Concat(solrURL, GenericStorageExtensionServicesConstants.DELIMITER_SLASH, solr_core);

                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "SOLR URL: " + solrURL);

                Boolean instanceExistsAlready = InstanceExists();
                lock (containerLock)
                {
                    if (!string.IsNullOrEmpty(solrURL))
                    {
                        if (!instanceExistsAlready)
                        {
                            try
                            {
                                Startup.Init<Dictionary<string, object>>(solrURL);
                            }
                            catch (Exception ex)
                            {
                                // Work Around: Need to use mutex here to resolve it properly
                                Startup.Container.Clear();
                                Startup.InitContainer();
                                Startup.Init<Dictionary<string, object>>(solrURL);
                                throw ex;
                            }
                        }
                    }
                }

                GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Instance exists: " +
                                       instanceExistsAlready.ToString());
            }
            catch (Exception ex)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ex.Message + ex.StackTrace);
                throw ex;
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering SolrIndexManager.SolrIndexManager(core)");
        }

        /// <summary>
        /// Returns True if the Solr client instance exists
        /// </summary>
        /// <returns></returns>
        private ISolrOperations<T> GetInstance<T>()
        {
            return ServiceLocator.Current.GetInstance<ISolrOperations<T>>();
        }

        private Boolean InstanceExists()
        {
            IEnumerable<ISolrOperations<Dictionary<string, object>>> instances;
            instances = ServiceLocator.Current.GetAllInstances<ISolrOperations<Dictionary<string, object>>>();
            Boolean indexExists = instances.Count() > 0;
            return indexExists;
        }

        /// <summary>
        /// This method indexes a Component presentation in Solr 
        /// </summary>
        /// <param name="query">IndexRequest containing the component presentation</param>
        /// <returns>IndexResponse indicating success or failure</returns>
        public IndexResponse AddDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                       "Entering SolrIndexManager.AddDocument for TCM URI: " +
                       query.ItemURI);

            IndexResponse response = new IndexResponse();

            OperationResult result = OperationResult.Failure;

            try
            {
                XmlDocument dcpXML = new XmlDocument();
                string removeWildCharDCP = string.Empty;
                if (!string.IsNullOrEmpty(query.DCP))
                {
                    removeWildCharDCP = Regex.Replace(query.DCP.ToString(), @"\b'[s]", "");
                    dcpXML.LoadXml(removeWildCharDCP);
                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "DCP XML before adding child element " + dcpXML);
                    Dictionary<string, string> additionalParameters = GetAdditionalParameters();
                    SetAdditionalParameters(additionalParameters, ref dcpXML);
                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "DCP XML post adding child element " + query.DCP);
                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "**************************************************************** ");
                    GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "dcpXML post adding child element " + dcpXML);
                }

                AddDocumentXML(dcpXML);
                CommitToSOLR();
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
                                   "Exiting SolrIndexManager.AddDocument, Result: " +
                                   result.ToString());

            return response;
        }

        /// <summary>
        /// This method removes an index from Solr 
        /// </summary>
        /// <param name="query">IndexRequest containing delete criteria</param>
        /// <returns>IndexResponse indicating success or failure</returns>
        public IndexResponse RemoveDocument(IndexRequest query)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering SolrIndexManager.RemoveDocument for TCM URI: " +
                                  query.ItemURI);

            IndexResponse response = new IndexResponse();

            OperationResult result = OperationResult.Failure;

            try
            {
                // To Remove element and child elements from the SOLR
                //AbstractSolrQuery deleteQuery = new SolrQueryByField(SolrServicesConstants.BaseParent, query.ItemURI);
                AbstractSolrQuery deleteQuery = new SolrQueryByField(GenericStorageExtensionServicesConstants.TCM_URI, query.ItemURI);
                RemoveFromSOLR(deleteQuery);
                CommitToSOLR();
                result = OperationResult.Success;
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
                                   "Exiting SolrIndexManager.RemoveDocument, Result: " +
                                   result.ToString());

            return response;
        }

        /// <summary>
        /// Reindexes all content 
        /// </summary>
        /// <param name="query">IndexRequest containing publication information</param>
        /// <returns>IndexResponse indicating success or failure</returns>
        /*public IndexResponse ReIndex(IndexRequest request)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, "Entering SolrIndexManager.ReIndex for TCM URI: " + 
                                  request.ItemURI);
            
            OperationResult result = OperationResult.Failure;
            string[] schemas;
            int publicationID = Convert.ToInt32(request.ItemURI);
            
            IndexResponse response = new IndexResponse();
            try
            {
                propertyConfiguration = ConfigurationManager.GetInstance().GetConfiguration(SolrServicesConstants.SCHEMA_ID_MAPPING_CONFIG) 
                    as IPropertyConfiguration;
                
                schemas = propertyConfiguration.GetPropertyArray("Schema");
                
                ReIndex(schemas, publicationID);
                CommitToSOLR();
                result = OperationResult.Success;
            }
            catch (Exception ex)
            {
                string logString = ServiceConstants.LOG_MESSAGE + Environment.NewLine;
                string itemURI = request != null ? request.ItemURI : "Request Query = NULL";
                logString = string.Concat(logString, string.Format("Item URI : {0}", itemURI),
                                            Environment.NewLine, string.Format("{0}{1}", ex.Message, ex.StackTrace));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                result = OperationResult.Failure;
            }
            response.Result = (int) result;
            
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO, 
                                   "Exiting SolrIndexManager.ReIndex, Result: " + 
                                   result.ToString());
            
            return response;
        }*/

        /// <summary>
        /// Adds a list of component presentation XML Documents to Solr
        /// </summary>
        /// <param name="productDocuments">List of component presentation XMLs</param>
        /// <returns>IndexResponse</returns>
        public IndexResponse AddMultipleDocumentXML(List<XmlDocument> documents)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                   "Entering SolrIndexManager.UpdateProductDocuments: "
                                   + documents.Count
                                   + " documents");

            OperationResult result = OperationResult.Failure;

            IndexResponse response = new IndexResponse();

            try
            {
                documents.ForEach(document => AddDocumentXML(document));
                CommitToSOLR();
                result = OperationResult.Success;
            }
            catch (Exception ex)
            {
                string logString = GenericStorageExtensionServiceConstants.LOG_MESSAGE + Environment.NewLine;
                logString = string.Concat(logString, Environment.NewLine,
                                          string.Format("{0}{1}", ex.Message, ex.StackTrace));

                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                result = OperationResult.Failure;
            }

            response.Result = (int)result;

            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                   "Exiting SolrIndexManager.AddProductDocuments, Result: " +
                                   result.ToString());
            return response;
        }

        /// <summary>
        /// This method removes all documents of the given type from a specific Solr Core
        /// </summary>
        /// <returns>IndexResponse</returns>
        public IndexResponse RemoveAllDocumentsOfType(string content_type)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                   "Entering SolrIndexManager.RemoveAllDocumentsOfType, Content Type: " +
                                   content_type);

            IndexResponse response = new IndexResponse();
            OperationResult result = OperationResult.Failure;

            try
            {
                AbstractSolrQuery query = new SolrQueryByField(GenericStorageExtensionServicesConstants.ContentType, content_type);
                RemoveFromSOLR(query);
                CommitToSOLR();
                result = OperationResult.Success;
            }
            catch (Exception ex)
            {
                string logString = GenericStorageExtensionServiceConstants.LOG_MESSAGE + Environment.NewLine;
                logString = string.Concat(logString, string.Format("{0}{1}", ex.Message, ex.StackTrace));
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, logString);
                result = OperationResult.Failure;
            }

            response.Result = (int)result;
            GenericStorageExtensionLogger.WriteLog(ELogLevel.INFO,
                                   "Exiting SolrIndexManager.RemoveAllDocumentsOfType, Result: " +
                                   result.ToString());
            return response;
        }

        /// <summary>
        /// This method takes Publication ID and Schema Array  as input and inserts the index in Solr. 
        /// This is used to ReIndex all the document type which are the candidates for indexing.
        /// </summary>
        /// <param name="schemaArray">Schema Array</param>
        /// <param name="contentRepositoryId">Content Repository ID</param>
        private void ReIndex(string[] schemaArray, int contentRepositoryId)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering SolrIndexManager.ReIndex");

            RemoveALLFromSOLR(contentRepositoryId);

            foreach (string schemaID in schemaArray)
            {
                ReIndexSchema(schemaID, contentRepositoryId);
            }

            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.ReIndex");
        }

        /// <summary>
        /// Re Index all documents of a Content Type identified by Schema ID
        /// </summary>
        /// <param name="schemaID">Schema ID</param>
        /// <param name="contentRepositoryId">Content Repository ID</param>
        private void ReIndexSchema(string schemaID, int contentRepositoryId)
        {
            List<XmlDocument> componentXMLs = TridionDataHelper.GetContentByType(Convert.ToInt32(schemaID),
                                                                                contentRepositoryId,
                                                                                Int32.MaxValue);

            if (componentXMLs == null) return;

            // Do not process null value component presentations
            componentXMLs = componentXMLs.Where(xdoc => xdoc != null).ToList();

            componentXMLs.ForEach(xdoc => AddDocumentXML(xdoc));
        }

        /// <summary>
        /// Adds XML document to Solr. 
        /// Maps the content to a list of Solr documents and adds them.
        /// </summary>
        /// <param name="componentXML">XML Document containing Component Presentation</param>
        private void AddDocumentXML(XmlDocument componentXML)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering SolrIndexManager.AddDocumentXML");

            string compXML = componentXML != null ? componentXML.InnerXml : " NULL";
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Component XML: " + compXML);

            MappedContent mappedContent = ContentMapper.ContentMapper.GetMappedContent(componentXML);
            AddToSolr(mappedContent.MappedDocuments, mappedContent.DeleteCriteria);

            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.AddDocumentXML");
        }

        /// <summary>
        /// Adds passed documents and deletes existing documents matching delete criteria
        /// </summary>
        /// <param name="docs">List of documents to add</param>
        /// <param name="dels">List of delete crileria as key-value pairs</param>
        private void AddToSolr(List<Dictionary<string, object>> documentsToAdd,
                               List<KeyValuePair<string, object>> deleteCriteria)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering SolrIndexManager.AddToSolr");
            var solr = ServiceLocator.Current.GetInstance<ISolrOperations<Dictionary<string, object>>>();

            List<Dictionary<string, object>> deldocs = new List<Dictionary<string, object>>();

            List<SolrQueryByField> deleteQueries = deleteCriteria.Select(item =>
                                                               new SolrQueryByField(item.Key, (string)item.Value)).ToList();

            if (deleteQueries.Count > 0)
            {
                AbstractSolrQuery deleteQuery = deleteQueries[0];
                foreach (SolrQueryByField dq in deleteQueries)
                {
                    deleteQuery |= dq;
                }
                solr.Delete(deleteQuery);
            }
            solr.Add(documentsToAdd);
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.AddToSolr");
        }

        /// <summary>
        /// Removes Solr Indexes based on the query
        /// </summary>
        /// <param name="solrQuery">Solr Query</param>
        private void RemoveFromSOLR(AbstractSolrQuery solrQuery)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering SolrIndexManager.RemoveFromSOLR");
            var solr = GetInstance<Dictionary<string, object>>();
            solr.Delete(solrQuery);
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.RemoveFromSOLR");
        }

        /// <summary>
        /// Commits changes till now to Solr
        /// </summary>
        /// <param name="rebuild">Oprimize Solr and Rebuild SpellCheck Dictoinary</param>
        private void CommitToSOLR(Boolean rebuild = false)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG,
                                   "Entering SolrIndexManager.CommitToSOLR, Rebuild: " + rebuild.ToString());
            var solr = GetInstance<Dictionary<string, object>>();
            solr.Commit();
            if (rebuild)
            {
                solr.Optimize();
                solr.BuildSpellCheckDictionary();
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.CommitToSOLR");
        }

        /// <summary>
        /// Removes all content for the given Publication ID
        /// </summary>
        /// <param name="publicationID">Publication ID</param>
        private void RemoveALLFromSOLR(int publicationID)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG,
                                   "Entering SolrIndexManager.RemoveALLFromSOLR for Publication ID: " +
                                   publicationID.ToString());

            string publication = "tcm:{0}-*";
            publication = string.Format(publication, publicationID.ToString());

            AbstractSolrQuery query = new SolrQueryByField(GenericStorageExtensionServicesConstants.TCM_URI, publication);
            RemoveFromSOLR(query);

            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Exiting SolrIndexManager.RemoveALLFromSOLR");
        }

        private Dictionary<string, string> GetAdditionalParameters()
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering In GetAdditionalParameters");
            List<string> parameters = GetParameterList();
            Dictionary<string, string> additionalParameters = new Dictionary<string, string>();
            foreach (string parameter in parameters)
            {
                switch (parameter)
                {
                    case "lastpublisheddate":
                        //additionalParameters.Add(parameter, DateTime.Now.ToShortDateString());
                        additionalParameters.Add(parameter, String.Format("{0:s}", DateTime.Now));
                        break;
                }
            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "End In GetAdditionalParameters additionalParameters " + additionalParameters);
            return additionalParameters;
        }

        private List<string> GetParameterList()
        {
            List<string> paramter = new List<string>();
            paramter.Add("lastpublisheddate");


            return paramter;
        }

        private void SetAdditionalParameters(Dictionary<string, string> parameters, ref XmlDocument xdoc)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "Entering In SetAdditionalParameters");
            foreach (KeyValuePair<string, string> parameter in parameters)
            {
                XmlElement additionalParam = xdoc.CreateElement(parameter.Key);
                additionalParam.InnerText = parameter.Value;
                xdoc.DocumentElement.AppendChild(additionalParam);

            }
            GenericStorageExtensionLogger.WriteLog(ELogLevel.DEBUG, "End In SetAdditionalParameters");
        }
    }
}
