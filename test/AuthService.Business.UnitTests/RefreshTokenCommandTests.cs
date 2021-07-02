using LT.DigitalOffice.AuthService.Business.Commands;
using LT.DigitalOffice.AuthService.Business.Commands.Interfaces;
using LT.DigitalOffice.AuthService.Business.Helpers.Token.Interfaces;
using LT.DigitalOffice.Kernel.Enums;
using LT.DigitalOffice.Kernel.Responses;
using LT.DigitalOffice.UnitTestKernel;
using Moq;
using Moq.AutoMock;
using NUnit.Framework;
using System;

namespace LT.DigitalOffice.AuthService.Business.UnitTests
{
    public class RefreshTokenCommandTests
    {
        private IRefreshTokenCommand _command;
        private AutoMocker _mocker;

        [SetUp]
        public void SetUp()
        {
            _mocker = new();
            _command = _mocker.CreateInstance<RefreshTokenCommand>();
        }

        [Test]
        public void ShouldThrowExceptionWhenValidatorThrows()
        {
            string token = "token";

            _mocker
                .Setup<ITokenValidator, Guid>(x => x.Validate(It.IsAny<string>()))
                .Throws(new Exception());

            Assert.Throws<Exception>(() => _command.Execute(token));
        }

        [Test]
        public void ShouldReturnNewToken()
        {
            string token = "token";
            string newToken = "newToken";
            Guid userId = Guid.NewGuid();

            _mocker
                .Setup<ITokenValidator, Guid>(x => x.Validate(It.IsAny<string>()))
                .Returns(userId);

            _mocker
                .Setup<ITokenEngine, string>(x => x.Create(userId))
                .Returns(newToken);

            var expected = new OperationResultResponse<string>
            {
                Body = newToken,
                Status = OperationResultStatusType.FullSuccess
            };

            SerializerAssert.AreEqual(expected, _command.Execute(token));
        }
    }
}
