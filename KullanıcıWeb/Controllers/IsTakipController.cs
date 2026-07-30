using KullanıcıWeb.Models;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using MiniExcelLibs;
using System.Collections.Generic;


namespace KullanıcıWeb.Controllers
{
    public class IsTakipController : Controller
    {

        private readonly IIsTakipService _isTakipService;
        public IsTakipController(IIsTakipService isTakipService)
        {
            _isTakipService = isTakipService;
        }



        // sisteme ekran geldiğinde verileri getirmek için yazdığımız metot
        public IActionResult Index(string filtre = "bana-atanan")
        {

            int? loginUserId = HttpContext.Session.GetInt32("UserId");
            string loginUsername = HttpContext.Session.GetString("Username") ?? "SYSTEM_ADMIN";


            int? loginRoleId = HttpContext.Session.GetInt32("RoleId");
            ViewBag.LoginRoleId = loginRoleId;

            var (isListesi, personelListesi, projeListesi, organizasyonListesi, kategoriListesi, durumListesi) = _isTakipService.GetIndexData(filtre, loginUsername, loginUserId);

            // seçim kutularının dolması için ViewBag'lere verileri atıyoruz
            ViewBag.Personeller = personelListesi;
            ViewBag.Projeler = projeListesi;
            ViewBag.Organizasyonlar = organizasyonListesi;
            ViewBag.Kategoriler = kategoriListesi;
            ViewBag.Durumlar = durumListesi;

            ViewBag.AktifFiltre = filtre;

            // frontend tarafına iş listesinin gönderildiği yer
            return View(isListesi);
        }




        // yeni iş ekleme işlemi için yazdığımız metot
        [HttpPost]
        public IActionResult IsEkle(string taskTitle, int? projectId, int? organizationId, string organizationName, int? categoryId, string importanceLevel, string priority, int? assignedUserId, int? stId, string reportedBy, int? durumId, int? masterTaskId)
        {
            _isTakipService.IsEkle(taskTitle, projectId, organizationId, organizationName, categoryId, importanceLevel, priority, assignedUserId, stId, reportedBy, durumId, masterTaskId);
            return RedirectToAction("Index");
        }




        // excel rapor önizlemsi
        [HttpGet]
        public JsonResult ExcelOnizlemeVerisi(string personelId, string durum, string organizasyonName)
        {
            string loginUsername = HttpContext.Session.GetString("UserName") ?? User.Identity?.Name ?? "SYSTEM_ADMIN";
            int? loginUserId = HttpContext.Session.GetInt32("UserId");
            var filtrelenmisListe = _isTakipService.GetExcelFiltreliIsListesi(personelId, durum, organizasyonName, loginUsername, loginUserId);

            var sonuclar = filtrelenmisListe.Select(x => new
            {
                taskId = x.TaskId,
                masterTaskId = x.MasterTaskId,
                flag = x.Flag,
                taskTitle = x.TaskTitle,
                projectName = x.ProjectName,
                organizationName = x.OrganizationName,
                categoryName = x.CategoryName,
                reportedBy = x.ReportedBy,
                startDate = x.StartDate,
                startDateFormated = x.StartDate.ToString("dd.MM.yyyy HH:mm"),
                assignedUserFullName = x.AssignedUserFullName,
                importanceLevel = x.ImportanceLevel,
                priority = x.Priority,
                durumName = x.DurumName
            }).ToList();

            return Json(sonuclar);
        }



        // excel raporunu indirme 
        [HttpGet]
        public IActionResult ExcelRaporIndir(string personelId, string durum, string organizasyonName)
        {
            try
            {
                string loginUsername = HttpContext.Session.GetString("Username") ?? "SYSTEM_ADMIN";
                int? loginUserId = HttpContext.Session.GetInt32("UserId");

                var filtrelenmisListe = _isTakipService.GetExcelFiltreliIsListesi(personelId, durum, organizasyonName, loginUsername, loginUserId);

                var excelData = filtrelenmisListe.Select(x => {
                int gecenGun = (DateTime.Now - x.StartDate).Days;
                if (gecenGun < 0) gecenGun = 0;

                    return new
                    {
                        İş_No = x.TaskId,
                        Üst_İş_No = x.MasterTaskId.HasValue ? x.MasterTaskId.Value.ToString() : "-",
                        Durum_Tipi = x.Flag == "E" ? "Eski" : "Aktif",
                        Madde_Açıklaması = x.TaskTitle,
                        Proje_Adı = x.ProjectName,
                        Organizasyon = x.OrganizationName,
                        Kategori = x.CategoryName,
                        Bildiren = x.ReportedBy,
                        Açılış_Tarihi = x.StartDate.ToString("dd.MM.yyyy HH:mm"),
                        Geçen_Gün_Sayısı = gecenGun + " Gün",
                        Atanan_Personel = x.AssignedUserFullName,
                        Önem_Derecesi = x.ImportanceLevel,
                        Öncelik = x.Priority,
                        Durum = x.DurumName
                    };
                }).ToList();

                using (var stream = new MemoryStream())
                {
                    stream.SaveAs(excelData);
                    stream.Position = 0;

                    string dosyaAdi = $"Is_Takip_Raporu_{DateTime.Now:yyyyMMdd_HHmm}.xlsx";
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", dosyaAdi);
                }
            }
            catch (Exception ex)
            {
                TempData["HataMesaji"] = "Excel dökümü oluşturulurken bir hata meydana geldi: " + ex.Message;
                return RedirectToAction("Index");
            }
        }



    }
}
