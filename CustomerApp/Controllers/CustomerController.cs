using CustomerApp.Models;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using FluentValidation.Results;

namespace CustomerApp.Controllers
{
    // Bu controller'a api/customer adresiyle erişilecek
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerQueryProvider _queryProvider;
        private readonly ICustomerCommandProvider _commandProvider;
        private readonly IValidator<UpdateCustomerCommand> _updateValidator;

        // Dependency Injection constructor
        public CustomerController(
            ICustomerQueryProvider queryProvider,
            ICustomerCommandProvider commandProvider,
            IValidator<UpdateCustomerCommand> updateValidator) 
        { 
            _queryProvider = queryProvider;
            _commandProvider = commandProvider;
            _updateValidator = updateValidator;
        }

        // Metodun ANA SORUMLULUĞU OKUMAK'tır (Query)
        // GET: api/customer/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomerById(int id)
        {
            var customer = await _queryProvider.GetCustomerByIdAsync(id);

            if (customer == null)
            {
                // Müşteri bulunamazsa 404 fırlatıyoruz
                return NotFound();
            }

            // Müşteri bulunursa 200 OK durumu ve müşteri verisini döndürüyoruz
            return Ok(customer);
        }

        // Metodun ANA SORUMLULUĞU YAZMAK'tır (Command), ancak iş akışını (workflow) tamamlamak için Okuma (Query) sağlayıcısını da kullanır.
        // POST: api/customer
        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerCommand newCustomer)
        {
            // # VALIDATIONS 1. Kontrol: ModelState (Model doğrulama)
            // ASP.NET Core, newCustomer nesnesini ve üzerindeki [Required] vb.
            // öznitelikleri otomatik olarak kontrol eder.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); // 400 Bad Request ve doğrulama hataları
            }

            // 2. Kontrol: NULL CHECK (eklenecek müşteriye ait veri geliyor mu kontrol et)
            if (newCustomer == null)
            {
                return BadRequest("Müşteri verisi boş olamaz.");
            }

            // Provider aracılığıyla müşteriyi db ekle
            int newCustomerId = await _commandProvider.InsertCustomerAsync(newCustomer);

            // Bu yeni ID'yi kullanarak veritabanından kaydın son halini
            // (CreatedAt ve IsActive dahil) tekrar çek.
            var createdCustomer = await _queryProvider.GetCustomerByIdAsync(newCustomerId);

            // Eğer bir nedenle kayıt bulunamazsa (çok düşük ihtimal ama olabilir)
            if (createdCustomer == null)
            {
                // Sadece ID'yi döndürerek bir hata olduğunu belirt
                return Conflict("Kayıt oluşturuldu ancak tekrar okunamadı.");
            }

            // 201 Created durum kodu ile birlikte yeni oluşturulan müşterinin
            // "GetCustomerById" endpoint'inin adresini ve müşterinin kendisini döndür.
            return CreatedAtAction( 
                nameof(GetCustomerById),    // Hangi metot ile çağrılabileceği bilgisi
                new { id = newCustomerId }, // O metota ait parametreleri
                createdCustomer             // yeni oluşturulan veri
            );
        }

        // Metotun ANA SORUMLULUĞU GÜNCELLEMEK'tir (Command), ancak iş akışını (workflow) tamamlamak için Okuma (Query) sağlayıcısını da kullanır.
        // PUT: api/customer/5
        [HttpPut("{id}")] // [HttpPost("update/{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] UpdateCustomerCommand updateCustomer)
        {
            // 1. ADIM: MANUEL VALİDASYON
            // Validator'ı manuel olarak ve ASENKRON (async) olarak çağırıyoruz.
            ValidationResult validationResult = await _updateValidator.ValidateAsync(updateCustomer);

            if (!validationResult.IsValid)
            {
                // Hata varsa, 400 BadRequest ile birlikte hata listesini döndür.
                // ModelState'i kullanmak yerine doğrudan hataları döndürmek daha temizdir.
                return BadRequest(validationResult.Errors);
            }

            // 2. Kontrol: NULL CHECK
            if (updateCustomer == null) {
                return BadRequest("Müşteri bilgisi boş");
            }

            // 1. Doğrulama: URL'deki ID ile gövdedeki (body) ID eşleşiyor mu?
            if (id != updateCustomer.CustomerID)
            {
                return BadRequest("URL'deki ID ile müşteri verisindeki ID eşleşmiyor.");
            }

            // 2. Doğrulama: Bu müşteri veritabanında var mı?
            var existingCustomer = await _queryProvider.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
            {
                return NotFound($"ID'si {id} olan müşteri bulunamadı.");
            }

            // 3. Adım: Veritabanında güncelleme işlemini yap VE SONUCU YAKALA
            var affectedRows = await _commandProvider.UpdateCustomerAsync(updateCustomer);

            // 2. KONTROL (Güncelleme sonrası kontrol)
            if (affectedRows <= 0)
            {
                // Bu, 'race condition' durumunda veya veritabanında hiçbir
                // alan değişmediyse olabilir. Her iki durumda da, kaynağın
                // bulunamadığını/güncellenemediğini belirtmek en doğrusudur.
                return NotFound($"ID'si {id} olan müşteri güncellenemedi (veya zaten silinmiş olabilir).");
            }

            // 4. Adım: Kullanıcıya verinin güncellenmiş son halini döndür.
            var updatedCustomer = await _queryProvider.GetCustomerByIdAsync(id);

            return Ok(updatedCustomer); // 200 OK ve güncellenmiş nesne
        }


        // Metodun ANA SORUMLULUĞU SİLMEK'tir (Command)
        // DELETE: api/customer/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            // Provider aracılığıyla müşteriyi pasif hale getir
            // ve etkilenen satır sayısını yakala
            var affectedRows = await _commandProvider.DeactivateCustomerAsync(id);

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
