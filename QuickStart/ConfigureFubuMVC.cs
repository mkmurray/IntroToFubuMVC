using System;
using FubuMVC.Core;
using FubuMVC.Core.UI.Configuration;
using FubuMVC.Spark;
using HtmlTags;
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
                .HomeIs<HtmlConventionsController>(x => x.BasicConventions())
                .UrlPolicy<AllStringOutputRoutesAreSpecialPolicy>()
                .RootAtAssemblyNamespace();

            this.UseSpark();

            ApplyConvention<VariableOutputConvention>();
            Policies.ConditionallyWrapBehaviorChainsWith<TransactionalBehavior>(
                x => x.Method.Name.StartsWith("Transactional"));

            // Match views to action methods by matching
            // on model type, view name, and namespace
            Views.TryToAttachWithDefaultConventions();

            HtmlConvention(x =>
            {
                x.Editors.IfPropertyIs<DateTime>().BuildBy(
                    request => new TextboxTag().Attr("value", request.Value<DateTime>().ToShortDateString()));

                x.Editors.IfPropertyIs<Colors>().Modify(
                    (request, tag) => tag.Style("color", request.StringValue()));
            });
        }
    }
}
