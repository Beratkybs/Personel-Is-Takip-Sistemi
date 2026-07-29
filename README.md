# Personel & İş Takip Sistemi

ASP.NET Core MVC (.NET) ile geliştirilmiş, Oracle veritabanı üzerinde çalışan bir personel yönetimi ve iş takip uygulaması. Kurum içi kullanım senaryosu için tasarlanmıştır: yöneticiler personel hesabı oluşturur, personel kendi işlerini/görevlerini takip eder.

## Özellikler

- Rol bazlı kullanıcı yönetimi (admin / personel)
- Otomatik kullanıcı adı ve ilk şifre üretimi
- İlk girişte zorunlu şifre değiştirme akışı
- SHA-256 ile şifre hashleme
- Proje / iş takibi, kategori ve organizasyon yönetimi
- Oracle veritabanı ile parametrik (SQL injection'a karşı korumalı) sorgular

## Teknoloji Yığını

- **Backend:** ASP.NET Core MVC (.NET 10)
- **Veritabanı:** Oracle (Oracle.ManagedDataAccess.Core)
- **Frontend:** Razor Views, jQuery, jQuery Validation
- **Auth:** Custom session tabanlı kimlik doğrulama (ASP.NET Identity kullanılmamıştır)

## Kurulum

### Gereksinimler

- .NET 10 SDK
- Erişim izni olan bir Oracle veritabanı (bu proje kurum içi bir veritabanına bağlanacak şekilde tasarlanmıştır)

### Adımlar

1. Depoyu klonlayın:
   ```bash
   git clone <repo-url>
   cd KullanıcıWeb
   ```

2. `appsettings.json.example` dosyasını kopyalayıp `appsettings.json` olarak yeniden adlandırın, kendi veritabanı bilgilerinizi girin:
   ```bash
   cp appsettings.json.example appsettings.json
   ```

3. Veritabanınızda gerekli tabloyu oluşturun (bkz. [Veritabanı Şeması](#veritabanı-şeması)).

4. Projeyi çalıştırın:
   ```bash
   dotnet run
   ```

5. Tarayıcıdan `http://localhost:5052` adresine gidin.

## Kullanıcı Akışı

Bu sistemde **açık kayıt (self-registration) yoktur.** Tüm hesaplar admin tarafından oluşturulur.

1. **Admin girişi** yapılır (ilk admin hesabı veritabanına elle eklenmelidir — aşağıya bakın).
2. Admin, personel ekleme ekranından yeni bir kullanıcı oluşturur (ad, soyad, e-posta, telefon, rol vb. bilgilerle).
3. Sistem otomatik olarak:
   - Kullanıcı adını `ad.soyad` formatında üretir (Türkçe karakterler sadeleştirilir, örn. `ahmet.yilmaz`)
   - İlk şifreyi `ad.soyad123` formatında oluşturur (örn. `ahmet.yilmaz123`) ve hash'leyerek kaydeder
4. Personel bu bilgilerle giriş yapar.
5. İlk girişte sistem otomatik olarak **şifre değiştirme ekranına** yönlendirir; personel yeni bir şifre belirlemeden sisteme devam edemez.

## Veritabanı Şeması

Uygulama şu ana tabloyu bekler: `HBK_KULLANICI_TABLE`

| Kolon | Açıklama |
|---|---|
| USER_ID | Birincil anahtar |
| USERNAME | Otomatik üretilen kullanıcı adı |
| EMAIL, FIRST_NAME, LAST_NAME, PHONE | Personel bilgileri |
| PASSWORD_HASH | SHA-256 hash'lenmiş şifre |
| ROLE_ID | Kullanıcının rolü |
| ORG_ID | Bağlı olduğu organizasyon (opsiyonel) |
| IS_ACTIVE | `E` (aktif) / `H` (pasif) |
| FIRST_LOGIN | `E` ise ilk girişte şifre değişimi zorunlu |
| CREATED_BY | Kaydı oluşturan kullanıcı |
| LAST_LOGIN_DATE | Son giriş tarihi |

> Not: Migration dosyaları yerine bu proje ham SQL sorguları ile çalışır; tabloyu kendi Oracle şemanızda elle oluşturmanız gerekir.

## Güvenlik Notları

- Gerçek veritabanı bağlantı bilgileri asla repoya eklenmemelidir — `appsettings.json` `.gitignore` içindedir, `appsettings.json.example` şablon olarak sunulur.
- Şifreler düz metin olarak değil, hash olarak saklanır.
- Tüm veritabanı sorguları parametrik olarak yazılmıştır.

## Lisans

Bu proje eğitim/portföy amaçlı paylaşılmıştır.
