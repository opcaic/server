using Microsoft.AspNetCore.Mvc;
using OPCAIC.ApiService.Controllers;

namespace OPCAIC.ApiService.Extensions
{
    public static class UrlHelperExtensions
    {
        public static string EmailConfirmationLink(this IUrlHelper urlHelper, long userId, string token, string scheme)
        {
            return urlHelper.Action(
                action: nameof(UsersController.GetEmailVerificationAsync),
                controller: "Users",
                values: (userId, token),
                protocol: scheme);
        }

        public static string ResetPasswordCallbackLink(this IUrlHelper urlHelper, long userId, string token, string scheme)
        {
            return urlHelper.Action(
                action: nameof(UsersController.PostForgotPasswordAsync),
                controller: "Users",
                values: (userId, token),
                protocol: scheme);
        }
    }
}