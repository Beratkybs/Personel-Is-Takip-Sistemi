using System;

namespace KullanıcıWeb.Models
{
    public class IsMesaj
    {
        public int MesajId { get; set; }
        public int TaskId { get; set; }
        public int? UserId { get; set; }
        public string MesajIcerik { get; set; }
        public string KullaniciAdSoyad { get; set; }
        public DateTime MesajTarih { get; set; }
        public string? GorselYolu { get; set; }
        public string? SenderName { get; set; }
        public int? IsUserActive { get; set; }



        public string HareketTipi { get; set; } = "MESSAGE";
    }
}