namespace CustomerApp.Models
{
    // Bu, bir müşteriyi GÜNCELLEMEK için gereken
    // minimum veri setidir.
    // 'CreatedAt' veya 'IsActive' gibi alanları İÇERMEZ.
    public class UpdateCustomerCommand
    {
        // Bu alanı, Controller'daki 'id != command.CustomerID' kontrolü
        // ve Provider'daki 'sp_UpdateCustomer' için kullanacağız.
        public int CustomerID { get; set; }

        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }

        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
    }
}
