using KullanıcıWeb.Helpers;
using KullanıcıWeb.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;

namespace KullanıcıWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IIsTakipService _isTakipService;

        public AccountController(IConfiguration configuration, IIsTakipService isTakipService)
        {
            _configuration = configuration;
            _isTakipService = isTakipService; 
        }



        // GİRİŞ SAYFASI
        [HttpGet]
        public IActionResult Login()
        {
            // Eğer kullanıcı zaten giriş yaptıysa ana sayfaya gönder
            if (HttpContext.Session.GetInt32("UserId") != null)
            {
                return RedirectToAction("Index", "IsTakip");
            }
            return View();
        }



        //GİRİŞ İŞLEMİ
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ViewBag.Hata = "Kullanıcı adı ve şifre boş bırakılamaz!";
                return View();
            }

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            string inputHash = HashHelper.ComputeSha256Hash(password); // Girilen şifreyi hashliyoruz

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT USER_ID, USERNAME, ROLE_ID, FIRST_LOGIN, IS_ACTIVE 
                                 FROM HBK_KULLANICI_TABLE 
                                 WHERE LOWER(USERNAME) = LOWER(:p1) AND PASSWORD_HASH = :p2";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.BindByName = true;

                    command.Parameters.Add(new OracleParameter("p1", username.Trim()));
                    command.Parameters.Add(new OracleParameter("p2", inputHash));

                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string isActive = reader["IS_ACTIVE"].ToString();
                            if (isActive != "E")
                            {
                                ViewBag.Hata = "Hesabınız pasif durumdadır. Sistem yöneticisiyle görüşün.";
                                return View();
                            }

                            int dbUserId = Convert.ToInt32(reader["USER_ID"]);
                            string updateLoginQuery = "UPDATE HBK_KULLANICI_TABLE SET LAST_LOGIN_DATE = SYSTIMESTAMP WHERE USER_ID = :id";
                            using (OracleCommand updateCmd = new OracleCommand(updateLoginQuery, connection))
                            {
                                updateCmd.Parameters.Add(new OracleParameter("id", dbUserId));
                                updateCmd.ExecuteNonQuery();
                            }

                            // Kullanıcı bilgilerini Session'a yazıyoruz
                            HttpContext.Session.SetInt32("UserId", Convert.ToInt32(reader["USER_ID"]));
                            HttpContext.Session.SetString("Username", reader["USERNAME"].ToString());
                            HttpContext.Session.SetInt32("RoleId", Convert.ToInt32(reader["ROLE_ID"]));

                            string firstLogin = reader["FIRST_LOGIN"].ToString();

                            // EĞER İLK GİRİŞ İSE ŞİFRE DEĞİŞTİRME EKRANINA ZORUNLU YÖNLENDİR
                            if (firstLogin == "E")
                            {
                                return RedirectToAction("ChangePassword");
                            }

                            return RedirectToAction("Index", "Home");
                        }
                        else
                        {
                            ViewBag.Hata = "Geçersiz kullanıcı adı veya şifre!";
                            return View();
                        }
                    }
                }
            }
        }






        // ŞİFRE DEĞİŞTİRME SAYFASI
        [HttpGet]
        public IActionResult ChangePassword()
        {
            // Giriş yapmamış kullanıcı erişemez
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login");
            }
            return View();
        }





        // ŞİFRE DEĞİŞTİRME İŞLEMİ
        [HttpPost]
        public IActionResult ChangePassword(string newPassword, string confirmPassword)
        {
            int? userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            if (string.IsNullOrEmpty(newPassword) || string.IsNullOrEmpty(confirmPassword))
            {
                ViewBag.Hata = "Şifre alanları boş geçilemez!";
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ViewBag.Hata = "Yeni şifreler birbiriyle uyuşmuyor!";
                return View();
            }

            // Şifre karmaşıklık kontrolü
            if (newPassword.Length < 6)
            {
                ViewBag.Hata = "Şifreniz en az 6 karakter uzunluğunda olmalıdır!";
                return View();
            }

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            string newHash = HashHelper.ComputeSha256Hash(newPassword);

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                // Şifreyi güncelliyor ve FIRST_LOGIN değerini 'H' (Hayır) yapıyoruz
                string query = @"UPDATE HBK_KULLANICI_TABLE 
                                 SET PASSWORD_HASH = :p1, FIRST_LOGIN = 'H', LAST_LOGIN_DATE = SYSTIMESTAMP
                                 WHERE USER_ID = :p2";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p1", newHash));
                    command.Parameters.Add(new OracleParameter("p2", userId.Value));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

            TempData["BasariMesaji"] = "Şifreniz başarıyla değiştirildi! Yeni şifrenizle sisteme giriş yapabilirsiniz.";

            // Oturumu kapatıp login ekranına yönlendiriyoruz ki yeni şifreyle girsin
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }



        // PROFİL
        [HttpGet]
        public IActionResult Profilim()
        {
             
    int? loginUserId = HttpContext.Session.GetInt32("UserId");

    if (loginUserId == null)
    {
        return RedirectToAction("Login");
    }

    
   
    var kullanici = _isTakipService.GetKullaniciById(loginUserId.Value);

    if (kullanici == null)
    {
        return NotFound("Kullanıcı bilgileri veritabanında bulunamadı.");
    }
            return View(kullanici);
        }


        // GÜVENLİ ÇIKIŞ
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}