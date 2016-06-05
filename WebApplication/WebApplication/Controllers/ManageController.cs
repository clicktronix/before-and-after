using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using WebApplication.Models;
using WebApplication.Models.ViewModels;
using System.Collections.Generic;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private PeopleService peopleService;
        readonly ApplicationDbContext _db = new ApplicationDbContext();
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
            peopleService = new PeopleService();
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
            ViewBag.GetCountOffersFriendships = peopleService.GetCountOffersFriendships(User.Identity.GetUserId());
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Ваш пароль изменен."
                : message == ManageMessageId.SetPasswordSuccess ? "Пароль задан."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Настроен поставщик двухфакторной проверки подлинности."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : message == ManageMessageId.AddPhoneSuccess ? "Ваш номер телефона добавлен."
                : message == ManageMessageId.RemovePhoneSuccess ? "Ваш номер телефона удален."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Создание и отправка маркера
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Ваш код безопасности: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Отправка SMS через поставщик SMS для проверки номера телефона
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // Это сообщение означает наличие ошибки; повторное отображение формы
            ModelState.AddModelError("", "Не удалось проверить телефон");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("UserPage", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // Это сообщение означает наличие ошибки; повторное отображение формы
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "Внешнее имя входа удалено."
                : message == ManageMessageId.Error ? "Произошла ошибка."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Запрос перенаправления к внешнему поставщику входа для связывания имени входа текущего пользователя
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }
        
        public ActionResult Edit()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ApplicationUser userProfile, EditViewModel model)
        {
            string username = User.Identity.Name;
            
            ApplicationUser user = _db.Users.FirstOrDefault(u => u.UserName.Equals(username));

            if (!ModelState.IsValid)
            {
                _db.Entry(user).State = EntityState.Modified;
                _db.SaveChanges();
                return RedirectToAction("Index");
            }

            user.Name = userProfile.Name;
            user.Surname = userProfile.Surname;
            user.Age = userProfile.Age;
            user.Country = userProfile.Country;
            user.City = userProfile.City;

            _db.Entry(user).State = EntityState.Modified;
            _db.SaveChanges();

            return RedirectToAction("UserPage", "Manage", model);
        }

        //Users page
        public ActionResult UserPage(string Id, bool Welcome = false)
        {
            //if (Welcome) ViewBag.Welcome = true;
            Random rand = new Random();
            List<PeopleViewModel> friends = new List<PeopleViewModel>();

            if (Id == null)
                friends = peopleService.GetAllFriends(User.Identity.GetUserId());
            else
                friends = peopleService.GetAllFriends(Id);

            ViewBag.CountOfFriends = friends.Count;

            var friendsOnline = friends.Where(f => (f.DateOfActivity - DateTime.Now).Value.TotalMinutes > -3).ToList();
            ViewBag.CountOfFriendsOnline = friendsOnline.Count;

            var onlySixFriends = new List<PeopleViewModel>();

            if (friends.Count <= 6)
                ViewBag.Friends = friends;
            else
            {
                for (var i = 0; i < 6; i++)
                {
                    var index = rand.Next(0, friends.Count);
                    onlySixFriends.Add(friends[index]);

                    friends.RemoveAt(index);
                }

                ViewBag.Friends = onlySixFriends;
            }

            var onlySixFriendsOnline = new List<PeopleViewModel>();

            if (friendsOnline.Count <= 6)
                ViewBag.FriendsOnline = friendsOnline;
            else
            {
                for (var i = 0; i < 6; i++)
                {
                    var index = rand.Next(0, friendsOnline.Count);
                    onlySixFriendsOnline.Add(friendsOnline[index]);

                    friendsOnline.RemoveAt(index);
                }

                ViewBag.FriendsOnline = onlySixFriendsOnline;
            }

            // для отображения общих друзей
            if (Id != User.Identity.GetUserId() && Id != null)
            {
                var commonFriends = peopleService.GetAllCommonFriends(User.Identity.GetUserId(), Id);
                ViewBag.AllCommonFriends = commonFriends; // все общие друзья
                ViewBag.CountOfCommonFriends = commonFriends.Count; // кол-во общих друзей

                var onlySixCommonFriends = new List<PeopleViewModel>();

                if (commonFriends.Count <= 6)
                    ViewBag.SixOrLessCommonFriends = commonFriends; // для отображения 6 или меньше общих друзей
                else
                {
                    for (var i = 0; i < 6; i++)
                    {
                        var index = rand.Next(0, commonFriends.Count);
                        onlySixCommonFriends.Add(commonFriends[index]);

                        commonFriends.RemoveAt(index);
                    }

                    ViewBag.SixOrLessCommonFriends = onlySixCommonFriends; // для отображения 6 или меньше общих друзей
                }

                ViewBag.CurrentUserId = User.Identity.GetUserId();
            }
            //-----------------------------

            // обращаемся к AutoMapper
            UserPageViewModel user = new UserPageViewModel();
            if (Id == null || Id == User.Identity.GetUserId())
            {
                Mapper.Map(peopleService.GetUser(User.Identity.GetUserId()), user);
                return View(user);
            }
            Mapper.Map(peopleService.GetUser(Id), user);
            if (peopleService.CheckIfUserIsMyFriend(User.Identity.GetUserId(), Id))
            {
                return View("UserPageOfFriend", user);
            }
            return View("UserPageOfAnotherPerson", user);
        }

        [HttpPost]
        public ActionResult GetStatus()
        {
            var status = peopleService.GetStatus(User.Identity.GetUserId());
            return PartialView(status ?? "");
        }

        [HttpPost]
        public ActionResult EditStatus(string status)
        {
            var result = peopleService.EditStatus(User.Identity.GetUserId(), status);

            if (!string.IsNullOrEmpty(result))
                return PartialView("GetStatus", result);

            return PartialView("GetStatus", "Изменить статус");
        }

        public ActionResult ChangePhoto()
        {
            var user = peopleService.GetUser(User.Identity.GetUserId());
            var userView = Mapper.Map<ApplicationUser, UserPageViewModel>(user);

            if (user != null)
                return View(userView);

            return HttpNotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePhoto(string submitButton, UserPageViewModel model, HttpPostedFileBase uploadImage)
        {
            switch (submitButton)
            {
                case "Сохранить":
                    return SaveNewPhoto(model, uploadImage);
                case "Отмена":
                    return Cancel();
                default:
                    return View();
            }
        }

        public ActionResult Cancel()
        {
            return RedirectToAction("UserPage");
        }

        public ActionResult SaveNewPhoto(UserPageViewModel model, HttpPostedFileBase uploadImage)
        {
            Random rnd = new Random();

            // тут работа с записью файла с аватаром на сервер
            string directory = Server.MapPath(@"\Content\Images\AccountImages");
            string fileName = null;

            if (uploadImage != null && uploadImage.ContentLength > 0)
            {
                string tmp = Path.GetFileName(uploadImage.FileName);
                fileName = (rnd.Next(1, 100000).ToString() + tmp.GetHashCode() + tmp.Substring(tmp.Length - 4, 4));
                uploadImage.SaveAs(Path.Combine(directory, fileName));
            }

            if (fileName == null) return RedirectToAction("UserPage");
            model.Avatar = fileName;
            var currentUser = peopleService.GetUser(model.Id);
            Mapper.Map(model, currentUser);
            peopleService.EditUser(currentUser);

            return RedirectToAction("UserPage");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Вспомогательные приложения
        // Используется для защиты от XSRF-атак при добавлении внешних имен входа
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}