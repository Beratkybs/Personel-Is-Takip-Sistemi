using System.Collections.Generic;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IKategoriService
    {
        
        List<Kategori> GetAllKategoriler();

        void KategoriEkle(string categoryName);

        void KategoriSil(int id);
    }
}