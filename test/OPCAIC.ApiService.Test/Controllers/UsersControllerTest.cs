using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Moq;
using OPCAIC.ApiService.Controllers;
using OPCAIC.ApiService.Exceptions;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.Infrastructure.Emails;
using OPCAIC.Infrastructure.Entities;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Controllers
{
	public class UsersControllerTest : ControllerTestBase<UsersController>
	{
		public UsersControllerTest(ITestOutputHelper output) : base(output)
		{
			FrontendUrlGeneratorMock = Services.Mock<IFrontendUrlGenerator>();
		}

		private readonly NewUserModel userModel = new NewUserModel
		{
			Email = "a@a.com",
			Password = "11afiejofa#XFAEFF@#23fafw",
			UserName = "user",
			Organization = "opcaic",
			LocalizationLanguage = "en"
		};

		private Mock<IFrontendUrlGenerator> FrontendUrlGeneratorMock { get; }

		private async Task AssertUnauthorizedCode(string errorCode, Func<Task> testCode)
		{
			var ex = await Assert.ThrowsAsync<UnauthorizedException>(testCode);
			Assert.Equal(errorCode, ex.Code);
		}

		private async Task AssertInvalidModel(int statusCode, string errorCode, Func<Task> testCode)
		{
			var ex = await Assert.ThrowsAsync<ModelValidationException>(testCode);
			Assert.Equal(statusCode, ex.StatusCode);

			var error = Assert.Single(ex.ValidationErrors);
			Assert.Equal(errorCode, error.Code);
		}

		private async Task<User> DoCreateUser(NewUserModel model, bool confirmEmail = false)
		{
			var userManager = GetService<UserManager>();
			var user = new User
			{
				Email = model.Email,
				UserName = model.UserName,
				LocalizationLanguage = model.LocalizationLanguage,
				Organization = model.Organization,
				EmailConfirmed = confirmEmail
			};
			
			await userManager.CreateAsync(user, model.Password);
			return user;
		}

		private async Task<UserIdentityModel> DoLogin(string email, string password)
		{
			var identity = await Controller.LoginAsync(
				new UserCredentialsModel { Email = email, Password = password, RememberMe = false },
				CancellationToken);

			Assert.Equal(email, identity.Email);
			Assert.NotNull(identity.AccessToken);
			Assert.NotNull(identity.RefreshToken);

			return identity;
		}

		[Fact]
		public async Task ConfirmEmail_BadToken()
		{
			var user = await DoCreateUser(userModel);
			Assert.False(user.EmailConfirmed);

			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.InvalidEmailVerificationToken,
				() => Controller.GetEmailVerificationAsync(user.Id, "randomToken", CancellationToken));

			var detail = await Controller.GetUserByIdAsync(user.Id, CancellationToken);
			Assert.False(detail.EmailVerified);
		}

		[Fact]
		public async Task ConfirmEmail_Success()
		{
			// capture email confirm token
			string token = null;
			FrontendUrlGeneratorMock.Setup(g => g.EmailConfirmLink(It.IsAny<long>(), It.IsAny<string>()))
				.Callback((long id, string resetToken) => { token = resetToken; });

			var user = await CreateUser_Success();
			Assert.False(user.EmailVerified);
			Assert.NotNull(token);

			var result =
				await Controller.GetEmailVerificationAsync(user.Id, token, CancellationToken);
			Assert.IsAssignableFrom<NoContentResult>(result);

			var detail = await Controller.GetUserByIdAsync(user.Id, CancellationToken);
			Assert.True(detail.EmailVerified);
		}

		[Fact]
		public async Task ConfirmEmail_UserNotFound()
		{
			Assert.IsAssignableFrom<BadRequestResult>(
				await Controller.GetEmailVerificationAsync(-1, "randomToken", CancellationToken));
		}

		[Fact]
		public async Task CreateUser_BadPassword()
		{
			// only short password needs to be tested, rest is the same
			userModel.Password = "1";
			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.PasswordTooShort,
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task CreateUser_ConflictedEmail()
		{
			// prepare conflicting user
			await DoCreateUser(userModel);

			userModel.UserName = "secondUser";
			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.UserEmailConflict,
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task CreateUser_ConflictedUserName()
		{
			// prepare conflicting user
			await DoCreateUser(userModel);

			userModel.Email = "secondUser@a.com";
			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.UserUsernameConflict,
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task<UserDetailModel> CreateUser_Success()
		{
			var result = await Controller.PostAsync(userModel, CancellationToken);
			var idModel =
				Assert.IsAssignableFrom<IdModel>(Assert
					.IsAssignableFrom<CreatedAtRouteResult>(result).Value);

			EmailServiceMock.Verify(
				s => s.SendEmailVerificationEmailAsync(
					idModel.Id, It.IsAny<string>(), CancellationToken));

			var detail = await Controller.GetUserByIdAsync(idModel.Id, CancellationToken);
			Assert.Equal(userModel.Email, detail.Email);
			Assert.Equal(userModel.LocalizationLanguage, detail.LocalizationLanguage);
			Assert.Equal(userModel.Organization, detail.Organization);
			Assert.Equal(userModel.UserName, detail.Username);

			return detail;
		}

		[Fact]
		public async Task Login_BadPassword()
		{
			await DoCreateUser(userModel, true);

			await AssertUnauthorizedCode(ValidationErrorCodes.LoginInvalid,
				() => DoLogin(userModel.Email, "wrong pass"));
		}

		[Fact]
		public async Task Login_UnknownUser()
		{
			await DoCreateUser(userModel, true);

			await AssertUnauthorizedCode(ValidationErrorCodes.LoginInvalid,
				() => DoLogin("aaaaaaa@a.com", "wrong pass"));
		}

		[Fact]
		public async Task Login_EmailNotConfirmed()
		{
			await DoCreateUser(userModel);

			await AssertUnauthorizedCode(ValidationErrorCodes.LoginEmailNotConfirmed,
				() => DoLogin(userModel.Email, userModel.Password));
		}

		[Fact]
		public async Task<UserIdentityModel> Login_Success()
		{
			await DoCreateUser(userModel, true);
			return await DoLogin(userModel.Email, userModel.Password);
		}

		[Fact]
		public async Task PasswordChange_Mismatch()
		{
			var user = await DoCreateUser(userModel);

			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.PasswordMismatch,
				() => Controller.PostPasswordAsync(
					new NewPasswordModel
					{
						Email = user.Email,
						NewPassword = "abfaiwef23r203r92r23rXX#@$",
						OldPassword = "Not old password"
					}, CancellationToken));
		}

		[Fact]
		public async Task PasswordChange_Success()
		{
			const string NewPassword = "abfaiwef23r203r92r23rXX#@$";
			var user = await DoCreateUser(userModel, true);

			Assert.IsAssignableFrom<OkResult>(await Controller.PostPasswordAsync(
				new NewPasswordModel
				{
					Email = user.Email,
					NewPassword = NewPassword,
					OldPassword = userModel.Password
				}, CancellationToken));

			await DoLogin(user.Email, NewPassword);
		}

		[Fact]
		public async Task PasswordChange_UnknownUser()
		{
			Assert.IsAssignableFrom<BadRequestResult>(await Controller.PostPasswordAsync(
				new NewPasswordModel
				{
					Email = "a@a.com",
					NewPassword = "abfaiwef23r203r92r23rXX#@$",
					OldPassword = userModel.Password
				}, CancellationToken));
		}

		[Fact]
		public Task ForgotPassword_UnknownUser()
		{
			return Controller.PostForgotPasswordAsync(new ForgotPasswordModel { Email = "my@mail.com" },
				CancellationToken);
		}

		[Fact]
		public async Task PasswordReset_Success()
		{
			const string NewPassword = "6789o#$#kjnhbgt6y7u8i9";
			var user = await DoCreateUser(userModel, true);

			// capture password reset token
			string token = null;
			FrontendUrlGeneratorMock.Setup(g => g.PasswordResetLink(user.Id, It.IsAny<string>()))
				.Callback((long id, string resetToken) => { token = resetToken; });

			await Controller.PostForgotPasswordAsync(
				new ForgotPasswordModel { Email = user.Email }, CancellationToken);
			Assert.NotNull(token);

			// actual reset
			await Controller.PostPasswordResetAsync(
				new PasswordResetModel
				{
					Email = user.Email,
					Password = NewPassword,
					ResetToken = token
				}, CancellationToken);

			// try to login
			await DoLogin(user.Email, NewPassword);
		}

		[Fact]
		public async Task PasswordReset_UnknownUser()
		{
			await Controller.PostPasswordResetAsync(
				new PasswordResetModel
				{
					Email = "my@mail.com",
					Password = "Ignored",
					ResetToken = "Token"
				}, CancellationToken);
		}

		[Fact]
		public async Task PasswordReset_BadPassword()
		{
			const string NewPassword = "1";
			var user = await DoCreateUser(userModel, true);

			var token = await GetService<UserManager>().GeneratePasswordResetTokenAsync(user);

			await AssertInvalidModel(StatusCodes.Status400BadRequest, ValidationErrorCodes.PasswordTooShort,
				() => Controller.PostPasswordResetAsync(
				new PasswordResetModel
				{
					Email = user.Email,
					Password = NewPassword,
					ResetToken = token
				}, CancellationToken));
		}

		[Fact]
		public async Task RefreshToken_Fail()
		{
			var identity = await Login_Success();

			await AssertUnauthorizedCode(ValidationErrorCodes.RefreshTokenInvalid,
			() => Controller.RefreshAsync(identity.Id, new RefreshToken { Token = "eafef" }, CancellationToken));

			await Assert.ThrowsAsync<NotFoundException>(() => Controller.RefreshAsync(
				-1, new RefreshToken { Token = "eafef" }, CancellationToken));
		}

		[Fact]
		public async Task RefreshToken_Success()
		{
			var identity = await Login_Success();

			var tokens = await Controller.RefreshAsync(identity.Id,
				new RefreshToken { Token = identity.RefreshToken }, CancellationToken);
		}
	}
}