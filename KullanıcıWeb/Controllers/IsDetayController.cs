using Microsoft.AspNetCore.Mvc;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Http;
using System;

namespace KullanıcıWeb.Controllers
{
    public class IsDetayController : Controller
    {
        private readonly IIsDetayService _isDetayService;
        private readonly IIsTakipService _isTakipService;


        public IsDetayController(IIsDetayService isDetayService, IIsTakipService isTakipService )
        {
            _isDetayService = isDetayService;
            _isTakipService = isTakipService;
        }






        //Görev Detay Sayfası
        public IActionResult Index(int id)
        {
            var (gorev, personellListesi, projeListesi, organizasyonListesi, kategorilListesi, durumListesi, mesajListesi, altGorevListesi) = _isDetayService.GetIsDetayData(id);
            if (gorev == null)
            {
                return NotFound();
            }

            ViewBag.Durumlar = durumListesi;
            ViewBag.Kategoriler = kategorilListesi;
            ViewBag.Personeller = personellListesi;
            ViewBag.Organizasyonlar = organizasyonListesi;
            ViewBag.Projeler = projeListesi;
            ViewBag.Mesajlar = mesajListesi;
            ViewBag.SubTasks = altGorevListesi;

            ViewBag.TaskId = id;
            var sessionUserId = HttpContext.Session.GetInt32("UserId")?.ToString() ?? HttpContext.Session.GetString("UserId") ?? "";
            var sessionRoleId = HttpContext.Session.GetInt32("RoleId")?.ToString() ?? HttpContext.Session.GetString("RoleId") ?? "";

            // 🧼 Oracle/Veritabanı karakter boşluklarını temizliyoruz
            ViewBag.LoginUserId = sessionUserId.Replace("\0", "").Trim();
            ViewBag.LoginRoleId = sessionRoleId.Replace("\0", "").Trim();

            return View(gorev);
        }


        

        // Alt görev ekleme
        [HttpPost]
        public IActionResult AltGorevEkle(string taskTitle, int? projectId, string organizationName, int? categoryId,
                                      string importanceLevel, string priority, int? assignedUserId, int? stId,
                                      int masterTaskId, string reportedBy, int? durumId)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");

            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (stId <= 0) { stId = null; }


            
            _isTakipService.IsEkle(
                taskTitle,
                projectId,
                null,
                organizationName,
                categoryId,
                importanceLevel,
                priority,
                assignedUserId,
                stId,
                reportedBy,
                durumId,
                masterTaskId
            );

            TempData["Mesaj"] = "Alt görev başarıyla oluşturuldu!";
            return RedirectToAction("Index", new { id = masterTaskId });
        }






        //Görev Bilgilerini Güncelleme
        [HttpPost]
        public IActionResult Guncelle(int taskId, string taskTitle, string projectId, string organizationName,
                                      string categoryId, string assignedUserId, string importanceLevel,
                                      string priority, string durumId, int? stId, string fromMaster)
        {
            int? sessionUserId = HttpContext.Session.GetInt32("UserId");
            string aktifKullanici = HttpContext.Session.GetString("Username") ?? "SYSTEM_USER";

            // Oturum kapandıysa giriş ekranına atar
            if (sessionUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int aktifKullaniciId = sessionUserId.Value;


            int? yeniDurumId = !string.IsNullOrEmpty(durumId) ? Convert.ToInt32(durumId) : (int?)null;
            if (yeniDurumId == 5 || yeniDurumId == 6)
            {
                bool kontrol = _isDetayService.AktifAltGorevVarMi(taskId);

                if (kontrol)
                {
                    TempData["HataMesajı"] = "Bu maddeye ait tamamlanmamış aktif alt maddeler bulunmaktadır. Ana maddeyi kapatabilmek için önce alt maddeleri tamamlamalısınız!";
                    return RedirectToAction("Index", new { id = taskId, fromMaster = fromMaster == "true" ? "true" : null });
                }
            }

            _isDetayService.DetayGuncelle(taskId, taskTitle, projectId, organizationName, categoryId,
                                          assignedUserId, importanceLevel, priority, durumId, stId, aktifKullanici, aktifKullaniciId);

            TempData["Mesaj"] = "Görev başarıyla güncellendi!";
            return RedirectToAction("Index", new { id = taskId, fromMaster = fromMaster == "true" ? "true" : null });
        }



        // MesaJ ekleme
        [HttpPost]
            public IActionResult YorumEkle(int taskId, string mesajIcerik, IFormFile? gorsel, string fromMaster)
            {
                if (string.IsNullOrWhiteSpace(mesajIcerik) && gorsel == null)
                {
                    return RedirectToAction("Index", new { id = taskId });
                }
                if (string.IsNullOrWhiteSpace(mesajIcerik))
                {
                    mesajIcerik = " ";
                }

                int? sessionUserId = HttpContext.Session.GetInt32("UserId");

                if (sessionUserId == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                int aktifUserId = sessionUserId.Value;

                _isDetayService.MesajEkle(taskId, aktifUserId, mesajIcerik, gorsel);

                return RedirectToAction("Index", new
                {
                id = taskId, fromMaster = fromMaster == "true" ? "true" : null, fragment = "mesajlasmaAlani"
            });
            }


        //Mesaj silme
        [HttpPost]
        public IActionResult MesajSil(int mesajId, int taskId)
        {
            var sessionUserId = HttpContext.Session.GetInt32("UserId")?.ToString() ?? "";
            if (string.IsNullOrEmpty(sessionUserId.Replace("\0", "").Trim()))
            {
                return RedirectToAction("Login", "Account");
            }


            _isDetayService.mesajsil(mesajId);
            return RedirectToAction("Index", new { id = taskId });




        }
    }
}