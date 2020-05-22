using KellermanSoftware.NetLoggingLibrary;
using KellermanSoftware.NetLoggingLibrary.Targets;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace Voice.Service.LoggerX
{
   internal class LoggerX
    {
        private static bool is_init = false;
        private static void initLogger()
        {
            if (is_init) { return; }
            var rootPath = HostingEnvironment.MapPath("~/ErrorLogs/");
            if(string.IsNullOrEmpty(rootPath))
            {
                is_init = false;
                return;
            }
            string _file_name = "errorLog.txt";
            var _errorFilePath = Path.Combine(rootPath, Path.GetFileName(_file_name));
            FileTarget errorTarget = new FileTarget(_errorFilePath);
            errorTarget.MinimumLevel = LoggingLevel.ERROR;
            Log.Config.Targets.Add(errorTarget);
           
            is_init = true;
        }
        
        public static void LogException(System.Exception ex)
        {
            initLogger();
            System.Diagnostics.Debug.WriteLine("<<ERROR>>");
            System.Diagnostics.Debug.WriteLine(ex.Message);
            if (is_init)
            {
                Log.LogException(ex);
            }
            
        }
    }
}
