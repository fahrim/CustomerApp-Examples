using CustomerApp.Models;

namespace CustomerApp
{
    // Bu, bizim "standart priz" tanımımızdır.
    // Sadece ne yapılacağını söyler, nasıl yapılacağını söylemez.
    public interface ICustomerProvider
    {
        Task<Customer?> GetCustomerByIdAsync(int id);

        // Yeni Insert Metodumuz - Customer tipinde veri alacak dönüşte de int türünde döndürecek
        // (First-LastName, Address, City gibi özellikleri kaydetmek için alacak ve kayıt ettiği kaydın CustomerID sini geri dönddürecek)
        // Veritabanı bağlantısı olduğu için async işlem ve bundan dolayı task
        Task<int> InsertCustomerAsync(Customer customer);

        Task<int> UpdateCustomerAsync(Customer customer);

        Task<int> DeactivateCustomerAsync(int id);
    }
}
