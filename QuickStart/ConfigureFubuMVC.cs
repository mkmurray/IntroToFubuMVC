using System;
using FubuMVC.Core;
using QuickStart.Controllers;

namespace QuickStart
{
    public class ConfigureFubuMVC : FubuRegistry
    {
        public ConfigureFubuMVC()
        {
            // This line turns on the basic diagnostics and request tracing
            IncludeDiagnostics(true);

            // All public methods from concrete classes ending in "Controller"
            // in this assembly are assumed to be action methods
            Actions.IncludeClassesSuffixedWithController();

            // Policies
            Routes
                .IgnoreControllerNamesEntirely()
                .IgnoreControllerNamespaceEntirely()
                .IgnoreMethodSuffix("Html")
                .HomeIs<UrlPatternController>(x => x.Routes_Summary())
                .UrlPolicy<AllStringOutputRoutesAreSpecialPolicy>()
                .RootAtAssemblyNamespace();

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();
        }
    }

}