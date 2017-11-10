﻿using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace WebApplicationIdentityServer2
{
    public class Config
    {
        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "d860efca-22d9-47fd-8249-791ba61b07c7",
                    Username = "Frank",
                    Password = "password",
                   

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Frank"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address","1, Main Road"),
                        new Claim("role","FreeUser"),
                        new Claim("subscriptionlevel","FreeUser"),
                        new Claim("country","nl")
                    }
                },
                new TestUser
                {
                    SubjectId = "b7539694-97e7-4dfe-84da-b4256e1ff5c7",
                    Username = "Claire",
                    Password = "password",

                    Claims = new List<Claim>
                    {
                        new Claim("given_name", "Claire"),
                        new Claim("family_name", "Underwood"),
                        new Claim("address","2, Big Street"),
                        new Claim("role","PayinUser"),
                        new Claim("subscriptionlevel","PayinUser"),
                        new Claim("country","be")
                    }
                }
            };
        }


        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Address(),
                new IdentityResource("roles","Your role(s)",new List<string>(){ "role" }),
                new IdentityResource("country","The Country you're living in",
                new List<string>{ "country"}),
                new IdentityResource("subscriptionlevel","You Subscription Level",
                new List<string>{ "subscriptionlevel" })
            };
        }

        public static IEnumerable<ApiResource> GetApiResource()
        {
            return new List<ApiResource>
            {
                new ApiResource("imagegalleryapi","Image Gallery API",
                new List<string>{ "role"})
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>() {
                new Client {
                     ClientName = "Image Gallery",
                     ClientId = "imagegalleryclient",
                     AllowedGrantTypes = GrantTypes.Hybrid,
                     //identitytokenlifetime = 300,
                     //authorizationcodelifetime = 300,
                     AccessTokenLifetime = 120,
                    RedirectUris = new List<string>(){
                         "https://localhost:44358/signin-oidc"
                     },
                     RequireConsent = false,
                     AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "imagegalleryapi",
                        "country",
                        "subscriptionlevel"
                    },
                     ClientSecrets =
                    {
                        new Secret("password".Sha256())

                    },
                     PostLogoutRedirectUris =
                    {
                        "https://localhost:44358/signout-callback-oidc"
                    }
                   //AlwaysIncludeUserClaimsInIdToken = true
                }
            };
        }
    }
}
