using System.Collections.Generic;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IKullaniciService
    {
        List<Kullanici> GetKullaniciListesi(string searchString);

        // Yeni bir kullanıcı ekler
        void Ekle(string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId, string ekleyenKullanici);

        // Mevcut kullanıcı bilgilerini günceller
        void Guncelle(int userId, string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId, string guncelleyenKullanici);

        // Kullanıcıyı siler
        void Sil(int id);
    }
}
