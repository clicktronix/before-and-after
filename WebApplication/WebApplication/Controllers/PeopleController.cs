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
	public class PeopleController : Controller
    {
        // ��� � ������ �����������, �������� � ���� ��� ����� ������
        private readonly WebApplication.Services.PeopleService _peopleService;

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

            // �������� ����
            var currentUser = _peopleService.GetUser(User.Identity.GetUserId());

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

        // ��� � ������������� � ������
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

        #region ������ � ������� (AJAX)
        // ���������� ������ � ������ (AJAX)
        [HttpPost]
        public ActionResult SendOfferFriendship(string friendId)
        {
            if (_peopleService.MakeOfferFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " ��������� " : " ���������� ")
                    + _peopleService.GetUser(friendId).Name + " � ������.",
                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.OfferFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };

                _peopleService.MakeEvent(_event);
                ViewBag.Result = "������ � ������ ����������.";
            }
            else
            {
                ViewBag.Result = "���-�� ����� �� ���";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // ������� �� ������ (AJAX)
        [HttpPost]
        public ActionResult DeleteFriendship(string friendId)
        {
            if (_peopleService.DeleteFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " ������ " : " ������� ")
                    + " ������������ " + _peopleService.GetUser(friendId).Name + " �� ������ ����� ������.",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.DeleteFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "�������� �� ������ �����������";
            }
            else
            {
                ViewBag.Result = "���-�� ����� �� ���";
            }

            return PartialView("FriendshipButtonStatus", friendId);
        }

        // ������������ ������ � ������ (AJAX)
        [HttpPost]
        public ActionResult ConfirmOfferFriendship(string friendId)
        {
            if (_peopleService.CreateNewFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " ���������� " : " ����������� ")
                    + "������ � ������ �� ������������ " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.ConfirmFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "������ �������";
            }
            else
            {
                ViewBag.Result = "���-�� ����� �� ���";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // ��������� ������ � ������ (AJAX)
        [HttpPost]
        public ActionResult DontConfirmOfferFriendship(string friendId)
        {
            if (_peopleService.CancelOfferFriendship(friendId, User.Identity.GetUserId()))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " �������� " : " ��������� ")
                    + "������ � ������ �� ������������ " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.DontConfirmOfferFriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "������ � ������ ���������.";
            }
            else
            {
                ViewBag.Result = "���-�� ����� �� ���";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }

        // �������� ������ � ������ (AJAX)
        [HttpPost]
        public ActionResult CancelOfferFriendship(string friendId)
        {
            if (_peopleService.CancelOfferFriendship(User.Identity.GetUserId(), friendId))
            {
                var _event = new Event
                {
                    Sender = _peopleService.GetUser(User.Identity.GetUserId()),
                    Text = _peopleService.GetUser(User.Identity.GetUserId()).Name +
                    (_peopleService.GetUser(User.Identity.GetUserId()).Gender == true ? " ������� " : " �������� ")
                    + "������ � ������ � ������������ " + _peopleService.GetUser(friendId).Name + ".",

                    Date = DateTime.Now,
                    Image = null,
                    EventType = EventType.CancelOfferfriendship,
                    Owners = { _peopleService.GetUser(User.Identity.GetUserId()), _peopleService.GetUser(friendId) }
                };
                _peopleService.MakeEvent(_event);
                ViewBag.Result = "������ � ������ ��������.";
            }
            else
            {
                ViewBag.Result = "���-�� ����� �� ���";
            }

            //return Redirect(HttpContext.Request.UrlReferrer.ToString());
            return PartialView("FriendshipButtonStatus", friendId);
        }
        #endregion

        #region ��������� ������ ������������� (AJAX)

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

            // ������ ��������� ApplicationUser-�� � ��������� PeopleViewModel-��
            viewUsers = Mapper.Map<IEnumerable<ApplicationUser>, List<PeopleViewModel>>(users);
            return PartialView("UsersList", viewUsers);
        }
        #endregion
    }
}