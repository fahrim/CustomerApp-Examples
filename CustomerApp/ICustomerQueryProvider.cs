using CustomerApp.Models;

namespace CustomerApp
{
    /**
     * Sadece sorgulama (read-only) işlemleri için kullanılan arayüz. [QUERY]
     */
    public interface ICustomerQueryProvider
    {
        Task<Customer?> GetCustomerByIdAsync(int id);

        Task<bool> IsEmailTakenAsync(string email, int customerId);
    }
}
