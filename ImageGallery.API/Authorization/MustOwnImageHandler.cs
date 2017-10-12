using ImageGallery.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ImageGallery.API.Authorization
{
    public class MustOwnImageHandler : AuthorizationHandler<MustOwnImageRequirement>
    {
        private readonly IGalleryRepository _galleryRepository;

        public MustOwnImageHandler(IGalleryRepository galleryRepository)
        {
            this._galleryRepository = galleryRepository;
        }

        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, MustOwnImageRequirement requirement)
        {
            var filterContext = context.Resource as AuthorizationFilterContext;
            if (filterContext == null)
            {
                context.Fail();
                return Task.FromResult(0);
            }

            var imageid = filterContext.RouteData.Values["id"].ToString();

            //try to parse it to a Guid
            Guid imageIdAsGuid;
            if (!Guid.TryParse(imageid, out imageIdAsGuid))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            //get the sub claim
            var ownerId = context.User.Claims.FirstOrDefault(c => c.Type == "sub").Value;

            if (!_galleryRepository.IsImageOwner(imageIdAsGuid,ownerId))
            {
                context.Fail();
                return Task.FromResult(0);
            }

            context.Succeed(requirement);
            return Task.FromResult(0);
        }
    }
}
