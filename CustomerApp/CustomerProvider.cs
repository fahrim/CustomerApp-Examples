using Microsoft.Data.SqlClient;
using System.Data;
using CustomerApp.Models;
using Dapper;

namespace CustomerApp
{
    /**
     * Veri Erişim Katmanı
     * 
     * AI NOTE: Bu sınıf, veritabanıyla konuşma işinin tamamını üstlenecek.
     * Dapper'ı ve sp_GetCustomerByID SP'sini burada kullanacağız.
     */
    public class CustomerProvider : ICustomerProvider
    {
        private readonly string? _connectionString;

        public CustomerProvider(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ID'ye göre tek bir müşteri getiren asenkron metot
        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString); // veritabanına bağlan -await ile bağlanmasını bekle -usign kullanınca kapattırır(kolaylık)

            // Dapperi kullanarak Stored Procedure'ü çalıştırıyoruz
            var customer = await connection.QuerySingleOrDefaultAsync<Customer>( // connection metodlarına çalışılacak/öğrenilecek/bakılacak
                "sp_GetCustomerByID",
                new { CustomerID = id }, // SP'ye gönderilecek parametreler -EK: @ olamdan göndersekte ki bu daha iyi kullanımmış Dapper bunu @ ile gönderecekmiş
                commandType: CommandType.StoredProcedure // Bu komutun bir SP olduğunu belirtiyoruz
            );

            return customer;
        }
        

        // Verileri Kaydedip ID döndüren metot
        public async Task<int> InsertCustomerAsync(Customer customer)
        {
            await using var connection = new SqlConnection(_connectionString);

            // SP'miz tüm properyleri istemiyor bu yüzden sadece istediklerini attık
            var parameters = new
            {
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.Address,
                customer.City,
                customer.Country
            };

            // ExecuteScalarAsync, sorgudan dönen ilk satırın ilk sütunundaki
            // değeri (yani bizim NewCustomerID'mizi) alır.
            var newCustomerId = await connection.ExecuteScalarAsync<int>(
                "sp_InsertCustomer", // SP adımız
                parameters, // SP'ye gönderilecek parametreler burada
                commandType: CommandType.StoredProcedure
            );

            return newCustomerId; // Yeni kaydın ID değerini döndürüyoruz.
        }

        public async Task<int> UpdateCustomerAsync(Customer customer)
        {
            await using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                customer.CustomerID,
                customer.FirstName,
                customer.LastName,
                customer.Email,
                customer.PhoneNumber,
                customer.Address,
                customer.City,
                customer.Country
            };

            // ExecuteAsync, bir sorgu değil, bir komut (UPDATE, DELETE, INSERT) çalıştırmak için kullanılır.
            // Geriye etkilenen satır sayısını (int) döndürür.
            var affectedRows = await connection.ExecuteAsync( // dönüş zaten int olduğu için generic versiyonu hata verdi
                "sp_UpdateCustomer", // SP adımız
                // customer, // Customer nesnesini doğrudan gönderebiliriz. - Dapper, property isimlerini (örn: customer.CustomerID). - SP parametreleriyle (@CustomerID) otomatik eşleştirir.
                parameters, // customer olduğu gibi gönderince createdAt gibi değerleride default olarak gönderiyor ve istemediğimiz güncelleme işlemi oluyor hatta 500 hatası alıyoruz eski tarih nedenyile
                commandType: CommandType.StoredProcedure
            );

            return affectedRows;
        }

        public async Task<int> DeactivateCustomerAsync(int id)
        {
            await using var connection = new SqlConnection(_connectionString);

            // Yine ExecuteAsync kullanıyoruz, çünkü bu bir komuttur
            // ve etkilenen satır sayısını döndürür.
            var affectedRows = await connection.ExecuteAsync(
                "sp_DeactivateCustomer",
                new { CustomerID = id }, // Sadece ID'yi anonim bir nesne ile gönderiyoruz
                commandType: CommandType.StoredProcedure
            );

            return affectedRows;
        }

        /**
         * Email'in başka bir müşteri tarafından kullanılıp kullanılmadığını kontrol eden metot
         */
        public async Task<bool> IsEmailTakenAsync(string email, int customerId)
        {
            await using var connection = new SqlConnection(_connectionString);

            var parameters = new
            {
                Email = email,
                CustomerID = customerId
            };

            // ExecuteScalarAsync<bool> ile SP'den dönen
            // CAST(1 AS BIT) veya CAST(0 AS BIT) değerini yakalıyoruz.
            var isTaken = await connection.ExecuteScalarAsync<bool>(
                "sp_CheckEmailExists",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return isTaken;
        }
    }
}
