namespace QuickStart.Controllers
{
    public class ViewLocationController
    {
        public HelloViewModel SayHelloWithSpark()
        {
            return new HelloViewModel
            {
                Message = "Hello Webinar!"
            };
        }
    }

    public class HelloViewModel
    {
        public string Message { get; set; }
    }
}