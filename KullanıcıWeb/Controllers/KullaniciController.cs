using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using KullanıcıWeb.Models;
using System.Collections.Generic;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Http;

namespace KullanıcıWeb.Controllers
{
    public class KullaniciController : Controller
    {

        private readonly IKullaniciService _kullaniciService;
        private readonly IOrganizasyonService _organizasyonService;
        public KullaniciController(IKullaniciService kullaniciService, IOrganizasyonService organizasyonService)
        {
            _kullaniciService = kullaniciService;
            _organizasyonService = organizasyonService;
        }


        // Kullanıcıları Arama Filtresiyle Listeleme
        public IActionResult Index(string searchString)
        {

            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId == null)
            {
                return RedirectToAction("Login", "Account"); 
            }


            var kullaniciListesi = _kullaniciService.GetKullaniciListesi(searchString);

            ViewBag.Organizasyonlar = _organizasyonService.GetAllOrganizasyonlar();

            ViewData["CurrentFilter"] = searchString;
            return View(kullaniciListesi);
        }



        // Yeni Kullanıcı Ekleme
        [HttpPost]
        public IActionResult Ekle(string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId)
        {

            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }

            string activeUser = HttpContext.Session.GetString("Username") ?? "SYSTEM_ADMIN";

            _kullaniciService.Ekle(email, firstName, lastName, phone, isActive, roleId, orgId, activeUser);
            return RedirectToAction("Index");
        }





        // Kullanıcı Bilgilerini Güncelleme
        [HttpPost]
        public IActionResult Guncelle(int userId, string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId)
        {

            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }
            string activeUser = HttpContext.Session.GetString("Username") ?? "SYSTEM_ADMIN";


            _kullaniciService.Guncelle(userId, email, firstName, lastName, phone, isActive, roleId, orgId,activeUser);
            return RedirectToAction("Index");
        }



        // Kullanıcıyı Silme
        [HttpPost]
        public IActionResult Sil(int id)
        {

            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }


            _kullaniciService.Sil(id);
            return RedirectToAction("Index");
        }
    }
}








