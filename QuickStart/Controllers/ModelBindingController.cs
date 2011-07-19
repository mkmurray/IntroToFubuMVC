using System;
using System.Web;
using FubuMVC.Core.Continuations;

namespace QuickStart.Controllers
{
    public class ModelBindingController
    {
        public FubuContinuation Edit(InputModel input)
        {
            var viewModel = new ModelBindingViewModel
            {
                FirstName = input.FirstName,
                LastName = input.LastName,
                DateOfBirth = input.DateOfBirth,
                FavoriteColor = input.FavoriteColor
            };

            // fake saving input to database
            HttpContext.Current.Session[ModelBindingViewModel.SessionKey] = viewModel;
            return FubuContinuation.RedirectTo(viewModel);
        }

        public ModelBindingViewModel View(ModelBindingViewModel model)
        {
            // fake retrieval from database
            // POST, REDIRECT, GET = HTTP workflow; we aren't returning view from POST so that if the
            // user refreshes or hits back button, it doesn't show "repost data?" dialog popup.
            return (ModelBindingViewModel)HttpContext.Current.Session[ModelBindingViewModel.SessionKey];
        }
    }

    public class ModelBindingViewModel
    {
        public static string SessionKey = "ModelBindingViewModel";

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Colors FavoriteColor { get; set; }
    }

    public class InputModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime DateOfBirth { get; set; }
        public Colors FavoriteColor { get; set; }
    }
}