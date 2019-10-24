using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OPCAIC.ApiService.Controllers;
using OPCAIC.ApiService.Models;
using OPCAIC.ApiService.Models.Users;
using OPCAIC.ApiService.ModelValidationHandling;
using OPCAIC.ApiService.Security;
using OPCAIC.ApiService.Services;
using OPCAIC.ApiService.Users.Commands;
using OPCAIC.Application.Dtos.Users;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Exceptions;
using OPCAIC.Application.Infrastructure;
using OPCAIC.Application.Infrastructure.Validation;
using OPCAIC.Application.Interfaces;
using OPCAIC.Application.Users.Queries;
using OPCAIC.Domain.Entities;
using OPCAIC.Domain.Enumerations;
using OPCAIC.Domain.Enums;
using Xunit;
using Xunit.Abstractions;

namespace OPCAIC.ApiService.Test.Controllers
{
	public class UsersControllerTest : ControllerTestBase<UsersController>
	{
		public UsersControllerTest(ITestOutputHelper output) : base(output)
		{
			Services.AddMediatR(typeof(GetUsersQuery).Assembly, typeof(CreateUserCommand).Assembly);
			FrontendUrlGeneratorMock = Services.Mock<IFrontendUrlGenerator>();
			EmailServiceMock = Services.Mock<IEmailService>();
			TurnOffAuthorization();
		}

		private readonly CreateUserCommand userModel = new CreateUserCommand
		{
			Email = "a@a.com",
			Password = "11afiejofa#XFAEFF@#23fafw",
			Username = "user",
			Organization = "opcaic",
			LocalizationLanguage = LocalizationLanguage.EN
		};

		private Mock<IFrontendUrlGenerator> FrontendUrlGeneratorMock { get; }

		public Mock<IEmailService> EmailServiceMock { get; }

		private async Task<User> DoCreateUser(CreateUserCommand model, bool confirmEmail = false)
		{
			var userManager = GetService<UserManager>();
			var user = new User
			{
				Email = model.Email,
				UserName = model.Username,
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
				new UserCredentialsModel {Email = email, Password = password},
				CancellationToken);

			Assert.Equal(email, identity.Email);
			Assert.NotNull(identity.AccessToken);
			Assert.NotNull(identity.RefreshToken);

			return identity;
		}

		[Fact]
		public async Task GetUsersAsync_Sucess()
		{
			var user = await DoCreateUser(userModel, true);

			var list = await Controller.GetUsersAsync(new GetUsersQuery 
			{
				Count = 5,
				RequestingUserRole = UserRole.Admin
			}, CancellationToken);

			Assert.Equal(1, list.Total);
			var preview = Assert.Single(list.List);
			Assert.Equal(user.UserName, preview.Username);
		}

		[Fact]
		public async Task UpdateUser_Success()
		{
			var user = await DoCreateUser(userModel, true);
			await Controller.UpdateAsync(
				new UpdateUserCommand
				{
					Id = user.Id, 
					LocalizationLanguage = LocalizationLanguage.CZ,
					Organization = "org",
					Role = user.Role,
					RequestingUserRole = UserRole.Admin,
				},
				CancellationToken);
			var model = await Controller.GetUserByIdAsync(user.Id, CancellationToken);
			Assert.NotEqual(userModel.LocalizationLanguage, user.LocalizationLanguage);
			Assert.NotEqual(userModel.Organization, user.Organization);
			Assert.Equal(LocalizationLanguage.CZ, user.LocalizationLanguage);
			Assert.Equal("org", user.Organization);
		}

		[Fact]
		public async Task ConfirmEmail_BadToken()
		{
			var user = await DoCreateUser(userModel);
			Assert.False(user.EmailConfirmed);

			await AssertInvalidModel(
				ValidationErrorCodes.InvalidEmailVerificationToken,
				null,
				() => Controller.GetEmailVerificationAsync(new EmailVerificationModel() { Email = user.Email, Token = "random token"}, 
					CancellationToken));

			var detail = await Controller.GetUserByIdAsync(user.Id, CancellationToken);
			Assert.False(detail.EmailVerified);
		}

		[Fact]
		public async Task ConfirmEmail_Success()
		{
			// capture email confirm token
			string token = null;
			FrontendUrlGeneratorMock
				.Setup(g => g.EmailConfirmLink(It.IsAny<string>(), It.IsAny<string>()))
				.Callback((string email, string resetToken) => { token = resetToken; });

			var user = await CreateUser_Success();
			Assert.False(user.EmailVerified);
			Assert.NotNull(token);

			var result =
				await Controller.GetEmailVerificationAsync(new EmailVerificationModel() { Email = user.Email, Token = token }, CancellationToken);
			Assert.IsAssignableFrom<NoContentResult>(result);

			var detail = await Controller.GetUserByIdAsync(user.Id, CancellationToken);
			Assert.True(detail.EmailVerified);
		}

		[Fact]
		public async Task ConfirmEmail_UserNotFound()
		{
			Assert.IsAssignableFrom<BadRequestResult>(
				await Controller.GetEmailVerificationAsync(new EmailVerificationModel() { Email = "not email", Token = "random token" }, CancellationToken));
		}

		[Fact]
		public async Task CreateUser_BadPassword()
		{
			// only short password needs to be tested, rest is the same
			userModel.Password = "1";
			await AssertInvalidModel(
				ValidationErrorCodes.PasswordTooShort,
				nameof(userModel.Password),
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task CreateUser_ConflictedEmail()
		{
			// prepare conflicting user
			await DoCreateUser(userModel);

			userModel.Username = "secondUser";
			await AssertInvalidModel(
				ValidationErrorCodes.UserEmailConflict,
				nameof(userModel.Email),
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task CreateUser_ConflictedUserName()
		{
			// prepare conflicting user
			await DoCreateUser(userModel);

			userModel.Email = "secondUser@a.com";
			await AssertInvalidModel(
				ValidationErrorCodes.UserUsernameConflict,
				nameof(userModel.Username),
				() => Controller.PostAsync(userModel, CancellationToken));
		}

		[Fact]
		public async Task<UserDetailDto> CreateUser_Success()
		{
			var result = await Controller.PostAsync(userModel, CancellationToken);
			var idModel =
				Assert.IsAssignableFrom<IdModel>(Assert
					.IsAssignableFrom<CreatedAtRouteResult>(result).Value);

			EmailServiceMock.Verify(
				s => s.EnqueueEmailAsync(
					It.IsAny<EmailType.UserVerificationType.Email>(), userModel.Email, CancellationToken));

			var detail = await Controller.GetUserByIdAsync(idModel.Id, CancellationToken);
			Assert.Equal(userModel.Email, detail.Email);
			Assert.Equal(userModel.LocalizationLanguage, detail.LocalizationLanguage);
			Assert.Equal(userModel.Organization, detail.Organization);
			Assert.Equal(userModel.Username, detail.Username);

			return detail;
		}

		[Fact]
		public Task ForgotPassword_UnknownUser()
		{
			// should still produce OK result
			return Controller.PostForgotPasswordAsync(
				new ForgotPasswordModel {Email = "my@mail.com"},
				CancellationToken);
		}

		[Fact]
		public async Task Login_BadPassword()
		{
			await DoCreateUser(userModel, true);

			await AssertUnauthorized(ValidationErrorCodes.LoginInvalid,
				() => DoLogin("aaaaaaa@a.com", "wrong pass"));
		}

		[Fact]
		public async Task Login_EmailNotConfirmed()
		{
			await DoCreateUser(userModel);

			await AssertUnauthorized(ValidationErrorCodes.LoginEmailNotConfirmed,
				() => DoLogin(userModel.Email, userModel.Password));
		}

		[Fact]
		public async Task<UserIdentityModel> Login_Success()
		{
			await DoCreateUser(userModel, true);
			return await DoLogin(userModel.Email, userModel.Password);
		}

		[Fact]
		public async Task Login_UnknownUser()
		{
			await DoCreateUser(userModel, true);

			await AssertUnauthorized(ValidationErrorCodes.LoginInvalid,
				() => DoLogin("aaaaaaa@a.com", "wrong pass"));
		}

		[Fact]
		public async Task PasswordChange_Mismatch()
		{
			var user = await DoCreateUser(userModel);
			HttpContext.User = await GetService<SignInManager>().CreateUserPrincipalAsync(user);

			await AssertInvalidModel(
				ValidationErrorCodes.PasswordMismatch,
				nameof(NewPasswordModel.OldPassword),
				() => Controller.PostPasswordAsync(
					new NewPasswordModel
					{
						NewPassword = "abfaiwef23r203r92r23rXX#@$",
						OldPassword = "Not old password"
					}, CancellationToken));
		}

		[Fact]
		public async Task PasswordChange_Fail()
		{
			var user = await DoCreateUser(userModel, true);
			HttpContext.User = await GetService<SignInManager>().CreateUserPrincipalAsync(user);

			await AssertInvalidModel(
				ValidationErrorCodes.PasswordTooShort,
				nameof(NewPasswordModel.NewPassword),
				() => Controller.PostPasswordAsync(
					new NewPasswordModel
					{
						NewPassword = "a", // too short
						OldPassword = userModel.Password
					}, CancellationToken));
		}

		[Fact]
		public async Task PasswordChange_Success()
		{
			const string NewPassword = "abfaiwef23r203r92r23rXX#@$";
			var user = await DoCreateUser(userModel, true);
			HttpContext.User = await GetService<SignInManager>().CreateUserPrincipalAsync(user);

			var newTokens = await Controller.PostPasswordAsync(
				new NewPasswordModel {NewPassword = NewPassword, OldPassword = userModel.Password},
				CancellationToken);

			// check that returned tokens are valid
			await Controller.RefreshAsync(user.Id,
				new RefreshToken {Token = newTokens.RefreshToken}, CancellationToken);

			// check that login is possible
			await DoLogin(user.Email, NewPassword);
		}

		[Fact]
		public async Task PasswordReset_BadPassword()
		{
			const string NewPassword = "1";
			var user = await DoCreateUser(userModel, true);

			var token = await GetService<UserManager>().GeneratePasswordResetTokenAsync(user);

			await AssertInvalidModel(
				ValidationErrorCodes.PasswordTooShort,
				nameof(userModel.Password),
				() => Controller.PostPasswordResetAsync(
					new PasswordResetModel
					{
						Email = user.Email, Password = NewPassword, ResetToken = token
					}, CancellationToken));
		}

		[Fact]
		public async Task PasswordReset_Success()
		{
			const string NewPassword = "6789o#$#kjnhbgt6y7u8i9";
			var user = await DoCreateUser(userModel, true);

			// capture password reset token
			string token = null;
			FrontendUrlGeneratorMock.Setup(g => g.PasswordResetLink(user.Email, It.IsAny<string>()))
				.Callback((string email, string resetToken) => { token = resetToken; });

			await Controller.PostForgotPasswordAsync(
				new ForgotPasswordModel {Email = user.Email}, CancellationToken);
			Assert.NotNull(token);

			// actual reset
			await Controller.PostPasswordResetAsync(
				new PasswordResetModel
				{
					Email = user.Email, Password = NewPassword, ResetToken = token
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
					Email = "my@mail.com", Password = "Ignored", ResetToken = "Token"
				}, CancellationToken);
		}

		[Fact]
		public async Task RefreshToken_Fail()
		{
			var identity = await Login_Success();

			await AssertUnauthorized(ValidationErrorCodes.RefreshTokenInvalid,
				() => Controller.RefreshAsync(identity.Id, new RefreshToken {Token = "eafef"},
					CancellationToken));

			await Assert.ThrowsAsync<NotFoundException>(() => Controller.RefreshAsync(
				-1, new RefreshToken {Token = "eafef"}, CancellationToken));
		}

		[Fact]
		public async Task RefreshToken_Success()
		{
			var identity = await Login_Success();

			var tokens = await Controller.RefreshAsync(identity.Id,
				new RefreshToken {Token = identity.RefreshToken}, CancellationToken);
		}
	}
}