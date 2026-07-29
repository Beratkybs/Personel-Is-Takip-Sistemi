using System;

namespace KullanıcıWeb.Models
{
    public class PlanlamaIs
    {
        public int PlanIsId { get; set; }
        public string Aciklama { get; set; }
        public string ProjeAdi { get; set; }
        public string Organizasyon { get; set; }
        public string Kategori { get; set; }
        public int BildirenKullaniciId { get; set; }
        public int? AtananPersonelId { get; set; } 
        public string OnemDerecesi { get; set; }
        public string Oncelik { get; set; }
        public int DurumId { get; set; }
        public DateTime? AcilisTarihi { get; set; }
        public DateTime? SonBitisTarihi { get; set; }
        public int? ReferansTaskId { get; set; }
        public string Flag { get; set; }

        
        public string BildirenKullaniciAdSoyad { get; set; }
        public string AtananPersonelAdSoyad { get; set; }
    }
}