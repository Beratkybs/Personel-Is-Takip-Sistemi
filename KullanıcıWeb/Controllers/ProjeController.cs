using KullanıcıWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using KullanıcıWeb.Services;

namespace KullanıcıWeb.Controllers

{
    public class ProjeController : Controller
    {

        private readonly IProjeService _projeService;
        public ProjeController(IProjeService projeService)
        {
            _projeService = projeService;
        }

        // sisteme ekran geldiğinde verileri getirmek için yazdığımız metot
        public IActionResult Index(string searchString)
        {
            var projeListesi = _projeService.GetProjeListesi(searchString);
            ViewData["CurrentFilter"] = searchString;

            return View(projeListesi);

        }




        // yeni proje ekleme işlemi için yazdığımız metot
        [HttpPost]
        public IActionResult ProjeEkle(string projectName)
        {
            _projeService.ProjeEkle(projectName);
            return RedirectToAction("Index");


        }



        // proje silme işlemi için yazdığımız metot
        [HttpPost]
        public IActionResult ProjeSil(int id)
        {
            var activeRoleId = HttpContext.Session.GetInt32("RoleId");
            if (activeRoleId != Roller.Admin)
            {
                TempData["HataMesaji"] = "Bu işlemi gerçekleştirmek için yetkiniz bulunmamaktadır!";
                return RedirectToAction("Index");
            }

            _projeService.ProjeSil(id);
            return RedirectToAction("Index");
        }
    }
}
    
    
                






 