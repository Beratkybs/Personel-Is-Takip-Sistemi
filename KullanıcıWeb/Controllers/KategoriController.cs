using Microsoft.AspNetCore.Mvc;
using KullanıcıWeb.Models;
using KullanıcıWeb.Services;
using System.Collections.Generic;

namespace KullanıcıWeb.Controllers
{
    public class KategoriController : Controller
    {
        private readonly IKategoriService _kategoriService;

        public KategoriController(IKategoriService kategoriService)
        {
            _kategoriService = kategoriService;
        }




        // Kategorileri listeleme ekranı
        public IActionResult Index()
        {
            var kategoriListesi = _kategoriService.GetAllKategoriler();
            return View(kategoriListesi);
        }




        // Yeni kategori ekleme
        [HttpPost]
        public IActionResult KategoriEkle(string categoryName)
        {
            _kategoriService.KategoriEkle(categoryName);
            return RedirectToAction("Index");
        }




        // Kategori silme 
        [HttpPost]
        public IActionResult KategoriSil(int id)
        {
            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }

            _kategoriService.KategoriSil(id);
            return RedirectToAction("Index");
        }
    }
}