using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericStorageExtension.Common.Logging;
using GenericStorageExtension.Common.Services.DataContracts;

namespace GenericStorageExtension.Common.ExceptionManagement
{
    public class ExceptionHelper
    {
        private static void LogException(GenericStorageExtensionIndexingException ampException)
        {
            GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, ampException.Message + ", Code " + ampException.Code);
        }
        public static void HandleException(Exception exception)
        {
            GenericStorageExtensionIndexingException ampException = exception as GenericStorageExtensionIndexingException;
            if (ampException != null)
            {
                LogException(ampException);
            }
        }
        public static void HandleException(Exception exception, out GenericStorageExtensionServiceFault fault)
        {
            GenericStorageExtensionIndexingException ampException = exception as GenericStorageExtensionIndexingException;
            fault = new GenericStorageExtensionServiceFault();
            if (ampException != null)
            {
                fault.Code = ampException.Code;
                fault.Message = ampException.Message;
            }
            else
            {
                fault.Code = GenericStorageExtension.Common.Services.GenericStorageExtensionServiceConstants.ServiceFault.UNKNOWN_EXCEPTION_CODE;
                fault.Message = GenericStorageExtension.Common.Services.GenericStorageExtensionServiceConstants.ServiceFault.UNKNOWN_EXCEPTION_MESSAGE;
            }
        }

        public static void HandleCustomException(Exception ex, string LogMessage)
        {
            GenericStorageExtensionIndexingException ampEx = ex as GenericStorageExtensionIndexingException;
            if (ampEx != null)
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.WARN, LogMessage);
            }
            else
            {
                GenericStorageExtensionLogger.WriteLog(ELogLevel.ERROR, LogMessage);
            }
        }
    }
}
