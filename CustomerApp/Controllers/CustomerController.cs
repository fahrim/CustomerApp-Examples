using CustomerApp.Models;
using Microsoft.AspNetCore.Mvc;

namespace CustomerApp.Controllers
{
    [Route("api/[controller]")] // Bu controller'a api/customer adresiyle erişilecek
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerProvider _customerProvider;

        // Dependency Injection ile CustomerProvider'ı alıyotuz. (model binding gibi provider bind ediyoruz gibi düşün)
        public CustomerController(ICustomerProvider customerProvider) { 
            _customerProvider = customerProvider;
        }

        // GET: api/customer/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _customerProvider.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                // Müşteri bulunamazsa 404 fırlatıyoruz
                return NotFound();
            }

            // Müşteri bulunursa 200 OK durumu ve müşteri verisini döndürüyoruz
            return Ok(customer);
        }

        // POST: api/customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer newCustomer) // - FromBody öğren???
        {
            // eklenecek müşteriye ait veri geliyor mu kontrol et? *nullcheck
            if (newCustomer == null)
            {
                return BadRequest("Müşteri verisi boş olamaz.");
            }

            // Provider aracılığıyla müşteriyi db ekle
            int newCustomerId = await _customerProvider.InsertCustomerAsync(newCustomer);

            // Bu yeni ID'yi kullanarak veritabanından kaydın son halini
            // (CreatedAt ve IsActive dahil) tekrar çek.
            var createdCustomer = await _customerProvider.GetCustomerByIdAsync(newCustomerId);

            // Eğer bir nedenle kayıt bulunamazsa (çok düşük ihtimal ama olabilir)
            if (createdCustomer == null)
            {
                // Sadece ID'yi döndürerek bir hata olduğunu belirt
                return Conflict("Kayıt oluşturuldu ancak tekrar okunamadı.");
            }

            // 201 Created durum kodu ile birlikte
            // yeni oluşturulan müşterinin "GetCustomerById" endpoint'inin adresini
            // ve müşterinin kendisini döndür.
            // - Öğren ???
            return CreatedAtAction( 
                nameof(GetCustomerById),    // Hangi metot ile çağrılabileceği bilgisi
                new { id = newCustomerId }, // O metota ait parametreleri
                createdCustomer                 // yeni oluşturulan veri
            );
        }

        [HttpPut("{id}")] // [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer updateCustomer)
        {
            // # VALIDATIONS
            if (updateCustomer == null) {
                return BadRequest("Müşteri bilgisi boş");
            }

            // 1. Doğrulama: URL'deki ID ile gövdedeki (body) ID eşleşiyor mu?
            if (id != updateCustomer.CustomerID)
            {
                return BadRequest("URL'deki ID ile müşteri verisindeki ID eşleşmiyor.");
            }

            // 2. Doğrulama: Bu müşteri veritabanında var mı?
            var existingCustomer = await _customerProvider.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound($"ID'si {id} olan müşteri bulunamadı.");
            }

            // 3. Adım: Veritabanında güncelleme işlemini yap VE SONUCU YAKALA
            var affectedRows = await _customerProvider.UpdateCustomerAsync(updateCustomer);

            // 2. KONTROL (Güncelleme sonrası kontrol)
            if (affectedRows == 0)
            {
                // Bu, 'race condition' durumunda veya veritabanında hiçbir
                // alan değişmediyse olabilir. Her iki durumda da, kaynağın
                // bulunamadığını/güncellenemediğini belirtmek en doğrusudur.
                return NotFound($"ID'si {id} olan müşteri güncellenemedi (veya zaten silinmiş olabilir).");
            }

            // 4. Adım: Kullanıcıya verinin güncellenmiş son halini döndür.
            var updatedCustomer = await _customerProvider.GetCustomerByIdAsync(id);

            return Ok(updatedCustomer); // 200 OK ve güncellenmiş nesne
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // Provider aracılığıyla müşteriyi pasif hale getir
            // ve etkilenen satır sayısını yakala
            var affectedRows = await _customerProvider.DeactivateCustomerAsync(id);

            // UPDATE'taki sağlam kontrolümüzü burada da kullanıyoruz:
            //if (affectedRows == 0)
            if (affectedRows <= 0) // SP deki NOCOUNT() yaptığımız için kaç tane işlem yaptığını sayamıyor ve -1 basıyor.
            {
                // 0 satır etkilendiyse, böyle bir müşteri yok demektir.
                return NotFound($"ID'si {id} olan müşteri bulunamadı.");
            }

            // İşlem başarılıysa, 204 No Content döndür.
            return NoContent(); // Ok("Silme işlemi başarılı!")
        }
    }
}
