using FluentValidation;
using FluentValidation.TestHelper;
using LT.DigitalOffice.AuthenticationService.Models.Dto;
using NUnit.Framework;

namespace LT.DigitalOffice.AuthenticationService.Validation.UnitTests
{
    class UserLoginValidatorTests
    {
        private IValidator<UserLoginInfoRequest> validator;

        [SetUp]
        public void SetUp()
        {
            validator = new UserLoginValidator();
        }

        [Test]
        public void EmptyLoginEmptyPassword()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Login, "");
            validator.ShouldHaveValidationErrorFor(x => x.Password, "");
        }

        [Test]
        public void EmptyLogin()
        {
            validator.ShouldHaveValidationErrorFor(x => x.Login, "");
            validator.ShouldNotHaveValidationErrorFor(x => x.Password, "Example");
        }

        [Test]
        public void EmptyPassword()
        {
            validator.ShouldNotHaveValidationErrorFor(x => x.Login, "Example@mail.com");
            validator.ShouldHaveValidationErrorFor(x => x.Password, "");
        }

        [Test]
        public void EmailIsValid()
        {
            validator.ShouldNotHaveValidationErrorFor(x => x.Login, "Example");
        }
    }
}