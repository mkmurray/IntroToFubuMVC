using FubuCore;
using FubuMVC.Core.Diagnostics;
using FubuMVC.Core.Registration.Conventions;
using FubuMVC.Core.Registration.Nodes;
using FubuMVC.Core.Registration.Routes;

namespace QuickStart
{
    public class AllStringOutputRoutesAreSpecialPolicy : IUrlPolicy
    {
        readonly IUrlPolicy _innerUrlPolicy;

        public AllStringOutputRoutesAreSpecialPolicy()
            : this(new UrlPolicy(x => true, new RouteInputPolicy()))
        {
        }

        public AllStringOutputRoutesAreSpecialPolicy(IUrlPolicy innerUrlPolicy)
        {
            _innerUrlPolicy = innerUrlPolicy;
        }

        public bool Matches(ActionCall call, IConfigurationObserver log)
        {
            if (log.IsRecording)
            {
                log.RecordCallStatus(call, "This route will have /special in front of it");
            }

            //Use FubuCore.TypeExtensions to aid the readability of your conventions
            //by using .CanBeCastTo<>() and other helper methods to match against types
            return call.HasOutput && call.OutputType().CanBeCastTo<string>();
        }

        public IRouteDefinition Build(ActionCall call)
        {
            var routeDefinition = _innerUrlPolicy.Build(call);
            routeDefinition.Prepend("special");
            return routeDefinition;
        }
    }
}