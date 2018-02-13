using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Microsoft.eShopOnContainers.Services.AI.ProductSearchImageBased.CNTK.API
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);

            AddBinFolderToPATH();
        }

        /// <summary>
        /// In order to CNTK to run properly, 
        /// we need to add the bin folder to the PATH environment variable
        /// </summary>
        private static void AddBinFolderToPATH()
        {
            string pathValue = Environment.GetEnvironmentVariable("PATH");
            string cntkPath = AppDomain.CurrentDomain.BaseDirectory + @"bin\";
            if (!pathValue.Contains(cntkPath))
            {
                pathValue += ";" + cntkPath;
                Environment.SetEnvironmentVariable("PATH", pathValue);
            }
        }
    }
}
