namespace QuickStart.Controllers
{
    public class AlternateViewLocationController
    {
        public AlternateViewLocationViewModel SayHelloAgain()
        {
            return new AlternateViewLocationViewModel
            {
                Message = "Hello again Webinar!",
            };
        }
    }

    public class AlternateViewLocationViewModel
    {
        public string Message { get; set; }
    }
}