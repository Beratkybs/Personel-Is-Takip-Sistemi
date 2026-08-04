using KullanıcıWeb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.IO;

namespace KullanıcıWeb.Services
{
    public class IsDetayService : IIsDetayService
    {
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public IsDetayService(IConfiguration configuration, IEmailService emailService)
        {
            _configuration = configuration;
            _emailService = emailService;
        }







        // ID göre iş detay ekranını getirme
        public (IsTakip gorev, List<Kullanici> personellListesi, List<Proje> projeListesi, List<Organizasyon> organizasyonListesi, List<Kategori> kategorilListesi, List<Durum> durumListesi, List<IsMesaj> mesajListesi, List<IsTakip> altGorevListesi) GetIsDetayData(int taskId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            IsTakip gorev = null;

            List<Kullanici> personeller = new List<Kullanici>();
            List<Proje> projeler = new List<Proje>();
            List<Organizasyon> organizasyonlar = new List<Organizasyon>();
            List<Kategori> kategoriler = new List<Kategori>();
            List<Durum> durumlar = new List<Durum>();
            List<IsMesaj> mesajlar = new List<IsMesaj>();
            List<IsTakip> altGorevler = new List<IsTakip>();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string query = @"SELECT
                       t.TASK_ID,
                       t.MASTER_TASK_ID,
                       t.FLAG,
                       t.TASK_TITLE,
                       t.ORGANIZATION_NAME AS ORGANIZASYON_ADI,
                       t.CATEGORY_ID,
                       t.DURUM_ID,
                       t.REPORTED_BY,
                       t.START_DATE,
                       t.LAST_UPDATED_BY,
                       t.LAST_UPDATE_DATE,
                       t.IMPORTANCE_LEVEL,
                       t.PRIORITY,
                       t.ST_ID,
                       t.ASSIGNED_USER_ID,
                       t.PROJECT_ID,
                       (k.FIRST_NAME || ' ' || k.LAST_NAME) AS ATANAN_PERSONEL_AD_SOYAD,
                       p.PROJECT_NAME AS PROJE_ADI,
                       cat.CATEGORY_NAME AS KATEGORI_ADI,
                       d.DURUM_NAME AS DURUM_ADI
                    FROM HBK_IS_TAKIP_TABLE t
                LEFT JOIN HBK_KULLANICI_TABLE k ON t.ASSIGNED_USER_ID = k.USER_ID
                LEFT JOIN HBK_PROJE_TABLE p ON t.PROJECT_ID = p.PROJECT_ID
                LEFT JOIN HBK_KATEGORI_TABLE cat ON t.CATEGORY_ID = cat.CATEGORY_ID
                LEFT JOIN HBK_DURUM_TABLE d ON t.DURUM_ID = d.DURUM_ID
                WHERE t.TASK_ID = :p1";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p1", taskId));

                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            gorev = new IsTakip
                            {
                                TaskId = Convert.ToInt32(reader["TASK_ID"]),
                                MasterTaskId = reader["MASTER_TASK_ID"] != DBNull.Value ? Convert.ToInt32(reader["MASTER_TASK_ID"]) : (int?)null,
                                Flag = reader["FLAG"]?.ToString() ?? "H",
                                TaskTitle = reader["TASK_TITLE"].ToString()!,

                                // Proje Bilgileri
                                ProjectId = reader["PROJECT_ID"] != DBNull.Value ? Convert.ToInt32(reader["PROJECT_ID"]) : (int?)null,
                                ProjectName = reader["PROJE_ADI"]?.ToString() ?? "Projesiz İş",

                                // Organizasyon Bilgileri
                                OrganizationName = reader["ORGANIZASYON_ADI"]?.ToString() ?? "Organizasyonsuz İş",

                                // Kategori Bilgileri
                                CategoryId = reader["CATEGORY_ID"] != DBNull.Value ? Convert.ToInt32(reader["CATEGORY_ID"]) : (int?)null,
                                CategoryName = reader["KATEGORI_ADI"]?.ToString() ?? "Kategorisiz İş",

                                // Durum Bilgileri
                                DurumId = reader["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(reader["DURUM_ID"]) : (int?)null,
                                DurumName = reader["DURUM_ADI"] != DBNull.Value ? reader["DURUM_ADI"].ToString() : "Durumsuz İş",

                                // Personel ve Tarih Bilgileri
                                ReportedBy = reader["REPORTED_BY"]?.ToString() ?? "Sistem",
                                StartDate = Convert.ToDateTime(reader["START_DATE"]),

                                AssignedUserId = reader["ASSIGNED_USER_ID"] != DBNull.Value ? Convert.ToInt32(reader["ASSIGNED_USER_ID"]) : (int?)null,
                                AssignedUserFullName = reader["ATANAN_PERSONEL_AD_SOYAD"]?.ToString() ?? "Atanmamış",

                                // Önem, Öncelik ve Log Bilgileri
                                ImportanceLevel = reader["IMPORTANCE_LEVEL"]?.ToString() ?? "Normal",
                                Priority = reader["PRIORITY"]?.ToString() ?? "Düşük",

                                LastUpdatedBy = reader["LAST_UPDATED_BY"] != DBNull.Value ? reader["LAST_UPDATED_BY"].ToString() : "-",
                                LastUpdateDate = reader["LAST_UPDATE_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["LAST_UPDATE_DATE"]) : (DateTime?)null,

                                StId = reader["ST_ID"] != DBNull.Value ? Convert.ToInt32(reader["ST_ID"]) : (int?)null
                            };
                        }
                    }
                }

                string subTaskQuery = @"SELECT
                                    t.TASK_ID,
                                    t.MASTER_TASK_ID,
                                    t.FLAG
                                    , t.TASK_TITLE,
                                    t.ORGANIZATION_NAME AS ORGANIZASYON_ADI,
                                    t.CATEGORY_ID,
                                    t.DURUM_ID,
                                    t.REPORTED_BY,
                                    t.START_DATE,
                                    t.LAST_UPDATED_BY,
                                    t.LAST_UPDATE_DATE,
                                    t.IMPORTANCE_LEVEL,
                                    t.PRIORITY,
                                    t.ST_ID,
                                    t.ASSIGNED_USER_ID,
                                    t.PROJECT_ID,
                                    (k.FIRST_NAME || ' ' || k.LAST_NAME) AS ATANAN_PERSONEL_AD_SOYAD,
                                    p.PROJECT_NAME AS PROJE_ADI,
                                    cat.CATEGORY_NAME AS KATEGORI_ADI,
                                    d.DURUM_NAME AS DURUM_ADI
                                    FROM HBK_IS_TAKIP_TABLE t 
                                    LEFT JOIN HBK_KULLANICI_TABLE k ON t.ASSIGNED_USER_ID = k.USER_ID
                                    LEFT JOIN HBK_PROJE_TABLE p ON t.PROJECT_ID = p.PROJECT_ID
                                    LEFT JOIN HBK_KATEGORI_TABLE cat ON t.CATEGORY_ID = cat.CATEGORY_ID
                                    LEFT JOIN HBK_DURUM_TABLE d ON t.DURUM_ID = d.DURUM_ID
                                    WHERE t.MASTER_TASK_ID = :p1
                                    ORDER BY CASE WHEN t.FLAG = 'H' THEN 0 ELSE 1 END ASC,
                                    t.TASK_ID DESC";
                using (OracleCommand cmdSub = new OracleCommand(subTaskQuery, connection))
                {
                    cmdSub.Parameters.Add(new OracleParameter("p1", taskId));
                    using (OracleDataReader rdrSub = cmdSub.ExecuteReader())
                    {
                        while (rdrSub.Read())
                        {
                            altGorevler.Add(new IsTakip
                            {
                                TaskId = Convert.ToInt32(rdrSub["TASK_ID"]),
                                MasterTaskId = Convert.ToInt32(rdrSub["MASTER_TASK_ID"]),
                                Flag = rdrSub["FLAG"]?.ToString() ?? "H",
                                TaskTitle = rdrSub["TASK_TITLE"].ToString()!,
                                ProjectId = rdrSub["PROJECT_ID"] != DBNull.Value ? Convert.ToInt32(rdrSub["PROJECT_ID"]) : (int?)null,
                                ProjectName = rdrSub["PROJE_ADI"]?.ToString() ?? "Projesiz İş",
                                OrganizationName = rdrSub["ORGANIZASYON_ADI"]?.ToString() ?? "Organizasyonsuz İş",
                                CategoryId = rdrSub["CATEGORY_ID"] != DBNull.Value ? Convert.ToInt32(rdrSub["CATEGORY_ID"]) : (int?)null,
                                CategoryName = rdrSub["KATEGORI_ADI"]?.ToString() ?? "Kategorisiz İş",
                                DurumId = rdrSub["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(rdrSub["DURUM_ID"]) : (int?)null,
                                DurumName = rdrSub["DURUM_ADI"]?.ToString() ?? "Durumsuz İş",

                                ReportedBy = rdrSub["REPORTED_BY"]?.ToString() ?? "Sistem",
                                StartDate = Convert.ToDateTime(rdrSub["START_DATE"]),
                                ImportanceLevel = rdrSub["IMPORTANCE_LEVEL"]?.ToString() ?? "Normal",
                                Priority = rdrSub["PRIORITY"]?.ToString() ?? "Düşük",
                                LastUpdatedBy = rdrSub["LAST_UPDATED_BY"] != DBNull.Value ? rdrSub["LAST_UPDATED_BY"].ToString() : "-",
                                LastUpdateDate = rdrSub["LAST_UPDATE_DATE"] != DBNull.Value ? Convert.ToDateTime(rdrSub["LAST_UPDATE_DATE"]) : (DateTime?)null,
                                StId = rdrSub["ST_ID"] != DBNull.Value ? Convert.ToInt32(rdrSub["ST_ID"]) : (int?)null,

                                AssignedUserFullName = rdrSub["ATANAN_PERSONEL_AD_SOYAD"]?.ToString() ?? "Atanmamış"
                            });
                        }
                    }
                }

                // Personel Listesi
                string userQuery = "SELECT USER_ID, FIRST_NAME, LAST_NAME FROM HBK_KULLANICI_TABLE";
                using (OracleCommand cmd = new OracleCommand(userQuery, connection))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        personeller.Add(new Kullanici
                        {
                            USER_ID = Convert.ToInt32(rdr["USER_ID"]),
                            FIRST_NAME = rdr["FIRST_NAME"].ToString()!,
                            LAST_NAME = rdr["LAST_NAME"].ToString()!
                        });
                    }
                }

                // Proje Listesi
                string projQuery = "SELECT PROJECT_ID, PROJECT_NAME FROM HBK_PROJE_TABLE";
                using (OracleCommand cmd = new OracleCommand(projQuery, connection))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        projeler.Add(new Proje
                        {
                            ProjectId = Convert.ToInt32(rdr["PROJECT_ID"]),
                            ProjectName = rdr["PROJECT_NAME"].ToString()!
                        });
                    }
                }

                // Organizasyon Listesi
                string orgQuery = "SELECT DISTINCT ORGANIZATION_NAME FROM HBK_IS_TAKIP_TABLE WHERE ORGANIZATION_NAME IS NOT NULL";
                using (OracleCommand cmd = new OracleCommand(orgQuery, connection))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        organizasyonlar.Add(new Organizasyon
                        {
                            OrgName = rdr["ORGANIZATION_NAME"].ToString()!
                        });
                    }
                }

                // Kategori Listesi
                string katQuery = "SELECT CATEGORY_ID, CATEGORY_NAME FROM HBK_KATEGORI_TABLE";
                using (OracleCommand cmd = new OracleCommand(katQuery, connection))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        kategoriler.Add(new Kategori
                        {
                            CategoryId = Convert.ToInt32(rdr["CATEGORY_ID"]),
                            CategoryName = rdr["CATEGORY_NAME"].ToString()!
                        });
                    }
                }

                // Durum Listesi
                string durumQuery = "SELECT DURUM_ID, DURUM_NAME FROM HBK_DURUM_TABLE";
                using (OracleCommand cmd = new OracleCommand(durumQuery, connection))
                using (OracleDataReader rdr = cmd.ExecuteReader())
                {
                    while (rdr.Read())
                    {
                        durumlar.Add(new Durum
                        {
                            DurumId = Convert.ToInt32(rdr["DURUM_ID"]),
                            DurumName = rdr["DURUM_NAME"].ToString()!
                        });
                    }
                }

                string mesajQuery = @"
                SELECT m.MESAJ_ID, 
                       m.TASK_ID, 
                       m.USER_ID, 
                       m.MESAJ_ICERIK, 
                       m.MESAJ_TARIH, 
                       m.GORSEL_YOLU,
                       COALESCE(m.SENDER_NAME, (k.FIRST_NAME || ' ' || k.LAST_NAME), 'Silinmiş Kullanıcı') AS KULLANICI_AD_SOYAD,
                       CASE WHEN k.USER_ID IS NULL THEN 0 ELSE 1 END AS IS_USER_ACTIVE,
                       'MESSAGE' AS HAREKET_TIPI
                FROM HBK_IS_MESAJ_TABLE m
                LEFT JOIN HBK_KULLANICI_TABLE k ON m.USER_ID = k.USER_ID
                WHERE m.TASK_ID = :p1

                UNION ALL

                SELECT g.LOG_ID AS MESAJ_ID,
                       g.TASK_ID,
                       g.ISLEMI_YAPAN_KULLANICI_ID AS USER_ID,
                       ( 
                         'Atanan personeli değiştirdi. ' || 
                         NVL((SELECT u2.FIRST_NAME || ' ' || u2.LAST_NAME FROM HBK_KULLANICI_TABLE u2 WHERE u2.USER_ID = g.ESKI_PERSONEL_ID), 'Silinmiş Kullanıcı') || 
                         ' -> ' || 
                         NVL((SELECT u3.FIRST_NAME || ' ' || u3.LAST_NAME FROM HBK_KULLANICI_TABLE u3 WHERE u3.USER_ID = g.YENI_PERSONEL_ID), 'Silinmiş Kullanıcı')
                       ) AS MESAJ_ICERIK,
                       g.ISLEM_TARIHI AS MESAJ_TARIH,
                       NULL AS GORSEL_YOLU,
                       NVL((SELECT u1.FIRST_NAME || ' ' || u1.LAST_NAME FROM HBK_KULLANICI_TABLE u1 WHERE u1.USER_ID = g.ISLEMI_YAPAN_KULLANICI_ID), 'Silinmiş Kullanıcı') AS KULLANICI_AD_SOYAD,
                       CASE WHEN g.ISLEMI_YAPAN_KULLANICI_ID IS NULL THEN 0 ELSE 1 END AS IS_USER_ACTIVE,
                'SYSTEM_LOG' AS HAREKET_TIPI
                FROM HBK_IS_GECMIS_TABLE g
                WHERE g.TASK_ID = :p1

                ORDER BY MESAJ_TARIH ASC";

                using (OracleCommand cmd = new OracleCommand(mesajQuery, connection))
                {
                    cmd.Parameters.Add(new OracleParameter("p1", taskId));
                    using (OracleDataReader rdr = cmd.ExecuteReader())
                    {
                        while (rdr.Read())
                        {
                            mesajlar.Add(new IsMesaj
                            {
                                MesajId = Convert.ToInt32(rdr["MESAJ_ID"]),
                                TaskId = Convert.ToInt32(rdr["TASK_ID"]),
                                UserId = rdr["USER_ID"] != DBNull.Value ? Convert.ToInt32(rdr["USER_ID"]) : (int?)null,
                                MesajIcerik = rdr["MESAJ_ICERIK"].ToString()!,
                                MesajTarih = Convert.ToDateTime(rdr["MESAJ_TARIH"]),
                                GorselYolu = rdr["GORSEL_YOLU"] != DBNull.Value ? rdr["GORSEL_YOLU"].ToString() : null,
                                KullaniciAdSoyad = rdr["KULLANICI_AD_SOYAD"].ToString()!,
                                IsUserActive = Convert.ToInt32(rdr["IS_USER_ACTIVE"]),

                                HareketTipi = rdr["HAREKET_TIPI"].ToString()!
                            });
                        }
                    }
                }

                return (gorev, personeller, projeler, organizasyonlar, kategoriler, durumlar, mesajlar, altGorevler);
            }
        }




        // Mesaj silme
        public void mesajsil(int MesajId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string deleteQuery = "DELETE FROM HBK_IS_MESAJ_TABLE WHERE MESAJ_ID = :p1";
                using (OracleCommand command = new OracleCommand(deleteQuery, connection))
                {
                    command.Parameters.Add(new OracleParameter("p1", MesajId));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }






        // Detay Güncelleme
        public void DetayGuncelle(int taskId, string taskTitle, string projectId, string organizationName,
                                   string categoryId, string assignedUserId, string importanceLevel,
                                   string priority, string durumId, int? stId, string guncelleyenKullanici, int aktifKullaniciId)
        {

            bool personelDegistiMi = false;
            int? yeniAssignedUserId = !string.IsNullOrEmpty(assignedUserId) ? Convert.ToInt32(assignedUserId) : (int?)null;
            int? yeniDurumId = !string.IsNullOrEmpty(durumId) ? Convert.ToInt32(durumId) : (int?)null;


            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                int? eskiAssignedUserId = null;
                string eskiBulQuery = $"SELECT ASSIGNED_USER_ID FROM HBK_IS_TAKIP_TABLE WHERE TASK_ID = :taskId";

                using (OracleCommand cmdEski = new OracleCommand(eskiBulQuery, connection))
                {
                    connection.Open();

                    cmdEski.Parameters.Add(new OracleParameter("taskId", taskId));
                    object res = cmdEski.ExecuteScalar();
                    if (res != null && res != DBNull.Value)
                    {
                        eskiAssignedUserId = Convert.ToInt32(res);
                    }
                }

                // Arayüzden gelen yeni atanan kullanıcı ID'si 


                string yeniFlag = (yeniDurumId == 6 || yeniDurumId == 5) ? "E" : "H";

                // Eğer eski atanan ile yeni aynı değilse log ekleme
                if (eskiAssignedUserId != yeniAssignedUserId)
                {
                    personelDegistiMi = true;
                    string gecmisInsertQuery = @"INSERT INTO HBK_IS_GECMIS_TABLE (TASK_ID, ISLEMI_YAPAN_KULLANICI_ID, ESKI_PERSONEL_ID, YENI_PERSONEL_ID, ISLEM_TARIHI)
                                        VALUES (:p_task, :p_yapan, :p_eski, :p_yeni, SYSTIMESTAMP)";

                    using (OracleCommand cmdGecmis = new OracleCommand(gecmisInsertQuery, connection))
                    {
                        cmdGecmis.Parameters.Add(new OracleParameter("p_task", taskId));
                        cmdGecmis.Parameters.Add(new OracleParameter("p_yapan", aktifKullaniciId));
                        cmdGecmis.Parameters.Add(new OracleParameter("p_eski", eskiAssignedUserId ?? (object)DBNull.Value));
                        cmdGecmis.Parameters.Add(new OracleParameter("p_yeni", yeniAssignedUserId ?? (object)DBNull.Value));

                        cmdGecmis.ExecuteNonQuery();
                    }
                }

                string updateQuery = @"UPDATE HBK_IS_TAKIP_TABLE
                                       SET TASK_TITLE = :p1, 
                                           PROJECT_ID = :p2, 
                                           ORGANIZATION_NAME = :p3, 
                                           CATEGORY_ID = :p4, 
                                           ASSIGNED_USER_ID = :p5, 
                                           IMPORTANCE_LEVEL = :p6, 
                                           PRIORITY = :p7, 
                                           DURUM_ID = :p8, 
                                           ST_ID = :p9,
                                           LAST_UPDATED_BY = :p10,
                                           FLAG = :p_flag,
                                           LAST_UPDATE_DATE = SYSTIMESTAMP
                                       WHERE TASK_ID = :p11";

                using (OracleCommand command = new OracleCommand(updateQuery, connection))
                {
                    command.BindByName = true;

                    command.Parameters.Add(new OracleParameter("p1", taskTitle));
                    command.Parameters.Add(new OracleParameter("p2", projectId ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p3", organizationName ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p4", categoryId ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p5", yeniAssignedUserId ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p6", importanceLevel));
                    command.Parameters.Add(new OracleParameter("p7", priority));
                    command.Parameters.Add(new OracleParameter("p8", durumId ?? (object)DBNull.Value));

                    if (stId.HasValue)
                        command.Parameters.Add(new OracleParameter("p9", OracleDbType.Int32)).Value = stId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p9", OracleDbType.Int32)).Value = DBNull.Value;

                    command.Parameters.Add(new OracleParameter("p10", !string.IsNullOrEmpty(guncelleyenKullanici) ? guncelleyenKullanici : "SYSTEM_USER"));
                    command.Parameters.Add(new OracleParameter("p_flag", yeniFlag));
                    command.Parameters.Add(new OracleParameter("p11", taskId));

                    
                    command.ExecuteNonQuery();
                }
            }
            if (personelDegistiMi && yeniAssignedUserId.HasValue && yeniAssignedUserId.Value > 0)
            {
               BildirimGonderArkaPlan(yeniAssignedUserId.Value, taskTitle, priority, importanceLevel, "İş Güncellendi Üstünüze İş Atandı");
            }
        }


        private void BildirimGonderArkaPlan(int userId, string taskTitle, string priority, string importanceLevel, string baslikTipi)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;


            string userQuery = "SELECT FIRST_NAME, LAST_NAME, EMAIL FROM HBK_KULLANICI_TABLE WHERE USER_ID = :p_user_id AND IS_ACTIVE = 'E'";

            string userEmail = string.Empty;
            string userFullName = string.Empty;
            bool userFound = false;

            // 1. ADIM: Veri tabanından kullanıcının e-posta ve isim bilgilerini okuyoruz
            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand command = new OracleCommand(userQuery, connection))
            {
                command.Parameters.Add(new OracleParameter("p_user_id", userId));

                try
                {
                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string firstName = reader["FIRST_NAME"]?.ToString() ?? "";
                            string lastName = reader["LAST_NAME"]?.ToString() ?? "";
                            userFullName = $"{firstName} {lastName}".Trim();

                            userEmail = reader["EMAIL"] != DBNull.Value ? reader["EMAIL"].ToString()! : string.Empty;
                            userFound = true;
                        }
                    }
                }
                catch (Exception ex)
                {

                    return;
                }

                if (userFound && !string.IsNullOrEmpty(userEmail))
                {
                    string subject = $"🔔 {baslikTipi}: {taskTitle}";

                    string htmlBody = $@"
                    <div style='font-family: Arial, sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 5px;'>
                        <h3 style='color: #2B3674;'>Merhaba {userFullName},</h3>
                        <p>İş güncellemesi yapılmış üzerinize iş atanmıştır</p>
                        <hr style='border: 0; border-top: 1px solid #eee;'/>
                        <p><b>İşlem Tipi:</b> {baslikTipi}</p>
                        <p><b>İş Başlığı:</b> {taskTitle}</p>
                        <p><b>Öncelik Durumu:</b> {priority ?? "Düşük"}</p>
                        <p><b>Önem Derecesi:</b> {importanceLevel ?? "Normal"}</p>
                        <hr style='border: 0; border-top: 1px solid #eee;'/>
                        <p>Detayları incelemek üzere sisteme giriş yapabilirsiniz.</p>
                    </div>";

                    Task.Run(async () =>
                    {
                        try
                        {
                            await _emailService.SendEmailAsync(userEmail, userFullName, subject, htmlBody);
                        }
                        catch (Exception)
                        {

                        }
                    });
                }
            }
        }






        public bool AktifAltGorevVarMi(int taskId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            int aktifAltGorevSayisi = 0;

            string countQuery = @"SELECT
                                COUNT(*) 
                                FROM HBK_IS_TAKIP_TABLE 
                                WHERE MASTER_TASK_ID = :taskId
                                AND FLAG = 'H' 
                                AND DURUM_ID NOT IN (5, 6)";

            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand command = new OracleCommand(countQuery, connection))
            {
                command.Parameters.Add(new OracleParameter("taskId", taskId));
                connection.Open();

                object result = command.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                {
                    aktifAltGorevSayisi = Convert.ToInt32(result);
                }
            }
            return aktifAltGorevSayisi > 0;
        }






        // Mesaj ekleme
        public void MesajEkle(int taskId, int userId, string icerik, IFormFile? gorsel)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            string? dbGorselYolu = null;

            if (gorsel != null && gorsel.Length > 0)
            {
                
                string benzersizDosyaAdi = Guid.NewGuid().ToString() + "_" + Path.GetFileName(gorsel.FileName);

                // Sunucudaki tam yükleme yolunu belirliyoruz (wwwroot/uploads/mesajlar)
                string yuklemeKlasoru = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "mesajlar");

                if (!Directory.Exists(yuklemeKlasoru))
                {
                    Directory.CreateDirectory(yuklemeKlasoru);
                }

                string tamDosyaYolu = Path.Combine(yuklemeKlasoru, benzersizDosyaAdi);

               
                using (var stream = new FileStream(tamDosyaYolu, FileMode.Create))
                {
                    gorsel.CopyTo(stream);
                }

              
                dbGorselYolu = "/uploads/mesajlar/" + benzersizDosyaAdi;
            }
            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                // Gorsel dosyasını kaydet ve yolu sakla
                string query = @"INSERT INTO HBK_IS_MESAJ_TABLE (TASK_ID, USER_ID, MESAJ_ICERIK, GORSEL_YOLU, SENDER_NAME) 
                        VALUES (:p1, :p2, :p3, :p4, (SELECT FIRST_NAME || ' ' || LAST_NAME FROM HBK_KULLANICI_TABLE WHERE USER_ID = :p2))";
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p1", taskId));
                    command.Parameters.Add(new OracleParameter("p2", userId));
                    command.Parameters.Add(new OracleParameter("p3", icerik));
                    command.Parameters.Add(new OracleParameter("p4", dbGorselYolu ?? (object)DBNull.Value));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
                }
            }
        }

    }
