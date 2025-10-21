using CustomerApp.Models;
using FluentValidation;

namespace CustomerApp.Validation
{
    public class UpdateCustomerCommandValidator : AbstractValidator<UpdateCustomerCommand>
    {

        private readonly ICustomerQueryProvider _queryProvider;

        public UpdateCustomerCommandValidator(ICustomerQueryProvider queryProvider, ICustomerCommandProvider commandProvider)
        {
            // Gelen providerlar'ı atadık (DI)
            _queryProvider = queryProvider;

            // Müşteri Adı zorunlu ve en fazla 50 karakter
            RuleFor(customer => customer.FirstName)
                .NotEmpty().WithMessage("Müşteri adı zorunludur.")
                .MaximumLength(50).WithMessage("Ad en fazla 50 karakter olabilir.");

            // Müşteri Soyadı zorunlu ve en fazla 50 karakter
            RuleFor(customer => customer.LastName)
                .NotEmpty().WithMessage("Müşteri soyadı zorunludur. [fluent]")
                .MaximumLength(50);

            // E-posta zorunlu, geçerli formatta ve en fazla 50 karakter
            RuleFor(customer => customer.Email)
                .NotEmpty().WithMessage("E-posta adresi zorunludur.")
                .EmailAddress().WithMessage("Geçerli bir e-posta formatı giriniz.")
                .MaximumLength(5).WithMessage("E-posta en fazla 50 karakter olabilir. (Test için max 5 karakter.)");

            // Telefon numarası en fazla 20 karakter
            RuleFor(c => c.PhoneNumber)
                .MaximumLength(20).WithMessage("Telefon numarası en fazla 20 karakter olabilir.");

            // Adres en fazla 255 karakter
            RuleFor(c => c.Address)
                .MaximumLength(255).WithMessage("Adres en fazla 255 karakter olabilir.");

            // Şehir en fazla 50 karakter
            RuleFor(c => c.City)
                .MaximumLength(50).WithMessage("Şehir en fazla 50 karakter olabilir.");

            // Ülke en fazla 50 karakter
            RuleFor(c => c.Country)
                .MaximumLength(50).WithMessage("Ülke en fazla 50 karakter olabilir.");


            // ---- FLUENT VALIDATION'IN GÜCÜ BURADA BAŞLIYOR ----

            // 1. Koşullu Validasyon (Conditional Validation)
            // Bankacılık uygulaman için kritik:
            // "Eğer ülke Türkiye ise, telefon numarası zorunludur."
            When(customer => customer.Country == "Türkiye", () =>
            {
                RuleFor(customer => customer.PhoneNumber)
                    .NotEmpty().WithMessage("Ülke Türkiye ise telefon numarası zorunludur.");
            });

            // 2. Veritabanı Gerektiren Validasyon (DI Kullanımı)
            // Validator'a ICustomerProvider'ı inject edip
            // e-postanın unique olup olmadığını KONTROL EDEBİLİRİZ.)
            RuleFor(customer => customer.Email)
                .MustAsync(async (UpdateCustomerCommand customer, string email, CancellationToken cancellation) =>
                    !await _queryProvider.IsEmailTakenAsync(email, customer.CustomerID))
                .WithMessage("Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor.");


            // Alternatif olarak, yukarıdaki MustAsync yerine özel bir metot da kullanabiliriz:
            RuleFor(customer => customer.Email)
                .NotEmpty().EmailAddress()
                .MustAsync(BeUniqueEmail)
                .WithMessage("Bu e-posta adresi zaten başka bir kullanıcı tarafından kullanılıyor. [Metot ile]");

        }

        // 3. Adım: Async kural metodunu uygula
        private async Task<bool> BeUniqueEmail(UpdateCustomerCommand customer, string email, CancellationToken cancellation)
        {
            // Provider'a sor
            bool isTaken = await _queryProvider.IsEmailTakenAsync(email, customer.CustomerID);

            // Kuralın BAŞARILI olması için e-postanın ALINMAMIŞ (isTaken == false) olması gerekir
            return !isTaken;
        }
    }
}
