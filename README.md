# Personel & İş  Takip Sistemi

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
- Bir Oracle veritabanı — kurum içi ortamda çalıştırılıyorsa şirket ağına/VPN'e bağlı olmanız gerekir. Kendi bilgisayarınızda denemek isterseniz aşağıdaki Docker adımlarını kullanabilirsiniz.

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

### Şirket veritabanına erişiminiz yoksa: Docker ile local Oracle kurulumu

Bu proje bir kurum içi Oracle veritabanına bağlanacak şekilde yazılmıştır. Şirket ağına erişiminiz yoksa (örn. projeyi incelemek isteyen biriyseniz), Docker ile kendi bilgisayarınızda bir Oracle veritabanı ayağa kaldırabilirsiniz:

1. [Docker Desktop](https://www.docker.com/products/docker-desktop/) kurulu olduğundan emin olun.

2. Local bir Oracle XE (Express Edition) container'ı başlatın:
   ```bash
   docker run -d -p 1521:1521 -e ORACLE_PASSWORD=YourPassword123 --name oracle-local gvenzl/oracle-xe:21-slim
   ```
   Container'ın ayağa kalkması birkaç dakika sürebilir; loglardan `DATABASE IS READY TO USE!` mesajını bekleyin:
   ```bash
   docker logs -f oracle-local
   ```

3. `appsettings.json` içindeki bağlantı dizesini local container'a göre güncelleyin:
   ```json
   "OracleConnection": "User Id=system;Password=YourPassword123;Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=XEPDB1)));"
   ```

4. [Veritabanı Şeması](#veritabanı-şeması) bölümündeki `CREATE TABLE` sorgusunu bu local veritabanında çalıştırın (örneğin SQL Developer, DBeaver gibi bir istemciyle bağlanıp).

5. En az bir admin kullanıcısı elle eklemeniz gerekir — `USERNAME`, `PASSWORD_HASH` (SHA-256), `ROLE_ID` ve `IS_ACTIVE = 'E'` alanlarını dolduran bir `INSERT` sorgusuyla.

6. Artık `dotnet run` ile projeyi local veritabanınıza bağlı olarak çalıştırabilirsiniz.

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

Uygulama 10 tablo üzerinde çalışır. Ana tablolar aşağıda listelenmiştir.

### HBK_KULLANICI_TABLE (Kullanıcılar)

| Kolon | Açıklama |
|---|---|
| USER_ID | Birincil anahtar |
| USERNAME | Otomatik üretilen kullanıcı adı |
| EMAIL, FIRST_NAME, LAST_NAME, PHONE | Personel bilgileri |
| PASSWORD_HASH | SHA-256 hash'lenmiş şifre |
| ROLE_ID | `HBK_ROLE_TABLE`'a referans |
| ORG_ID | `HBK_ORGANIZASYON_TABLE`'a referans (opsiyonel) |
| IS_ACTIVE | `E` (aktif) / `H` (pasif) |
| FIRST_LOGIN | `E` ise ilk girişte şifre değişimi zorunlu |
| CREATED_BY | Kaydı oluşturan kullanıcı |
| CREATED_AT | Oluşturulma tarihi |
| LAST_LOGIN_DATE | Son giriş tarihi |

### HBK_ROLE_TABLE (Roller)

| Kolon | Açıklama |
|---|---|
| ROLE_ID | Birincil anahtar |
| ROLE_NAME | Rol adı |
| ROLE_CODE | Rol kodu |
| DESCRIPTION | Açıklama |

### HBK_ORGANIZASYON_TABLE (Organizasyonlar)

| Kolon | Açıklama |
|---|---|
| ORG_ID | Birincil anahtar |
| ORG_NAME | Organizasyon adı |

### HBK_KATEGORI_TABLE (Kategoriler)

| Kolon | Açıklama |
|---|---|
| CATEGORY_ID | Birincil anahtar |
| CATEGORY_NAME | Kategori adı |

### HBK_PROJE_TABLE (Projeler)

| Kolon | Açıklama |
|---|---|
| PROJECT_ID | Birincil anahtar |
| PROJECT_NAME | Proje adı |

### HBK_DURUM_TABLE (Durumlar)

| Kolon | Açıklama |
|---|---|
| DURUM_ID | Birincil anahtar |
| DURUM_NAME | Durum adı (örn. Beklemede, Devam Ediyor, Tamamlandı) |

### HBK_IS_TAKIP_TABLE (İş / Görev Takibi)

| Kolon | Açıklama |
|---|---|
| TASK_ID | Birincil anahtar |
| MASTER_TASK_ID | Üst görev (alt görevler için, opsiyonel) |
| FLAG | İş durumu bayrağı |
| TASK_TITLE | Görev başlığı |
| PROJECT_ID | `HBK_PROJE_TABLE`'a referans |
| ORGANIZATION_NAME | Organizasyon adı |
| CATEGORY_ID | `HBK_KATEGORI_TABLE`'a referans |
| DURUM_ID | `HBK_DURUM_TABLE`'a referans |
| REPORTED_BY | Görevi oluşturan kullanıcı |
| IMPORTANCE_LEVEL | Önem derecesi |
| PRIORITY | Öncelik |
| ASSIGNED_USER_ID | `HBK_KULLANICI_TABLE`'a referans (atanan personel) |
| START_DATE | Başlangıç tarihi |
| LAST_UPDATED_BY, LAST_UPDATE_DATE | Son güncelleme bilgisi |
| MAN_DAYS | Adam/gün tahmini |
| ST_ID | Alt durum/ek referans |

### HBK_PLANLAMA_ISLERI (Planlanan İşler)

| Kolon | Açıklama |
|---|---|
| PLAN_IS_ID | Birincil anahtar |
| REFERANS_TASK_ID | Onaylanınca bağlanacağı `HBK_IS_TAKIP_TABLE` kaydı (opsiyonel) |
| ACIKLAMA | Açıklama |
| PROJE_ADI, ORGANIZASYON, KATEGORI | Metin olarak saklanan seçimler |
| BILDIREN_KULLANICI_ID | Planı oluşturan kullanıcı |
| ATANAN_PERSONEL_ID | Atanan personel |
| ONEM_DERECESI, ONCELIK | Önem/öncelik |
| DURUM_ID | `HBK_DURUM_TABLE`'a referans |
| ACILIS_TARIHI, SON_BITIS_TARIHI | Tarihler |
| FLAG | Planlama durumu (`P`: planlandı, `H`: hazır vb.) |

### HBK_IS_MESAJ_TABLE (Görev Mesajları)

| Kolon | Açıklama |
|---|---|
| MESAJ_ID | Birincil anahtar |
| TASK_ID | `HBK_IS_TAKIP_TABLE`'a referans |
| USER_ID | Mesajı yazan kullanıcı |
| MESAJ_ICERIK | Mesaj metni |
| MESAJ_TARIH | Gönderim tarihi |
| GORSEL_YOLU | Ekli görsel yolu (opsiyonel) |
| SENDER_NAME | Gönderen adı (kullanıcı silinse bile korunur) |

### HBK_IS_GECMIS_TABLE (İşlem Geçmişi / Audit Log)

| Kolon | Açıklama |
|---|---|
| LOG_ID | Birincil anahtar |
| TASK_ID | `HBK_IS_TAKIP_TABLE`'a referans |
| ISLEMI_YAPAN_KULLANICI_ID | İşlemi yapan kullanıcı |
| ESKI_PERSONEL_ID, YENI_PERSONEL_ID | Personel değişikliği öncesi/sonrası |
| ISLEM_TARIHI | İşlem tarihi |

> Not: Migration dosyaları yerine bu proje ham SQL sorguları ile çalışır; tabloları kendi Oracle şemanızda elle oluşturmanız gerekir.

Tüm tabloları oluşturan `CREATE TABLE` sorguları:

```sql
CREATE TABLE HBK_ROLE_TABLE (
    ROLE_ID      NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ROLE_NAME    VARCHAR2(100) NOT NULL,
    ROLE_CODE    VARCHAR2(50),
    DESCRIPTION  VARCHAR2(300)
);

CREATE TABLE HBK_ORGANIZASYON_TABLE (
    ORG_ID    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ORG_NAME  VARCHAR2(150) NOT NULL
);

CREATE TABLE HBK_KATEGORI_TABLE (
    CATEGORY_ID    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    CATEGORY_NAME  VARCHAR2(150) NOT NULL
);

CREATE TABLE HBK_PROJE_TABLE (
    PROJECT_ID    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    PROJECT_NAME  VARCHAR2(150) NOT NULL
);

CREATE TABLE HBK_DURUM_TABLE (
    DURUM_ID    NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    DURUM_NAME  VARCHAR2(100) NOT NULL
);

CREATE TABLE HBK_KULLANICI_TABLE (
    USER_ID           NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    USERNAME          VARCHAR2(100) NOT NULL,
    EMAIL             VARCHAR2(150),
    FIRST_NAME        VARCHAR2(100),
    LAST_NAME         VARCHAR2(100),
    PHONE             VARCHAR2(20),
    PASSWORD_HASH     VARCHAR2(200) NOT NULL,
    ROLE_ID           NUMBER NOT NULL REFERENCES HBK_ROLE_TABLE(ROLE_ID),
    ORG_ID            NUMBER REFERENCES HBK_ORGANIZASYON_TABLE(ORG_ID),
    IS_ACTIVE         CHAR(1) DEFAULT 'E',
    FIRST_LOGIN       CHAR(1) DEFAULT 'E',
    CREATED_BY        VARCHAR2(100),
    CREATED_AT        TIMESTAMP DEFAULT SYSTIMESTAMP,
    LAST_LOGIN_DATE   TIMESTAMP
);

CREATE TABLE HBK_IS_TAKIP_TABLE (
    TASK_ID             NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    MASTER_TASK_ID      NUMBER,
    FLAG                CHAR(1) DEFAULT 'H',
    TASK_TITLE          VARCHAR2(300) NOT NULL,
    PROJECT_ID          NUMBER REFERENCES HBK_PROJE_TABLE(PROJECT_ID),
    ORGANIZATION_NAME   VARCHAR2(150),
    CATEGORY_ID         NUMBER REFERENCES HBK_KATEGORI_TABLE(CATEGORY_ID),
    DURUM_ID            NUMBER REFERENCES HBK_DURUM_TABLE(DURUM_ID),
    REPORTED_BY         VARCHAR2(100),
    IMPORTANCE_LEVEL    VARCHAR2(50),
    PRIORITY            VARCHAR2(50),
    ASSIGNED_USER_ID    NUMBER REFERENCES HBK_KULLANICI_TABLE(USER_ID),
    START_DATE          TIMESTAMP DEFAULT SYSTIMESTAMP,
    LAST_UPDATED_BY     VARCHAR2(100),
    LAST_UPDATE_DATE    TIMESTAMP,
    MAN_DAYS            NUMBER,
    ST_ID               NUMBER
);

CREATE TABLE HBK_PLANLAMA_ISLERI (
    PLAN_IS_ID              NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    REFERANS_TASK_ID        NUMBER REFERENCES HBK_IS_TAKIP_TABLE(TASK_ID),
    ACIKLAMA                VARCHAR2(1000),
    PROJE_ADI               VARCHAR2(150),
    ORGANIZASYON            VARCHAR2(150),
    KATEGORI                VARCHAR2(150),
    BILDIREN_KULLANICI_ID   NUMBER REFERENCES HBK_KULLANICI_TABLE(USER_ID),
    ATANAN_PERSONEL_ID      NUMBER REFERENCES HBK_KULLANICI_TABLE(USER_ID),
    ONEM_DERECESI           VARCHAR2(50),
    ONCELIK                 VARCHAR2(50),
    DURUM_ID                NUMBER REFERENCES HBK_DURUM_TABLE(DURUM_ID),
    ACILIS_TARIHI           TIMESTAMP,
    SON_BITIS_TARIHI        TIMESTAMP,
    FLAG                    CHAR(1)
);

CREATE TABLE HBK_IS_MESAJ_TABLE (
    MESAJ_ID       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    TASK_ID        NUMBER REFERENCES HBK_IS_TAKIP_TABLE(TASK_ID),
    USER_ID        NUMBER REFERENCES HBK_KULLANICI_TABLE(USER_ID),
    MESAJ_ICERIK   VARCHAR2(2000),
    MESAJ_TARIH    TIMESTAMP DEFAULT SYSTIMESTAMP,
    GORSEL_YOLU    VARCHAR2(300),
    SENDER_NAME    VARCHAR2(200)
);

CREATE TABLE HBK_IS_GECMIS_TABLE (
    LOG_ID                       NUMBER GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    TASK_ID                      NUMBER REFERENCES HBK_IS_TAKIP_TABLE(TASK_ID),
    ISLEMI_YAPAN_KULLANICI_ID    NUMBER REFERENCES HBK_KULLANICI_TABLE(USER_ID),
    ESKI_PERSONEL_ID             NUMBER,
    YENI_PERSONEL_ID             NUMBER,
    ISLEM_TARIHI                 TIMESTAMP DEFAULT SYSTIMESTAMP
);
```

> Sıralama önemli: tablolar birbirine `REFERENCES` ile bağlı olduğu için yukarıdaki sırayla (önce bağımsız tablolar, sonra onlara referans verenler) çalıştırılmalıdır.

## Güvenlik Notları

- Gerçek veritabanı bağlantı bilgileri asla repoya eklenmemelidir — `appsettings.json` `.gitignore` içindedir, `appsettings.json.example` şablon olarak sunulur.
- Şifreler düz metin olarak değil, hash olarak saklanır.
- Tüm veritabanı sorguları parametrik olarak yazılmıştır.

## Lisans

Bu proje eğitim/portföy amaçlı paylaşılmıştır.
