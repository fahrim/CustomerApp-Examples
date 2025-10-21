using CustomerApp.Models;

namespace CustomerApp
{
    /**
     * Sadece komut (yazma) işlemleri için kullanılan arayüz. [COMMAND]
     */
    public interface ICustomerCommandProvider
    {
        Task<int> InsertCustomerAsync(Customer customer);
        Task<int> UpdateCustomerAsync(Customer customer);
        Task<int> DeactivateCustomerAsync(int id);
    }
}
