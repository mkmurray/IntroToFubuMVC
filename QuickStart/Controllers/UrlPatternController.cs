using FubuCore;
using FubuMVC.Core;
using HtmlTags;

namespace QuickStart.Controllers
{
    public class UrlPatternController
    {
        public HtmlDocument Routes_Summary()
        {
            //summary of changes since last branch
            var document = new HtmlDocument {Title = "Overview"};
            document.Add("p").Text("Moved controllers to new folder and namespace.");
            document.Add("p").Text("Added .IgnoreControllerNamespaceEntirely()");
            document.Add("p").Text("Added global.asax, calling AppStartFubuMVC.Start() in Application_Start");
            document.Add("p").Text("Removed web activator attribute from FubuMVC.cs");
            document.Add("p").Text("Removed getting started dll from fubu-content");
            document.Add("p").Text("Added .HomeIs<UrlPatternController>(x => x.Routes_Summary()) to ConfigureFubuMVC.cs");
            document.Add("p").Text("Demonstration of UrlPattern attribute and various usages in UrlPatternController.cs");
            document.Add("p").Text("Custom UrlPolicy in AllStringOutputRoutesAreSpecialPolicy.cs");
            return document;
        }

        [UrlPattern("exact/routes")]
        public string TellRouteExactlyWhatToBe()
        {
            return "using the UrlPattern attribute";
        }

        [UrlPattern("mix/{FirstColor}/with/{SecondColor}")]
        public HtmlDocument ActionsThatTakeAnInputModel(MixColorInputModel input)
        {
            //can specify incoming route values to match model properties
            var document = new HtmlDocument
            {
                Title = "Mix Colors"
            };
            document.Add("p").Text("Mixing {0} with {1}".ToFormat(input.FirstColor, input.SecondColor));
            return document;
        }

        [UrlPattern("specify/a/{DefaultValue:likeThis}")]
        public string DefaultValuesForUrlPatterns(DefaultInputModel input)
        {
            //when nothing is specified, input.DefaultValue will == "likeThis"
            return "Using value passed in or default value of \"likeThis\". Value = " + input.DefaultValue;
        }
    }

    public class MixColorInputModel
    {
        public string FirstColor { get; set; }
        public string SecondColor { get; set; }
    }

    public class DefaultInputModel
    {
        public string DefaultValue { get; set; }
    }
}