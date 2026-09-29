using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using WebApplication1.Controllers;

namespace WebApplication1.Models //Consistency
{
    public class Car
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Вкажіть марку автомобіля.")]
        [StringLength(20, ErrorMessage = "Марка не може містити більше 20 символів.")]
        public string Brand { get; set; }

        [Required(ErrorMessage = "Вкажіть модель автомобіля.")]
        [StringLength(20, ErrorMessage = "Модель не може містити більше 20 символів.")]
        public string Model { get; set; }

        [Required(ErrorMessage = "Вкажіть рік випуску.")]
        [Range(1900, 2026, ErrorMessage = "Рік повинен бути в діапазоні від 1900 до 2026.")]
        public int Year { get; set; }
        [Required]
        [Range(0, 10000000)]
        public decimal Price { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; } 
        [StringLength(500)]
        public string? ImagePaths { get; set; }
    }
    public class User
    {
        public int Id { get; set; }          // Первинний ключ
        [Required]
        [StringLength(30)]
        public string Login { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [Required]
        public string Role { get; set; } //*
    }
    public class Order
    {
        public int Id { get; set; }


        [Required]
        public int CarId { get; set; } // Зовнішній ключ до машини
        [ValidateNever]
        public Car Car { get; set; }


        [Required]
        public int UserId { get; set; } // Зовнішній ключ до клієнта
        [ValidateNever]
        public User User { get; set; }

        [Required(ErrorMessage = "Вкажіть дату замовлення.")]
        [DataType(DataType.DateTime)]
        [OrderDateValidation] // Кастомний атрибут,допомога chat gpt
        public DateTime OrderDate { get; set; } = DateTime.Now;
    }
    public class DeliveryInfo
    {
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; }

        [Required]
       
        public string Phone { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string PostalCode { get; set; }

        public string AdditionalInfo { get; set; }


        public int OrderId { get; set; }
        [ValidateNever]
        public Order Order { get; set; }
    }
}
