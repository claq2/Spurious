using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace SpuriousApi
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        protected void Application_Start()
        {
            GlobalConfiguration.Configure(WebApiConfig.Register);
            logger.Info("Starting app");
        }
    }
}
