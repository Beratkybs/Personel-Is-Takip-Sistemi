using KullanıcıWeb.Models;
using System.Collections.Generic;

namespace KullanıcıWeb.Services
{
    public interface IIsDetayService
    {
   
        (
            IsTakip gorev,
            List<Kullanici> personellListesi,
            List<Proje> projeListesi,
            List<Organizasyon> organizasyonListesi,
            List<Kategori> kategorilListesi,
            List<Durum> durumListesi,
            List<IsMesaj> mesajListesi,
            List<IsTakip> altGorevListesi

        ) GetIsDetayData(int taskId);

        void MesajEkle(int taskId, int userId, string icerik, IFormFile? gorsel);
        void mesajsil(int MesajId);
        void DetayGuncelle(int taskId, string taskTitle, string projectId, string organizationName,
                            string categoryId, string assignedUserId, string importanceLevel,
                            string priority, string durumId, int? stId, string guncelleyenKullanici, int aktifKullaniciId);
        bool AktifAltGorevVarMi(int taskId);
    }
}