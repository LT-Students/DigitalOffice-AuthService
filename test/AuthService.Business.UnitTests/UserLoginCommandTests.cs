using FluentValidation;
using FluentValidation.Results;
using LT.DigitalOffice.AuthService.Business.Commands;
using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Business.Helpers;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Models.Dto.Responses;
using LT.DigitalOffice.AuthService.Token.Interfaces;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Broker.Responses;
using LT.DigitalOffice.Kernel.Broker;
using LT.DigitalOffice.Kernel.Exceptions.Models;
using LT.DigitalOffice.UnitTestKernel;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthService.Business.UnitTests
{
    public class UserLoginCommandTests
    {
        #region private fields
        private Mock<ITokenEngine> _tokenEngineMock;
        private Mock<ILoginValidator> _loginValidatorMock;
        private Mock<ILogger<LoginCommand>> _loggerMock;
        private Mock<IRequestClient<IUserCredentialsRequest>> _requestBrokerMock;
        private Mock<Response<IOperationResult<IUserCredentialsResponse>>> _responseBrokerMock;
        private Mock<IOperationResult<IUserCredentialsResponse>> _operationResultMock;
        private Mock<IUserCredentialsResponse> _brokerResponseMock;

        private string _salt;
        private string _passwordHash;
        private ILoginCommand _command;

        private LoginRequest _request;
        #endregion

        #region Setup
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {

            _salt = "Example_Salt1";

            _loginValidatorMock = new Mock<ILoginValidator>();
            _loggerMock = new Mock<ILogger<LoginCommand>>();

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

            var connectionInfo = new Mock<ConnectionInfo>();

            connectionInfo
                .Setup(x => x.RemoteIpAddress)
                .Returns(new IPAddress(byte.MinValue));

            httpContextAccessorMock
                .Setup(x =>
                    x.HttpContext.Connection)
                .Returns(connectionInfo.Object);

            _request = new LoginRequest
            {
                LoginData = "User_login_example",
                Password = "Example_1234"
            };

            BrokerSetUp();

            _tokenEngineMock = new Mock<ITokenEngine>();
            _command = new LoginCommand(
                _tokenEngineMock.Object,
                _loginValidatorMock.Object,
                _loggerMock.Object,
                httpContextAccessorMock.Object,
                _requestBrokerMock.Object);
        }

        [SetUp]
        public void SetUp()
        {
            _brokerResponseMock = new Mock<IUserCredentialsResponse>();
            _brokerResponseMock.Setup(x => x.UserId).Returns(Guid.NewGuid());
            _brokerResponseMock.Setup(x => x.PasswordHash).Returns(_passwordHash);
            _brokerResponseMock.Setup(x => x.Salt).Returns(_salt);
            _brokerResponseMock.Setup(x => x.UserLogin).Returns(_request.LoginData);

            _operationResultMock = new Mock<IOperationResult<IUserCredentialsResponse>>();
            _operationResultMock.Setup(x => x.Body).Returns(_brokerResponseMock.Object);
            _operationResultMock.Setup(x => x.IsSuccess).Returns(true);
            _operationResultMock.Setup(x => x.Errors).Returns(new List<string>());

            _requestBrokerMock.Setup(
                x => x.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    It.IsAny<object>(), default, default))
                .Returns(Task.FromResult(_responseBrokerMock.Object));

            _responseBrokerMock
                .SetupGet(x => x.Message)
                .Returns(_operationResultMock.Object);
        }

        private void BrokerSetUp()
        {
            _passwordHash = PasswordHelper.GetPasswordHash(
                _request.LoginData,
                _salt,
                _request.Password);

            _responseBrokerMock = new Mock<Response<IOperationResult<IUserCredentialsResponse>>>();
            _requestBrokerMock = new Mock<IRequestClient<IUserCredentialsRequest>>();
        }
        #endregion

        #region Successful test
        [Test]
        public void ShouldReturnUserIdAndJwtWhenPasswordsAndEmailHasMatch()
        {
            string JwtToken = "Example_jwt";

            var expectedLoginResponse = new LoginResult
            {
                UserId = _brokerResponseMock.Object.UserId,
                Token = JwtToken
            };

            _loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(true);

            _tokenEngineMock
                .Setup(X => X.Create(_brokerResponseMock.Object.UserId))
                .Returns(JwtToken);

            SerializerAssert.AreEqual(expectedLoginResponse, _command.Execute(_request));
        }
        #endregion

        #region Fail tests
        [Test]
        public void ShouldThrowExceptionWhenPasswordsHasNotMatch()
        {
            _loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(true);

            _request.Password = "Example";

            Assert.Throws<ForbiddenException>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldThrowExceptionWhenNotConnectionToBroker()
        {
            _requestBrokerMock.Setup(
                x => x.GetResponse<IOperationResult<IUserCredentialsResponse>>(
                    It.IsAny<object>(), default, default))
                .ThrowsAsync(new Exception());

            Assert.Throws<NotFoundException>(() => _command.Execute(_request));
        }

        [Test]
        public void ShouldThrowExceptionWhenUserEmailHasNotMatchInDb()
        {
            _operationResultMock.Setup(x => x.Body).Returns((IUserCredentialsResponse)null);
            _operationResultMock.Setup(x => x.IsSuccess).Returns(false);
            _operationResultMock.Setup(x => x.Errors).Returns(new List<string> { "User email not found" });

            _loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(true);

            Assert.Throws<NotFoundException>(
                () => _command.Execute(_request),
                "User email not found");
        }

        [Test]
        public void ShouldThrowExceptionWhenUserLoginInfoNotValid()
        {
            _request.LoginData = string.Empty;

            _loginValidatorMock
               .Setup(x => x.Validate(It.IsAny<IValidationContext>()).IsValid)
               .Returns(false);

            Assert.Throws<ValidationException>(() => _command.Execute(_request));
        }
        #endregion
    }
}