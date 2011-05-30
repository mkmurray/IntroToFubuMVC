using System;
using System.Web;
using System.Web.Routing;
using FubuMVC.Core;
using FubuMVC.StructureMap;

namespace IntroToFubu
{
    public class Global : HttpApplication
    {
        protected void Application_Start(object sender, EventArgs e)
        {
            FubuApplication.For<IntroToFubuRegistry>()
                .StructureMapObjectFactory()
                .Bootstrap(RouteTable.Routes);
        }
    }
}