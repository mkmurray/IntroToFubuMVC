using System;
using FubuMVC.Core;
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
                .ConstrainToHttpMethod(x => x.Method.Name.StartsWith("Edit"), "POST")
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
                x.Displays.IfPropertyIs<DateTime>().BuildBy(
                    request => new HtmlTag("span").Text(request.Value<DateTime>().ToShortDateString()));

                x.Editors.IfPropertyIs<Colors>().Modify(
                    (request, tag) => tag.Style("color", request.StringValue()));
                x.Displays.IfPropertyIs<Colors>().Modify(
                    (request, tag) => tag.Style("color", request.StringValue()));
            });
        }
    }
}
