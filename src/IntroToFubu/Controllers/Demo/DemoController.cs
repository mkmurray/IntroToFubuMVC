using FubuMVC.Core;
using IntroToFubu.Models.Input;
using IntroToFubu.Models.View;

namespace IntroToFubu.Controllers.Demo
{
    public class DemoController
    {
        public HomeViewModel Home()
        {
            return new HomeViewModel();
        }

        [UrlPattern("hello/{Name}")]
        public HelloViewModel Hello(HelloInputModel input)
        {
            return new HelloViewModel { Name = input.Name };
        }
    }
}