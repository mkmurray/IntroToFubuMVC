using System;
using QuickStart.App_Start;

namespace QuickStart
{
    public class Global : System.Web.HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            AppStartFubuMVC.Start();
        }
    }
}