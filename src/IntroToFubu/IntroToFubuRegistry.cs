using FubuMVC.Core;
using FubuMVC.Spark;
using IntroToFubu.Controllers.Demo;
using IntroToFubu.Models.Input;

namespace IntroToFubu
{
    public class IntroToFubuRegistry : FubuRegistry
    {
        public IntroToFubuRegistry()
        {
            IncludeDiagnostics(true);

            Applies.
                ToThisAssembly();

            Actions.
                IncludeClassesSuffixedWithController();

            Routes
                .HomeIs<DemoController>(x => x.Home())
                .IgnoreControllerNamespaceEntirely();

            this.UseSpark();

            Views
                .TryToAttachWithDefaultConventions();
        }
    }
}