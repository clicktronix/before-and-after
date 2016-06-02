using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace WebApplication.Models.ViewModels
{
    public class UserPageViewModel
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
        public DateTime DateOfBirth { set; get; }

        [Display(Name = "Аватар:")]
        public string Avatar { set; get; }

        [Display(Name = "Страна:")]
        public string Country { set; get; }

        [Display(Name = "Город:")]
        public string City { set; get; }

        [RegularExpression("[0-9]*", ErrorMessage = "Поле Телефон может содержать только цифры")]
        [Display(Name = "Телефон:")]
        public string PhoneNumber { set; get; }

        [Required]
        [Display(Name = "Пол:")]
        public string Gender { set; get; }

        [Display(Name = "Почта:")]
        public string Email { set; get; }

        [Display(Name = "Последняя активность:")]
        public DateTime? DateOfActivity { set; get; }

        [Display(Name = "Статус:")]
        public string Status { set; get; }
    }
}