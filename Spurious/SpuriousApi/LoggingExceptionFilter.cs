using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace SpuriousApi
{
    public class LoggingExceptionFilter : ExceptionFilterAttribute
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            logger.Error(actionExecutedContext.Exception, "Service error");
        }
    }
}
