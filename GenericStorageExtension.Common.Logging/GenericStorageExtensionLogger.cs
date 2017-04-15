using log4net;
using log4net.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenericStorageExtension.Common.Logging
{
    public enum ELogLevel
    {
        DEBUG = 1,
        ERROR,
        FATAL,
        INFO,
        WARN
    }
    public class GenericStorageExtensionLogger
    {
        #region Members
        private static readonly ILog Logger = LogManager.GetLogger(typeof(GenericStorageExtensionLogger));
        #endregion

        #region Constructors
        static GenericStorageExtensionLogger()
        {


            string LOGFILECONFIG = ConfigurationManager.AppSettings["LoggingConfigPath"];

            System.IO.FileInfo config = new System.IO.FileInfo(LOGFILECONFIG);
            XmlConfigurator.Configure(config);
        }
        #endregion

        #region Methods
        public static void WriteLog(ELogLevel logLevel, String log)
        {

            if (GenericStorageExtensionLogger.Logger.IsDebugEnabled && logLevel.Equals(ELogLevel.DEBUG))
            {

                Logger.Debug(log);

            }

            else if (GenericStorageExtensionLogger.Logger.IsErrorEnabled && logLevel.Equals(ELogLevel.ERROR))
            {

                Logger.Error(log);

            }

            else if (GenericStorageExtensionLogger.Logger.IsFatalEnabled && logLevel.Equals(ELogLevel.FATAL))
            {

                Logger.Fatal(log);

            }

            else if (GenericStorageExtensionLogger.Logger.IsInfoEnabled && logLevel.Equals(ELogLevel.INFO))
            {

                Logger.Info(log);

            }

            else if (logLevel.Equals(ELogLevel.WARN))
            {
                Logger.Warn(log);
            }
        }

        #endregion
    }
}
