using KullanıcıWeb.Models;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace KullanıcıWeb.Controllers
{
    public class PlanlamaController : Controller
    {
        private readonly IPlanlamaService _planlamaService;
        private readonly IIsTakipService _isTakipService;

        public PlanlamaController(IPlanlamaService planlamaService, IIsTakipService isTakipService)
        {
            _planlamaService = planlamaService;
            _isTakipService = isTakipService;
        }

        // Planlama Ekranını Listeleme
        public IActionResult Index()
        {
            if (HttpContext.Session.GetInt32("RoleId") != Roller.Admin)
            {
                return Forbid();
            }

            List<PlanlamaIs> planlar = _planlamaService.GetAktifPlanlar();  

            var (isListesi, personelListesi, projeListesi, organizasyonListesi, kategoriListesi, durumListesi) =
                _isTakipService.GetIndexData(filtre: "tum-hatalar", loginUsername: "", loginUserId: null);

            ViewBag.Projeler = projeListesi;
            ViewBag.Organizasyonlar = organizasyonListesi;
            ViewBag.Kategoriler = kategoriListesi;
            ViewBag.Personeller = personelListesi;
            ViewBag.Durumlar = durumListesi;

            return View(planlar);
        }





        // Takvim sayfasını render edecek olan ana View
        public IActionResult TakvimGörünümü()
        {
            if (HttpContext.Session.GetInt32("RoleId") != Roller.Admin)
            {
                return Forbid();
            }

            var tumVeriler = _isTakipService.GetIndexData(filtre: "tum-hatalar", loginUsername: "", loginUserId: null);

            ViewBag.Personeller = tumVeriler.personelListesi;
            return View();
        }

        [HttpGet]
        public JsonResult GetPersonelTakvimData(int personelId)
        {
            try
            {
                var data = _planlamaService.GetPersonelTakvim(personelId);
                return Json(data);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }





        // İş Detay Sayfasından Kopyalama Aksiyonu
        [HttpPost]
        public IActionResult PlanlamayaKopyala(int taskId)
        {
            if (HttpContext.Session.GetInt32("RoleId") != Roller.Admin) return Forbid();

            int loginUserId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool basariliMi = _planlamaService.IsTakipIsiniPlanlamayaKopyala(taskId, loginUserId, out string mesaj);

            if (basariliMi)
                TempData["Mesaj"] = mesaj;
            else
                TempData["HataMesajı"] = mesaj;

            return RedirectToAction("Index", "IsDetay", new { id = taskId });
        }




        // Plan Ekleme Aksiyonu
        [HttpPost]
        public IActionResult YeniPlanEkle(PlanlamaIs plan, DateTime? AcilisTarihi, DateTime? SonBitisTarihi)
        {
            int bildirenKullaniciId = HttpContext.Session.GetInt32("UserId") ?? 0;

            bool sonuc = _planlamaService.YeniPlanEkle(plan, AcilisTarihi ?? DateTime.Now, SonBitisTarihi ?? DateTime.Now.AddDays(7), bildirenKullaniciId);
            if (sonuc)
                TempData["Mesaj"] = "Yeni plan başarıyla oluşturuldu.";
            else
                TempData["HataMesajı"] = "Plan eklenirken teknik bir hata oluştu.";

            return RedirectToAction("Index");
        }






        // Havuzdaki İşi Dynamic Olarak Zamanlama
        [HttpPost]
        public IActionResult PlaniPlanla(int planIsId, string acilisTarihi, string sonBitisTarihi)
        {
            try
            {
                var culture = new System.Globalization.CultureInfo("tr-TR");

                DateTime parsedAcilis = DateTime.ParseExact(acilisTarihi, "yyyy-MM-dd", culture); 
                DateTime parsedBitis = DateTime.ParseExact(sonBitisTarihi, "yyyy-MM-dd", culture);

                bool sonuc = _planlamaService.PlaniAktiflestir(planIsId, parsedAcilis, parsedBitis);
                if (sonuc)
                {
                    return Json(new { success = true });
                }

                return Json(new { success = false, message = "Planlama güncellenirken bir hata oluştu." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Tarih dönüşüm hatası: " + ex.Message });
            }
        }






        [HttpPost]
        public IActionResult PlanSil(int planIsId)
        {
            if (HttpContext.Session.GetInt32("RoleId") != Roller.Admin)
                return Json(new { success = false, message = "Yetkisiz işlem." });

            try
            {
                bool sonuc = _planlamaService.PlanSil(planIsId);

                return Json(new
                {
                    success = sonuc,
                    message = sonuc ? null : "Plan silinirken bir hata oluştu veya kayıt bulunamadı."
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Silme işlemi sırasında hata: " + ex.Message });
            }
        }
    }
}
    
