using System;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using WebApplication.Models.DomainModels;
using WebApplication.Services;

namespace WebApplication.Controllers
{
    [Authorize]
	public class EventController : Controller
    {
        // как и другие контроллеры, содержит в себе вот такой сервис
        private readonly WebApplication.Services.PeopleService _peopleService;

        public EventController()
        {
            _peopleService = new PeopleService();
        }
        // GET: Event
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult AllEvents()
        {
            var events = _peopleService.GetAllEventsFromUser(User.Identity.GetUserId()).OrderByDescending(d => d.Date);

            return View(events);
        }

        public ActionResult RecordsOnTheWall(string userId)
        {
            var events =
                _peopleService.GetAllEventsFromUser(userId)
                    .Where(e => e.EventType == EventType.RecordOnTheWall).ToList();

            var events1 = events.Where(e => e.Sender.Id == userId && (e.Owners.Count == 1)).ToList();
            var events2 = events.Where(e => e.Sender.Id != userId).ToList();
            events1.AddRange(events2);
            return PartialView("AllEvents", events1.OrderByDescending(d => d.Date));
        }

        [HttpPost]
        public ActionResult RecordOnTheWall(string senderUserId, string recieverUserId, string textOnTheWall)
        {
            var keys = Request.Form.Keys;
            var _event = new Event
            {
                Sender = _peopleService.GetUser(senderUserId),
                Text = textOnTheWall,

                Date = DateTime.Now,
                Image = null,
                EventType = EventType.RecordOnTheWall,
                Owners = { _peopleService.GetUser(senderUserId), _peopleService.GetUser(recieverUserId) }
            };
            _peopleService.MakeEvent(_event);
            return PartialView("AllEvents", _peopleService.GetAllEventsFromUser(recieverUserId).Where(e => e.EventType == EventType.RecordOnTheWall).OrderByDescending(d => d.Date));
        }
    }
}