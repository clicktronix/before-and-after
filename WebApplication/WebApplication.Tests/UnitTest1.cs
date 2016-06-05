using System.Web.Mvc;
using NUnit.Framework;
using WebApplication.Controllers;
using WebApplication.Models;
using Assert = NUnit.Framework.Assert;

namespace WebApplication.Tests
{
    [TestFixture, NUnit.Framework.Description("Тестирование контроллера Account")]
    public class AccountControllerTest
    {
        [Test]
        public void ForgotPassword()
        {
            var controller = new AccountController();
            var result = controller.ForgotPassword();
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<ForgotPasswordViewModel>(((ViewResult)result).Model);
        }

        [Test]
        public void ResetPassword()
        {
            var controller = new AccountController();
            var result = controller.ResetPassword((string) null);
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<ResetPasswordViewModel>(((ViewResult)result).Model);
        }

        [Test]
        public void Register()
        {
            var controller = new AccountController();
            var result = controller.Register();
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsInstanceOf<RegisterViewModel>(((ViewResult)result).Model);
        }
    }
}
