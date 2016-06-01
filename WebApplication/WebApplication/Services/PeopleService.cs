using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using AutoMapper;
using WebApplication.Interfaces;
using WebApplication.Models;
using WebApplication.Models.DomainModels;
using WebApplication.Models.Repositories;
using WebApplication.Models.ViewModels;

namespace WebApplication.Services
{
    public class PeopleService
    {
        public IUnitOfWork UnitOfWork { get; set; }

        public PeopleService()
        {
            UnitOfWork = new EfUnitOfWork();
        }

        #region User
        public ApplicationUser GetUser(string id)
        {
            if (id == null)
                throw new ValidationException("Не установлено id пользователя");

            var user = UnitOfWork.Users.GetById(id);

            if (user == null)
                throw new ValidationException("Пользователь не найден");

            return user;
        }

        public ApplicationUser GetUser(Func<ApplicationUser, bool> predicate)
        {
            return UnitOfWork.Users.Find(predicate).FirstOrDefault();
        }

        public IEnumerable<ApplicationUser> GetUsers()
        {
            return UnitOfWork.Users.GetAll();
        }

        public void EditUser(ApplicationUser user)
        {
            UnitOfWork.Users.Update(user);
            UnitOfWork.Save();
        }

        public string GetDateOfActivity(DateTime dateOfActivity)
        {
            if ((DateTime.Now - dateOfActivity).TotalMinutes < 60)
            {
                var res = (DateTime.Now - dateOfActivity).TotalMinutes;
                return Math.Round(res).ToString() + " мин. назад";
            }

            if ((DateTime.Now - dateOfActivity).TotalHours < 24)
            {
                var res = (DateTime.Now - dateOfActivity).TotalHours;
                return Math.Round(res).ToString() + " ч. назад";
            }
            
            return dateOfActivity.ToLongDateString() + " в " + dateOfActivity.ToLongTimeString();
        }

        public void SetDateOfActivity(string currentUserId)
        {
            UnitOfWork.Users.GetById(currentUserId).DateOfActivity = DateTime.Now;
            UnitOfWork.Save();
        }

        public string GetStatus(string userId)
        {
            return UnitOfWork.Users.GetById(userId).Status;
        }

        public string EditStatus(string userId, string status)
        {
            var user = UnitOfWork.Users.GetById(userId);
            user.Status = status;
            UnitOfWork.Users.Update(user);
            UnitOfWork.Save();

            return status;
        }
        #endregion

        #region Friendship
        // проверяем, есть ли определённый юзер в друзьях у нашего юзера
        public bool CheckIfUserIsMyFriend(string currentUserId, string friendId)
        {
            // Оставил, потому-что неизвестно какой из запросов оптимальней.
            //var offerUser = UnitOfWork.Users.GetById(currentUserId);
            //var receivedUser = UnitOfWork.Users.GetById(friendId);

            //var fr = UnitOfWork.Friendships.GetAll();

            //if (fr.Any(f => f.FirstUser == offerUser && f.SecondUser == receivedUser || f.SecondUser == offerUser && f.FirstUser == receivedUser && f.DateOfDelete == null))
            //    return true;

            //return false;

            var currentFriendships = UnitOfWork.Friendships.GetAll()
                .Where(f => f.FirstUser.Id == currentUserId).Union(UnitOfWork.Friendships.GetAll()
                .Where(f => f.SecondUser.Id == currentUserId)).Where(f => f.DateOfDelete == null);

            var sectionResult = currentFriendships.Where(f => f.FirstUser.Id == friendId)
                .Union(currentFriendships.Where(f => f.SecondUser.Id == friendId));

            if (sectionResult.Any())
                return true;

            return false;
        }

        public bool CheckIfThereIsNewOfferFriendshipFromUserToMe(string currentUserId, string friendId)
        {
            var offeringUser = UnitOfWork.Users.GetById(friendId);
            var receivedUser = UnitOfWork.Users.GetById(currentUserId);

            return (currentUserId == friendId) || (UnitOfWork.OfferFriendships.Find(o => o.Status == false)
                .Where(o => o.Offer == offeringUser).FirstOrDefault(o => o.Received == receivedUser) != null);
        }

        public bool CheckIfThereIsNewOfferFriendshipFromMeToUser(string currentUserId, string friendId)
        {
            var offeringUser = UnitOfWork.Users.GetById(currentUserId);
            var receivedUser = UnitOfWork.Users.GetById(friendId);

            if ((currentUserId == friendId) || (UnitOfWork.OfferFriendships.Find(o => o.Status == false)
                .Where(o => o.Offer == offeringUser).FirstOrDefault(o => o.Received == receivedUser) != null))
            {
                return true;
            }

            return false;
        }

        // создаем заявку в друзья
        public bool MakeOfferFriendship(string offeringUserId, string receivedUserId)
        {
            var offeringUser = UnitOfWork.Users.GetById(offeringUserId);
            var receivedUser = UnitOfWork.Users.GetById(receivedUserId);

            // здесь мы проверяем, не совпадает ли айди отправителя с айди получателем, а также
            // есть ли уже необработанная заявка к этому получателю.
            if (CheckIfThereIsNewOfferFriendshipFromMeToUser(offeringUserId, receivedUserId)) return false;
 
            if (CheckIfThereIsNewOfferFriendshipFromUserToMe(offeringUserId, receivedUserId)) return false;

            var offer = new OfferFriendship
            {
                Offer = offeringUser,
                Received = receivedUser,
                Status = false,
                Date = DateTime.Now
            };

            UnitOfWork.OfferFriendships.Create(offer);
            UnitOfWork.Save();

            return true;
        }

        // отменяем заявку в друзья
        public bool CancelOfferFriendship(string offeringUserId, string receivedUserId)
        {
            var offerongUser = UnitOfWork.Users.GetById(offeringUserId);
            var receivedUser = UnitOfWork.Users.GetById(receivedUserId);

            var offerFriendship = UnitOfWork.OfferFriendships.Find(o => o.Status == false)
                .Where(o => o.Offer == offerongUser).FirstOrDefault(o => o.Received == receivedUser);

            if (offerFriendship == null)
            {
                return false;
            }

            offerFriendship.Status = true;
            UnitOfWork.OfferFriendships.Update(offerFriendship);
            UnitOfWork.Save();

            return true;
        }

        // получаем количество необработанных заявок в друзья
        public int GetCountOffersFriendships(string currentUserId)
        {
            var currentUser = UnitOfWork.Users.GetById(currentUserId);
            var offers = UnitOfWork.OfferFriendships.GetAll().Where(o => o.Status == false)
                .Where(o => o.Received == currentUser);

            return offers.Count();
        }

        public List<PeopleViewModel> GetAllFriends(string currentUserId)
        {
            var selection1 = UnitOfWork.Friendships.GetAll().Where(f => f.FirstUser.Id == currentUserId && f.DateOfDelete == null);
            var selection2 = UnitOfWork.Friendships.GetAll().Where(f => f.SecondUser.Id == currentUserId && f.DateOfDelete == null);

            var userF1 = UnitOfWork.Users.GetAll()
                .Join(selection2, user => user.Id, friendship => friendship.FirstUser.Id,
                    (user, friendship) => Mapper.Map<ApplicationUser, PeopleViewModel>(user)).ToList();
            var userF2 = UnitOfWork.Users.GetAll()
                .Join(selection1, user => user.Id, friendship => friendship.SecondUser.Id,
                    (user, friendship) => Mapper.Map<ApplicationUser, PeopleViewModel>(user)).ToList();
            userF1.AddRange(userF2);

            return userF1;
        }

        public List<PeopleViewModel> GetAllFriendsOnline(string currentUserId)
        {
            var myFriends = GetAllFriends(currentUserId);
            var myFriendsOnline = myFriends.Where(f => (f.DateOfActivity - DateTime.Now).Value.TotalMinutes > -3);

            return myFriendsOnline.ToList();
        }

        public List<PeopleViewModel> GetAllCommonFriends(string currentUserId, string friendId)
        {
            var myFriends = GetAllFriends(currentUserId);
            var friendsOfMyFriend = GetAllFriends(friendId);

            //// без LINQ
            //foreach (var f1 in myFriends)
            //{
            //    foreach (var f2 in friendsOfMyFriend)
            //    {
            //        if (f1.Id == f2.Id)
            //            commonFriends.Add(f1);
            //    }
            //}

            //IEnumerable<PeopleViewModel> commonFriends = myFriends.Intersect(friendsOfMyFriend); // эта хуйня не работает

            // LINQ от решарпера
            return (from f1 in myFriends from f2 in friendsOfMyFriend where f1.Id == f2.Id select f1).ToList();
        }

        public List<PeopleViewModel> GetAllFriendshipOffers(string currentUserId)
        {
            var users = new List<ApplicationUser>();
            var offers = new List<OfferFriendship>();
            var selection1 = new List<PeopleViewModel>();

            users = UnitOfWork.Users.GetAll().ToList();
            offers = UnitOfWork.OfferFriendships.GetAll().ToList();

            selection1 = users.Join(offers.Where(o => o.Status == false).Where(o => o.Received.Id == currentUserId), user => user.Id, friendship => friendship.Offer.Id,
            (user, friendship) => Mapper.Map<ApplicationUser, PeopleViewModel>(user)).ToList();

            return selection1;
        }

        public bool CreateNewFriendship(string currentUserId, string friendId)
        {
            var offeringUser = UnitOfWork.Users.GetById(friendId);
            var receivedUser = UnitOfWork.Users.GetById(currentUserId);

            var currentFriendships = UnitOfWork.Friendships.GetAll()
                .Where(f => f.FirstUser.Id == currentUserId).Union(UnitOfWork.Friendships.GetAll()
                .Where(f => f.SecondUser.Id == currentUserId)).Where(f => f.DateOfDelete == null);

            var sectionResult = currentFriendships.Where(f => f.FirstUser.Id == friendId)
                    .Union(currentFriendships.Where(f => f.SecondUser.Id == friendId));

            if (sectionResult.Any())
                return false;

            //вот здесь При старом запросе отрабатывает, а при AJAX-е выбрасывает NullExeption!!!
            //Входные параметры в метод - идентичны!
            // Короче непонятная фигня. Добавил выше:
            //                       var offerUser = UnitOfWork.Users.GetById(friendId);
            //                       var receivedUser = UnitOfWork.Users.GetById(currentUserId);
            //  для того, чтобы в выборке ниже, искать не по айдишникам, а по юзерам в целом... и заработало.
            // -----КОМЕНТ НЕ УБИРАТЬ!----- Еще вернемся к этому вопросу.
            var offerFriendship = UnitOfWork.OfferFriendships.Find(o => o.Offer == offeringUser)
                    .Where(o => o.Received == receivedUser).FirstOrDefault(o => o.Status == false);

            offerFriendship.Status = true;

            var friendship = new Friendship
            {
                FirstUser = GetUser(friendId),
                SecondUser = GetUser(currentUserId),
                DateOfCreate = DateTime.Now,
                DateOfDelete = null
            };

            UnitOfWork.Friendships.Create(friendship);
            UnitOfWork.Save();

            return true;
        }

        public bool DeleteFriendship(string currentUserId, string friendId)
        {
            var friendships1 = UnitOfWork.Friendships.GetAll()
                    .Where(f => f.FirstUser.Id == currentUserId).Where(f => f.SecondUser.Id == friendId).ToList();
            var friendships2 = UnitOfWork.Friendships.GetAll()
                    .Where(f => f.SecondUser.Id == currentUserId).Where(f => f.FirstUser.Id == friendId).ToList();
            friendships1.AddRange(friendships2);
            var friendship = friendships1.FirstOrDefault(f => f.DateOfDelete == null);

            friendship.DateOfDelete = DateTime.Now;
            UnitOfWork.Friendships.Update(friendship);
            UnitOfWork.Save();

            return true;
        }

        #endregion
        
        #region Event
        // создаем событие
        public bool MakeEvent(Event _event)
        {
            if (UnitOfWork.Events.GetAll().FirstOrDefault(e => e.Id == _event.Id) != null) return false;

            UnitOfWork.Events.Create(_event);
            UnitOfWork.Save();

            return true;
        }

        public List<EventViewModel> GetAllEventsFromUser(string userId)
        {
            var events = UnitOfWork.Events.GetAll().Where(o => o.Owners.Any(u => u.Id == userId));
            return Mapper.Map<IEnumerable<Event>, List<EventViewModel>>(events);
        }

        public List<EventViewModel> GetAllEventsFromUserFromType(string userId, EventType eventType)
        {
            var events = UnitOfWork.Events.GetAll().Where(o => o.Owners.Any(u => u.Id == userId))
                .Where(e => e.EventType == eventType);
            return Mapper.Map<IEnumerable<Event>, List<EventViewModel>>(events);
        }
        #endregion

        public void Dispose()
        {
            UnitOfWork.Dispose();
        }
    }
}