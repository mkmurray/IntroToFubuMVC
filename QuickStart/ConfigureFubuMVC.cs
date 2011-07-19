using FubuMVC.Core;
using FubuMVC.Spark;
using QuickStart.Behaviors;
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
                .HomeIs<BehaviorController>(x => x.VariableOutput())
                .UrlPolicy<AllStringOutputRoutesAreSpecialPolicy>()
                .RootAtAssemblyNamespace();

            this.UseSpark();

            ApplyConvention<VariableOutputConvention>();
            Policies.ConditionallyWrapBehaviorChainsWith<TransactionalBehavior>(
                x => x.Method.Name.StartsWith("Transactional"));

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();
        }
    }
}
