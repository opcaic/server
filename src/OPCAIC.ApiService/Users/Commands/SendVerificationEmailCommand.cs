using System.Threading;
using System.Threading.Tasks;
using MediatR;
using OPCAIC.ApiService.Interfaces;
using OPCAIC.Application.Emails;
using OPCAIC.Application.Emails.Templates;
using OPCAIC.Application.Interfaces;
using OPCAIC.Domain.Entities;

namespace OPCAIC.ApiService.Users.Commands
{
	public class SendVerificationEmailCommand : IRequest
	{
		public SendVerificationEmailCommand(User user)
		{
			User = user;
		}

		public User User { get; }

		public class Handler : IRequestHandler<SendVerificationEmailCommand>
		{
			private readonly IEmailService emailService;
			private readonly IUserManager userManager;
			private readonly IFrontendUrlGenerator urlGenerator;

			/// <inheritdoc />
			public Handler(IEmailService emailService, IUserManager userManager, IFrontendUrlGenerator urlGenerator)
			{
				this.emailService = emailService;
				this.userManager = userManager;
				this.urlGenerator = urlGenerator;
			}

			/// <inheritdoc />
			public async Task<Unit> Handle(SendVerificationEmailCommand request, CancellationToken cancellationToken)
			{
				var token =
					await userManager.GenerateEmailConfirmationTokenAsync(request.User);
				var url = urlGenerator.EmailConfirmLink(request.User.Email, token);

				await emailService.EnqueueEmailAsync(EmailType.UserVerification.CreateEmail(url),
					request.User.Email, cancellationToken);

				return Unit.Value;
			}
		}
	}
}