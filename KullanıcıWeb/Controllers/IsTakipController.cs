using KullanıcıWeb.Models;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
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



    }
}
