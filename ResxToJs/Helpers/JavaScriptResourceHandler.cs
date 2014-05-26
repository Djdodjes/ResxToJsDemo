using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Web;
using System.Web.Script.Serialization;
using System.Linq;

namespace ResxToJs.Helpers
{
    public class JavaScriptResourceHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var requestedCulture = new CultureInfo(context.Request.QueryString["locale"]);
            var classKey = context.Request.QueryString["classKey"];

            var dictionary = ReadResources(classKey, requestedCulture);

            var javaScriptSerializer = new JavaScriptSerializer();
            var script = @"if (typeof(Resources) == ""undefined"") Resources = {};
                            Resources." + classKey + " = " +
                            javaScriptSerializer.Serialize(dictionary) + ";";

            context.Response.ContentType = "application/javascript";

            context.Response.Expires = 43200; // 30 days
            context.Response.Cache.SetLastModified(DateTime.UtcNow);

            context.Response.Write(script);
        }

        private static Dictionary<object, object> ReadResources(string classKey, CultureInfo requestedCulture)
        {
            //var resourceManager = new ResourceManager("Resources." + classKey, Assembly.Load("App_GlobalResources"));
            var resourceManager = new ResourceManager("ResxToJs.Resources.Views." + classKey, Assembly.GetExecutingAssembly());
            using (var resourceSet =
                resourceManager.GetResourceSet(CultureInfo.InvariantCulture, true, true))
            {

                return resourceSet
                    .Cast<DictionaryEntry>()
                    .ToDictionary(x => x.Key,
                         x => resourceManager.GetObject((string)x.Key, requestedCulture));
            }

        }
        #endregion
    }
}
