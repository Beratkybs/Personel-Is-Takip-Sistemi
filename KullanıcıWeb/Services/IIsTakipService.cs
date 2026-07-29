using System.Collections.Generic;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IIsTakipService
    {
        
        (List<IsTakip> isListesi, List<Kullanici> personelListesi, List<Proje> projeListesi, List<Organizasyon> organizasyonListesi, List<Kategori> kategoriListesi, List<Durum> durumListesi) GetIndexData(string filtre, string loginUsername, int? loginUserId);

        // Yeni bir is ekler
        void IsEkle(string taskTitle, int? projectId, int? organizationId, string organizationName, int? categoryId, string importanceLevel, string priority, int? assignedUserId, int? stId, string reportedBy, int? durumId, int? masterTaskId);

       

        Kullanici GetKullaniciById(int userId);



    
    }
}
