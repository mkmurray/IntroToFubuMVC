using System;

namespace QuickStart.Controllers
{
    public class HtmlConventionsController
    {
        public BasicConventionsViewModel BasicConventions()
        {
            return new BasicConventionsViewModel
            {
                FirstName = "Bob",
                LastName = "Corey",
                DateOfBirth = new DateTime(1978, 1, 1),
                FavoriteColor = Colors.Red
            };
        }
    }

    public class BasicConventionsViewModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Colors FavoriteColor { get; set; }
    }

    public enum Colors
    {
        Red,
        Blue,
        Green,
        Yellow,
        Black
    }
}