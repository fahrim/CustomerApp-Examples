using CustomerApp.Models;

namespace CustomerApp
{
    /**
     * Sadece komut (yazma) işlemleri için kullanılan arayüz. [COMMAND]
     */
    public interface ICustomerCommandProvider
    {
        Task<int> InsertCustomerAsync(CreateCustomerCommand customer);
        Task<int> UpdateCustomerAsync(UpdateCustomerCommand customer);
        Task<int> DeactivateCustomerAsync(int id);
    }
}
