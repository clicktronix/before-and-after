using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using WebApplication.Models.DomainModels;

namespace WebApplication.Models.ViewModels
{
    public class EventViewModel
    {
        public long Id { set; get; }

        [Display(Name = "Отправитель")]
        public ApplicationUser Sender { set; get; }

        [Display(Name = "Содержание")]
        public string Text { set; get; }

        [Display(Name = "Дата")]
        public DateTime Date { set; get; }

        [Display(Name = "Изображение")]
        public string Image { set; get; }

        public EventType EventType { set; get; }

        public ICollection<ApplicationUser> Owners { set; get; }

        public EventViewModel()
        {
            Owners = new Collection<ApplicationUser>();
        }
    }
}