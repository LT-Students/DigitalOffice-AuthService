using FluentValidation.TestHelper;
using LT.DigitalOffice.AuthService.Models.Dto.Requests;
using LT.DigitalOffice.AuthService.Validation.Interfaces;
using NUnit.Framework;

namespace LT.DigitalOffice.AuthService.Validation.UnitTests
{
    public class LoginValidatorTests
    {
        private ILoginValidator validator;

        [OneTimeSetUp]
        public void SetUp()
        {
            validator = new LoginValidator();
        }

        [Test]
        public void GoodLoginRequestTest()
        {
            var request = new LoginRequest
            {
                LoginData = "admin",
                Password = "admin"
            };

            var result = validator.TestValidate(request);

            result.ShouldNotHaveAnyValidationErrors();
        }

        [Test]
        public void BadLoginRequestTest()
        {
            var request = new LoginRequest();

            var result = validator.TestValidate(request);
            result.ShouldHaveValidationErrorFor(x => x.LoginData);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}