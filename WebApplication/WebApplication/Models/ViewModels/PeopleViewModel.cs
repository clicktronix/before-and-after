using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication.Models.ViewModels
{
    // вьюмодель для работы с другими пользователями
    public class PeopleViewModel
    {
        [HiddenInput(DisplayValue = false)]
        public string Id { set; get; }

        [Required]
        [Display(Name = "Имя:")]
        public string Name { set; get; }

        [Required]
        [Display(Name = "Фамилия:")]
        public string Surname { set; get; }

        [Required]
        [Display(Name = "Дата рождения:")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime Age { set; get; }

        [Display(Name = "Аватар:")]
        public string Avatar { set; get; }

        [Required]
        [Display(Name = "Страна:")]
        public string Country { set; get; }

        [Required]
        [Display(Name = "Город:")]
        public string City { set; get; }

        [Display(Name = "Телефон:")]
        public string PhoneNumber { set; get; }

        [Required]
        [Display(Name = "Пол:")]
        public string Gender { set; get; }

        [Display(Name = "Почта:")]
        public string Email { set; get; }

        public bool IsFriend { get; set; }
        public bool IsThereNewOfferFriendshipFromUserToMe { get; set; }
        public bool IsThereNewOfferFriendshipFromMeToUser { get; set; }

        [Display(Name = "Последняя активность:")]
        public DateTime? DateOfActivity { set; get; }

        [Display(Name = "Статус:")]
        public string Status { set; get; }
    }
}