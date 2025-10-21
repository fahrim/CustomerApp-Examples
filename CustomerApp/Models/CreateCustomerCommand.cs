using System.ComponentModel.DataAnnotations;

namespace CustomerApp.Models
{
    // Bu sınıf, 'Customer' modelinden farklı olarak,
    // CustomerID, CreatedAt, IsActive gibi sunucu taraflı alanları İÇERMEZ.
    public class CreateCustomerCommand
    {
        // # CreateCustomer metodunda FluentValidation kullanmayacağımız için data annotation ekliyoruz
        [Required(ErrorMessage = "Müşteri adı zorunludur.")]
        [StringLength(50)]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Müşteri soyadı zorunludur.")]
        [StringLength(50)]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "E-posta adresi zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
        [StringLength(50)]
        public required string Email { get; set; }

        // Bunlar '?' (nullable) olduğu için 'required' gerekmez.
        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [StringLength(255)]
        public string? Address { get; set; }

        [StringLength(50)]
        public string? City { get; set; }

        [StringLength(50)]
        public string? Country { get; set; }
    }
}
