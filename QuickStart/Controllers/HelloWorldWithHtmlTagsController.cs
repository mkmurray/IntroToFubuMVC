using HtmlTags;

namespace QuickStart.Controllers
{
    public class HelloWorldWithHtmlTagsController
    {
        public HtmlDocument BlueHello()
        {
            var document = new HtmlDocument
            {
                Title = "Saying hello to you"
            };

            document
                .Add("h1")
                .Text("Hello world!")
                .Style("color", "blue");

            return document;
        }
    }
}