using LT.DigitalOffice.AuthentificationService.Broker.Requests;
using LT.DigitalOffice.AuthentificationService.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using System;

namespace LT.DigitalOffice.AuthentificationService.Business
{
    class ForgotPasswordCommand
    {
		private readonly IRequestClient<IUserEmailRequest> requestClientUS;
		private readonly IRequestClient<IUserDescriptionRequest> requestClientMS;

		public ForgotPasswordCommand(
			[FromServices] IRequestClient<IUserEmailRequest> requestClientUS,
			[FromServices] IRequestClient<IUserDescriptionRequest> requestClientMS)
		{
			this.requestClientUS = requestClientUS;
			this.requestClientMS = requestClientMS;
		}

		public void Execute(string userEmail)
		{
			var userDescription = GetChekingResultUserEmail(userEmail);

			SentRequestConfirmInMessageService(userDescription);
		}

		private IUserDescriptionResponse GetChekingResultUserEmail(string userEmail)
		{
			var brokerResponse = requestClientUS.GetResponse<IOperationResult<IUserDescriptionResponse>>(new
			{
				UserEmail = userEmail
			}).Result;

			if (!brokerResponse.Message.IsSuccess)
			{
				throw new Exception();
			}

			return brokerResponse.Message.Body;
		}

		private void SentRequestConfirmInMessageService(IUserDescriptionResponse userDescription)
		{
			var brokerResponse = requestClientMS.GetResponse<IOperationResult<IUserDescriptionRequest>>(new
			{
				userDescription.Id,
				userDescription.Email,
				userDescription.FirstName,
				userDescription.LastName,
				userDescription.MiddleName
			}).Result;

			if (!brokerResponse.Message.IsSuccess)
			{
				throw new Exception();
			}
		}
	}
}
