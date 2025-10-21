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
    public class CustomerProvider : ICustomerQueryProvider, ICustomerCommandProvider
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
        public async Task<int> InsertCustomerAsync(CreateCustomerCommand customerCommand)
        {
            await using var connection = new SqlConnection(_connectionString);

            // 'customerCommand' nesnemiz 'CreatedAt' veya 'IsActive' içermediği için,
            // onu doğrudan Dapper'a gönderebiliriz.
            // Dapper, command nesnesindeki (FirstName) property'leri
            // SP'deki (@FirstName) parametreleriyle otomatik eşleştirecek.

            // ExecuteScalarAsync, sorgudan dönen ilk satırın ilk sütunundaki
            // değeri (yani bizim NewCustomerID'mizi) alır.
            var newCustomerId = await connection.ExecuteScalarAsync<int>(
                "sp_InsertCustomer", // SP adımız
                customerCommand, // SP'ye gönderilecek parametreler burada
                commandType: CommandType.StoredProcedure
            );

            return newCustomerId; // Yeni kaydın ID değerini döndürüyoruz.
        }

        public async Task<int> UpdateCustomerAsync(UpdateCustomerCommand customer)
        {
            await using var connection = new SqlConnection(_connectionString);

            // ARTIK 'parameters' ADINDA BİR ANONİM NESNEYE İHTİYACIMIZ YOK!
            // 'customer' nesnemiz 'CreatedAt' içermediği için,
            // onu doğrudan Dapper'a gönderebiliriz.
            // Dapper, 'customer.CustomerID' gibi property'leri
            // SP'deki '@CustomerID' gibi parametrelerle eşleştirecektir.

            // ExecuteAsync, bir sorgu değil, bir komut (UPDATE, DELETE, INSERT) çalıştırmak için kullanılır.
            // Geriye etkilenen satır sayısını (int) döndürür.
            var affectedRows = await connection.ExecuteAsync( // dönüş zaten int olduğu için generic versiyonu hata verdi
                "sp_UpdateCustomer", // SP adımız
                customer,
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
