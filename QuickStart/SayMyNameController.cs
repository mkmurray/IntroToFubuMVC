using HtmlTags;

namespace QuickStart
{
    public class SayMyNameController
    {
        public HtmlDocument get_my_name_is_Name(NameModel input)
        {
            var document = new HtmlDocument();
            document.Title = "What's your name?";
            document.Add("h1").Text("My name is " + input.Name);
            return document;
        }
    }

    public class NameModel
    {
        public string Name { get; set; }
    }
}