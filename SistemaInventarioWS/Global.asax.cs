using System;
using System.Web;

namespace SistemaInventarioWS
{
    public class Global : HttpApplication
    {

        protected void Application_Start(object sender, EventArgs e)
        {

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (HttpContext.Current.Request.HttpMethod == "OPTIONS")
            {
                HttpContext.Current.Response.StatusCode = 200;
                HttpContext.Current.Response.End();
                return;
            }

            var rawUrl = HttpContext.Current.Request.RawUrl.Split('?')[0].TrimEnd('/');
            if (rawUrl == "" || rawUrl == "/")
            {
                HttpContext.Current.Response.Redirect("~/HelpPage/index.ashx", false);
                HttpContext.Current.ApplicationInstance.CompleteRequest();
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}