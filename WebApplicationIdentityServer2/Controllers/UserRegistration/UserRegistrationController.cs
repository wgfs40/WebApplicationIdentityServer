using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebApplicationIdentityServer2.Services;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Http.Authentication;

namespace WebApplicationIdentityServer2.Controllers.UserRegistration
{
    public class UserRegistrationController : Controller
    {
        private readonly IMarvinUserRepository _marvinUserRepository;
        private readonly IIdentityServerInteractionService _interaction;

        public UserRegistrationController(IMarvinUserRepository marvinUserRepository, IIdentityServerInteractionService interaction)
        {
            this._marvinUserRepository = marvinUserRepository;
            this._interaction = interaction;
        }

        [HttpGet]
        public IActionResult RegisterUser(string returnUrl)
        {
            var vm = new RegisterUserViewModel() {
                 ReturnUrl = returnUrl
            };

            return View(vm);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterUser(RegisterUserViewModel model)
        {
            if (ModelState.IsValid)
            {
                //create + claims
                var userToCreate = new Entities.User();
                userToCreate.Password = model.Password;
                userToCreate.Username = model.Username;
                userToCreate.IsActive = true;
                userToCreate.Claims.Add(new Entities.UserClaim("country", model.Country));
                userToCreate.Claims.Add(new Entities.UserClaim("address", model.Address));
                userToCreate.Claims.Add(new Entities.UserClaim("given_name", model.Firstname));
                userToCreate.Claims.Add(new Entities.UserClaim("family_name", model.Lastname));
                userToCreate.Claims.Add(new Entities.UserClaim("email", model.Email));
                userToCreate.Claims.Add(new Entities.UserClaim("subscriptionlevel", "FreeUser"));

                // add it through the repository
                _marvinUserRepository.AddUser(userToCreate);

                if (!_marvinUserRepository.Save())
                {
                    throw new Exception($"Creating a user failed.");
                }

                //log the user in 
                await HttpContext.Authentication.SignInAsync(userToCreate.SubjectId,userToCreate.Username);

                //continue with the flow
                // make sure the returnUrl is still valid, and if yes - redirect back to authorize endpoint or a local page
                if (_interaction.IsValidReturnUrl(model.ReturnUrl) || Url.IsLocalUrl(model.ReturnUrl))
                {
                    return Redirect(model.ReturnUrl);
                }

                return Redirect("~/");
            }
            //modelStated invalid, return the view with the passed-in model
            //so changes can be made
            return View(model);
        }
    }
}
