using FubuMVC.Core;
using FubuMVC.Spark;
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
                .HomeIs<ViewLocationController>(x => x.SayHelloWithSpark())
                .UrlPolicy<AllStringOutputRoutesAreSpecialPolicy>()
                .RootAtAssemblyNamespace();

            this.UseSpark();

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();
        }
    }

}