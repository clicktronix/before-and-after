using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Microsoft.AspNet.Identity;
using WebApplication.Models;
using WebApplication.Models.DomainModels;
using WebApplication.Models.ViewModels;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize]
    // контроллер, где будут методы для взаимодействия с другими пользователями
    public class PeopleController : Controller
    {
        // как и другие контроллеры, содержит в себе вот такой сервис
        private readonly PeopleService _peopleService;

        public PeopleController()
        {
            _peopleService = new PeopleService();
        }

        // GET: People
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ShowUsers(string choice)
        {
            var users = _peopleService.GetUsers();

            switch (choice)
            {
                case "Male":
                    users = _peopleService.GetUsers().Where(u => u.Gender == true).ToList();
                    break;
                case "Female":
                    users = _peopleService.GetUsers().Where(u => u.Gender == false).ToList();
                    break;
                default:
                    users = _peopleService.GetUsers().ToList();
                    break;
            }

            // получить себя
            var currentUser = _peopleService.GetUser(User.Identity.GetUserId());

            //users = _peopleService.GetUsers();

            //Маппим коллекцию ApplicationUser - ов в коллекцию PeopleViewModel - ов
            List<PeopleViewModel> viewUsers = Mapper.Map<IEnumerable<ApplicationUser>, List<PeopleViewModel>>(users);

            foreach (var user in viewUsers)
            {
                user.IsFriend = _peopleService.CheckIfUserIsMyFriend(currentUser.Id, _peopleService.GetUser(user.Id).Id);
                user.IsThereNewOfferFriendshipFromUserToMe = _peopleService.CheckIfThereIsNewOfferFriendshipFromUserToMe(currentUser.Id, _peopleService.GetUser(user.Id).Id);
                user.IsThereNewOfferFriendshipFromMeToUser = _peopleService.CheckIfThereIsNewOfferFriendshipFromMeToUser(currentUser.Id, _peopleService.GetUser(user.Id).Id);
            }

            return View(viewUsers);
        }

        

        public ActionResult ShowFriendsOrOffers()
        {
            var getCountOffersFriendships = _peopleService.GetCountOffersFriendships(User.Identity.GetUserId());

            if (getCountOffersFriendships > 0)
                return OfferfriendshipList();
            else
                return FriendshipList(User.Identity.GetUserId());
        }

        // Вью с приглашениями в друзья
        public ActionResult OfferfriendshipList()
        {
            var viewUsers = _peopleService.GetAllFriendshipOffers(User.Identity.GetUserId());
            return View("OfferfriendshipList", viewUsers);
        }

        public ActionResult FriendshipList(string id)
        {
            ViewBag.UserId = id;
            var friends = _peopleService.GetAllFriends(id);
            return View("FriendshipList", friends);
        }

        public ActionResult FriendsOnlineList(string id)
        {
            ViewBag.UserId = id;
            var friendsOnline = _peopleService.GetAllFriendsOnline(id);
            return View("FriendsOnlineList", friendsOnline);
        }

        public ActionResult CommonFriendshipList(string id)
        {
            ViewBag.UserId = id;
            var friends = _peopleService.GetAllCommonFriends(User.Identity.GetUserId(), id);
            return View("CommonFriendshipList", friends);
        }

        #region Работа с Дружбой (AJAX)
        // Отправляем заявку в друзья (AJAX)
        [HttpPost]
        public ActionResult SendOfferFriendship(string friendId)
        {
            if (_peopleService.MakeOfferFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " пригласил " : " пригласила ")
                    + _peopleService.GetUser(friendId).Name + " в друзья.",
                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.OfferFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };

                _peopleService.MakeEvent(_event);
                ViewBag.Result = "Заявка в друзья отправлена.";
            }
            else
            {
                ViewBag.Result = "Что-то пошло не так";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // Удаляем из друзей (AJAX)
        [HttpPost]
        public ActionResult DeleteFriendship(string friendId)
        {
            if (_peopleService.DeleteFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " удалил " : " удалила ")
                    + " пользователя " + _peopleService.GetUser(friendId).Name + " из списка своих друзей.",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.DeleteFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "Удаление из друзей произведено";
            }
            else
            {
                ViewBag.Result = "Что-то пошло не так";
            }

            return PartialView("FriendshipButtonStatus", friendId);
        }

        // Подтверждаем заявку в друзья (AJAX)
        [HttpPost]
        public ActionResult ConfirmOfferFriendship(string friendId)
        {
            if (_peopleService.CreateNewFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " подтвердил " : " подтвердила ")
                    + "заявку в друзья от пользователя " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.ConfirmFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "Заявка принята";
            }
            else
            {
                ViewBag.Result = "Что-то пошло не так";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // Отклоняем заявку в друзья (AJAX)
        [HttpPost]
        public ActionResult DontConfirmOfferFriendship(string friendId)
        {
            if (_peopleService.CancelOfferFriendship(friendId, User.Identity.GetUserId()))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " отклонил " : " отклонила ")
                    + "заявку в друзья от пользователя " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.DontConfirmOfferFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "Заявка в друзья отклонена.";
            }
            else
            {
                ViewBag.Result = "Что-то пошло не так";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // Отменяем заявку в друзья (AJAX)
        [HttpPost]
        public ActionResult CancelOfferFriendship(string friendId)
        {
            if (_peopleService.CancelOfferFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " отменил " : " отменила ")
                    + "заявку в друзья к пользователю " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.CancelOfferfriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "Заявка в друзья отменена.";
            }
            else
            {
                ViewBag.Result = "Что-то пошло не так";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }
        #endregion

        #region Получение списка пользователей (AJAX)

        [HttpPost]
        public ActionResult GetUsersList(string userId, string gender, bool online, string name, string surname, bool commonFriends)
        {
            List<PeopleViewModel> viewUsers = new List<PeopleViewModel>();

            var users = new List<ApplicationUser>();

            if (commonFriends)
            {
                users = Mapper.Map<List<PeopleViewModel>, List<ApplicationUser>>(_peopleService.GetAllCommonFriends(User.Identity.GetUserId(), userId));
            }
            else
            {
                if (!string.IsNullOrEmpty(userId))
                    users = Mapper.Map<List<PeopleViewModel>, List<ApplicationUser>>(_peopleService.GetAllFriends(userId));
                else
                    users = _peopleService.GetUsers().ToList();                
            }

            if (online)
                users = users.Where(f => (f.DateOfActivity - DateTime.Now).Value.TotalMinutes > -3).ToList();

            switch (gender)
            {
                case "Male":
                    users = users.Where(u => u.Gender == true).ToList();
                    break;
                case "Female":
                    users = users.Where(u => u.Gender == false).ToList();
                    break;
            }

            if (name != null)
                users = users.Where(u => u.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase)).ToList();

            if (surname != null)
                users = users.Where(u => u.Surname.StartsWith(surname, StringComparison.OrdinalIgnoreCase)).ToList();

            // Маппим коллекцию ApplicationUser-ов в коллекцию PeopleViewModel-ов
            viewUsers = Mapper.Map<IEnumerable<ApplicationUser>, List<PeopleViewModel>>(users);
            return PartialView("UsersList", viewUsers);
        }
        #endregion
    }
}