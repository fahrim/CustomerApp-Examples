using System.ComponentModel.DataAnnotations;

namespace CustomerApp.Models
{
    public class Customer
    {
        //public int CustomerID { get; set; }

        //[Required(ErrorMessage = "Müşteri adı zorunludur. [MODEL VAL.]")] // Boş olamaz
        //[StringLength(50, ErrorMessage = "Ad en fazla 50 karakter olabilir.")] // Maksimum uzunluk
        //public string FirstName { get; set; }

        //[Required(ErrorMessage = "Müşteri soyadı zorunludur.")]
        //[StringLength(50)]
        //public string LastName { get; set; }

        //[Required(ErrorMessage = "E-posta adresi zorunludur.")]
        //[EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")] // Format kontrolü
        //[StringLength(50)]
        //public string Email { get; set; }

        //[StringLength(20)]
        //public string? PhoneNumber { get; set; }

        //[StringLength(255)]
        //public string? Address { get; set; }

        //[StringLength(50)]
        //public string? City { get; set; }

        //[StringLength(50)]
        //public string? Country { get; set; }

        //public DateTime CreatedAt { get; set; }
        //public bool IsActive { get; set; }

        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
    }
}
