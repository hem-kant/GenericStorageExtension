using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.Configuration
{
    public class ConfigurationSectionHandler : IConfigurationSectionHandler
    {
        #region Public instance constructors

        /// <summary>
        /// Default class constructor.
        /// </summary>
        public ConfigurationSectionHandler()
        {
        }

        #endregion

        #region Implementation of IConfigurationSectionHandler methods

        /// <summary>
        /// Returns the XML node 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="configContext"></param>
        /// <param name="section"></param>
        /// <returns></returns>
        public object Create(object parent, object configContext, System.Xml.XmlNode section)
        {
            return section;
        }

        #endregion
    }
}
