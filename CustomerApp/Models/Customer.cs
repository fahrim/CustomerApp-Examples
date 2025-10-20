namespace CustomerApp.Models
{
    public class Customer
    {
        public int CustomerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; } // veritabanında null olabilir o yüzden string? yaptık
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Country { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; } // bool SQL de bit
    }
}
