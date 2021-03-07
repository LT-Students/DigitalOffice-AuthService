using LT.DigitalOffice.AuthenticationService.Broker.Consumers;
using LT.DigitalOffice.AuthenticationService.Token.Interfaces;
using LT.DigitalOffice.Broker.Requests;
using LT.DigitalOffice.Kernel.Broker;
using MassTransit.Testing;
using Moq;
using NUnit.Framework;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LT.DigitalOffice.AuthenticationService.Broker.UnitTests.Consumers
{
    class UserJwtConsumerTests
    {
        private Mock<ITokenValidator> jwtValidationMock;

        private InMemoryTestHarness harness;
        private string userJwt;
        private ConsumerTestHarness<JwtConsumer> consumerTestHarness;

        #region Setup
        [SetUp]
        public void SetUp()
        {
            userJwt = "Example_jwt";

            harness = new InMemoryTestHarness();
            jwtValidationMock = new Mock<ITokenValidator>();

            consumerTestHarness = harness.Consumer(() => new JwtConsumer(jwtValidationMock.Object));
        }
        #endregion

        #region Response to broker
        [Test]
        public async Task ShouldSendResponseToBrokerWhenUserJwtIsValid()
        {
            object expectedErrors = null;
            Guid expectedBody = Guid.Empty;

            await harness.Start();

            try
            {
                var requestClient = await harness.ConnectRequestClient<ICheckTokenRequest>();

                var response = await requestClient.GetResponse<IOperationResult<Guid>>(new
                {
                    UserJwt = userJwt
                });

                Assert.IsTrue(response.Message.IsSuccess);
                Assert.AreEqual(expectedErrors, response.Message.Errors);
                Assert.AreEqual(expectedBody, response.Message.Body);
                Assert.IsTrue(consumerTestHarness.Consumed.Select<ICheckTokenRequest>().Any());
            }
            finally
            {
                await harness.Stop();
            }
        }
        #endregion

        #region Throw exception
        [Test]
        public async Task ShouldExceptioWhenUserJwtIsNotValid()
        {
            string expectedErrors = "Token failed validation";
            Guid expectedBody = Guid.Empty;

            jwtValidationMock
                .Setup(x => x.Validate(It.IsAny<string>()))
                .Throws(new Exception("Token failed validation"));

            await harness.Start();

            try
            {
                var requestClient = await harness.ConnectRequestClient<ICheckTokenRequest>();

                var response = await requestClient.GetResponse<IOperationResult<Guid>>(new
                {
                    UserJwt = userJwt
                });

                Assert.IsFalse(response.Message.IsSuccess);
                Assert.AreEqual(expectedBody, response.Message.Body);
                Assert.AreEqual(expectedErrors, string.Join(", ", response.Message.Errors));
                Assert.IsTrue(consumerTestHarness.Consumed.Select<ICheckTokenRequest>().Any());
            }
            finally
            {
                await harness.Stop();
            }
        }
        #endregion
    }
}