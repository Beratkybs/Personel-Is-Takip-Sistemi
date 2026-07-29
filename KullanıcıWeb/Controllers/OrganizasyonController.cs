using Microsoft.AspNetCore.Mvc;
using KullanıcıWeb.Models;
using KullanıcıWeb.Services;
using System.Collections.Generic;

namespace KullanıcıWeb.Controllers
{
    public class OrganizasyonController : Controller
    {
        private readonly IOrganizasyonService _organizasyonService;

        public OrganizasyonController(IOrganizasyonService organizasyonService)
        {
            _organizasyonService = organizasyonService;
        }





        // Organizasyonları listeleme ekranı
        public IActionResult Index()
        {
            var orgListesi = _organizasyonService.GetAllOrganizasyonlar();
            return View(orgListesi);
        }



        // Yeni organizasyon ekleme
        [HttpPost]
        public IActionResult OrganizasyonEkle(string orgName)
        {
            _organizasyonService.OrganizasyonEkle(orgName);
            return RedirectToAction("Index");
        }




        // Organizasyon silme
        [HttpPost]
        public IActionResult OrganizasyonSil(int id)
        {
            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }

            _organizasyonService.OrganizasyonSil(id);
            return RedirectToAction("Index");
        }
    }
}