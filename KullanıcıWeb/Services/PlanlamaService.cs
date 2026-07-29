using KullanıcıWeb.Models;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace KullanıcıWeb.Services
{
    public class PlanlamaService : IPlanlamaService
    {
        private readonly IConfiguration _configuration;

        public PlanlamaService(IConfiguration configuration)
        {
            _configuration = configuration;
        }


        public List<PlanlamaIs> GetAktifPlanlar() 
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            List<PlanlamaIs> planListesi = new List<PlanlamaIs>();

            string query = @"
                SELECT p.PLAN_IS_ID, 
                       p.REFERANS_TASK_ID, 
                       p.ACIKLAMA, 
                       p.PROJE_ADI, 
                       p.ORGANIZASYON, 
                       p.KATEGORI,
                       p.BILDIREN_KULLANICI_ID,
                       p.ATANAN_PERSONEL_ID,
                       p.ONEM_DERECESI,
                       p.ONCELIK,
                       p.DURUM_ID,
                       p.ACILIS_TARIHI, 
                       p.SON_BITIS_TARIHI, 
                       p.FLAG,
                       (k1.FIRST_NAME || ' ' || k1.LAST_NAME) AS BILDIREN_AD_SOYAD,
                       (k2.FIRST_NAME || ' ' || k2.LAST_NAME) AS ATANAN_PERSONEL_AD_SOYAD
                FROM HBK_PLANLAMA_ISLERI p
                LEFT JOIN HBK_KULLANICI_TABLE k1 ON p.BILDIREN_KULLANICI_ID = k1.USER_ID
                LEFT JOIN HBK_KULLANICI_TABLE k2 ON p.ATANAN_PERSONEL_ID = k2.USER_ID
                WHERE p.FLAG IN ('H', 'P')
                ORDER BY 
                    CASE WHEN p.SON_BITIS_TARIHI < TRUNC(SYSDATE) THEN 1 ELSE 0 END ASC,
                    p.SON_BITIS_TARIHI ASC";

            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand command = new OracleCommand(query, connection))
            {
                connection.Open();
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        planListesi.Add(new PlanlamaIs
                        {
                            PlanIsId = Convert.ToInt32(reader["PLAN_IS_ID"]),
                            ReferansTaskId = reader["REFERANS_TASK_ID"] != DBNull.Value ? Convert.ToInt32(reader["REFERANS_TASK_ID"]) : (int?)null,
                            Aciklama = reader["ACIKLAMA"]?.ToString() ?? "",
                            ProjeAdi = reader["PROJE_ADI"]?.ToString() ?? "-",
                            Organizasyon = reader["ORGANIZASYON"]?.ToString() ?? "-",
                            Kategori = reader["KATEGORI"]?.ToString() ?? "-",
                            BildirenKullaniciId = Convert.ToInt32(reader["BILDIREN_KULLANICI_ID"]),
                            AtananPersonelId = reader["ATANAN_PERSONEL_ID"] != DBNull.Value ? Convert.ToInt32(reader["ATANAN_PERSONEL_ID"]) : (int?)null,
                            OnemDerecesi = reader["ONEM_DERECESI"]?.ToString() ?? "Normal",
                            Oncelik = reader["ONCELIK"]?.ToString() ?? "Düşük",
                            DurumId = reader["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(reader["DURUM_ID"]) : 0,
                            AcilisTarihi = reader["ACILIS_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["ACILIS_TARIHI"]) : (DateTime?)null,
                            SonBitisTarihi = reader["SON_BITIS_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["SON_BITIS_TARIHI"]) : (DateTime?)null,
                            Flag = reader["FLAG"]?.ToString() ?? "H",

                            BildirenKullaniciAdSoyad = reader["BILDIREN_AD_SOYAD"] != DBNull.Value ? reader["BILDIREN_AD_SOYAD"].ToString() : "Bilinmeyen",
                            AtananPersonelAdSoyad = reader["ATANAN_PERSONEL_AD_SOYAD"] != DBNull.Value ? reader["ATANAN_PERSONEL_AD_SOYAD"].ToString() : "Atanmamış"
                        });
                    }
                }
            }

            return planListesi;
        }







        // Seçilen personelin takvim üzerindeki planlı iş yükünü getirir
        public List<PlanlamaIs> GetPersonelTakvim(int personelId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            List<PlanlamaIs> planListesi = new List<PlanlamaIs>();

            string query = @"
                SELECT p.PLAN_IS_ID, 
                       p.REFERANS_TASK_ID, 
                       p.ACIKLAMA, 
                       p.PROJE_ADI, 
                       p.ORGANIZASYON, 
                       p.KATEGORI,
                       p.BILDIREN_KULLANICI_ID,
                       p.ATANAN_PERSONEL_ID,
                       p.ONEM_DERECESI,
                       p.ONCELIK,
                       p.DURUM_ID,
                       p.ACILIS_TARIHI, 
                       p.SON_BITIS_TARIHI, 
                       p.FLAG,
                       (k1.FIRST_NAME || ' ' || k1.LAST_NAME) AS BILDIREN_AD_SOYAD,
                       (k2.FIRST_NAME || ' ' || k2.LAST_NAME) AS ATANAN_PERSONEL_AD_SOYAD
                FROM HBK_PLANLAMA_ISLERI p
                LEFT JOIN HBK_KULLANICI_TABLE k1 ON p.BILDIREN_KULLANICI_ID = k1.USER_ID
                LEFT JOIN HBK_KULLANICI_TABLE k2 ON p.ATANAN_PERSONEL_ID = k2.USER_ID
                WHERE p.ATANAN_PERSONEL_ID = :p_personelId 
                  AND p.SON_BITIS_TARIHI IS NOT NULL
                ORDER BY p.ACILIS_TARIHI ASC";

            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand command = new OracleCommand(query, connection))
            {
                command.BindByName = true;
                command.Parameters.Add(new OracleParameter("p_personelId", personelId));

                connection.Open();
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        planListesi.Add(new PlanlamaIs
                        {
                            PlanIsId = Convert.ToInt32(reader["PLAN_IS_ID"]),
                            ReferansTaskId = reader["REFERANS_TASK_ID"] != DBNull.Value ? Convert.ToInt32(reader["REFERANS_TASK_ID"]) : (int?)null,
                            Aciklama = reader["ACIKLAMA"]?.ToString() ?? "",
                            ProjeAdi = reader["PROJE_ADI"]?.ToString() ?? "-",
                            Organizasyon = reader["ORGANIZASYON"]?.ToString() ?? "-",
                            Kategori = reader["KATEGORI"]?.ToString() ?? "-",
                            BildirenKullaniciId = Convert.ToInt32(reader["BILDIREN_KULLANICI_ID"]),
                            AtananPersonelId = reader["ATANAN_PERSONEL_ID"] != DBNull.Value ? Convert.ToInt32(reader["ATANAN_PERSONEL_ID"]) : (int?)null,
                            OnemDerecesi = reader["ONEM_DERECESI"]?.ToString() ?? "Normal",
                            Oncelik = reader["ONCELIK"]?.ToString() ?? "Düşük",
                            DurumId = reader["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(reader["DURUM_ID"]) : 0,
                            AcilisTarihi = reader["ACILIS_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["ACILIS_TARIHI"]) : (DateTime?)null,
                            SonBitisTarihi = reader["SON_BITIS_TARIHI"] != DBNull.Value ? Convert.ToDateTime(reader["SON_BITIS_TARIHI"]) : (DateTime?)null,
                            Flag = reader["FLAG"]?.ToString() ?? "H",

                            BildirenKullaniciAdSoyad = reader["BILDIREN_AD_SOYAD"] != DBNull.Value ? reader["BILDIREN_AD_SOYAD"].ToString() : "Bilinmeyen",
                            AtananPersonelAdSoyad = reader["ATANAN_PERSONEL_AD_SOYAD"] != DBNull.Value ? reader["ATANAN_PERSONEL_AD_SOYAD"].ToString() : "Atanmamış"
                        });
                    }
                }
            }

            return planListesi;
        }










        //İş Takip'ten Tarihsiz Kopyalama
        public bool IsTakipIsiniPlanlamayaKopyala(int taskId, int bildirenKullaniciId, out string mesaj)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            string kontrolQuery = @"SELECT COUNT(*) FROM HBK_PLANLAMA_ISLERI WHERE REFERANS_TASK_ID = :p_task";


            string selectQuery = @"
        SELECT t.TASK_TITLE, t.ORGANIZATION_NAME, t.IMPORTANCE_LEVEL, t.PRIORITY, t.DURUM_ID,
               t.ASSIGNED_USER_ID,
               c.CATEGORY_NAME, p.PROJECT_NAME
        FROM HBK_IS_TAKIP_TABLE t
        LEFT JOIN HBK_KATEGORI_TABLE c ON t.CATEGORY_ID = c.CATEGORY_ID
        LEFT JOIN HBK_PROJE_TABLE p ON t.PROJECT_ID = p.PROJECT_ID
        WHERE t.TASK_ID = :p_task";

            PlanlamaIs shadowPlan = new PlanlamaIs();
            bool taskBulundu = false;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();

                using (OracleCommand kontrolCmd = new OracleCommand(kontrolQuery, connection))
                {
                    kontrolCmd.BindByName = true;
                    kontrolCmd.Parameters.Add(new OracleParameter("p_task", taskId));

                    if (Convert.ToInt32(kontrolCmd.ExecuteScalar()) > 0)
                    {
                        mesaj = $"#{taskId} numaralı iş daha önce planlamaya atanmış.";
                        return false;
                    }
                }

                using (OracleCommand selectCmd = new OracleCommand(selectQuery, connection))
                {
                    selectCmd.BindByName = true;
                    selectCmd.Parameters.Add(new OracleParameter("p_task", taskId));
                    using (OracleDataReader reader = selectCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            shadowPlan.Aciklama = reader["TASK_TITLE"]?.ToString() ?? "İsimsiz Görev";
                            shadowPlan.Organizasyon = reader["ORGANIZATION_NAME"]?.ToString() ?? "-";
                            shadowPlan.ProjeAdi = reader["PROJECT_NAME"]?.ToString() ?? "-";
                            shadowPlan.Kategori = reader["CATEGORY_NAME"]?.ToString() ?? "-";
                            shadowPlan.OnemDerecesi = reader["IMPORTANCE_LEVEL"]?.ToString() ?? "Normal";
                            shadowPlan.Oncelik = reader["PRIORITY"]?.ToString() ?? "Düşük";
                            shadowPlan.DurumId = reader["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(reader["DURUM_ID"]) : 1;
                            shadowPlan.AtananPersonelId = reader["ASSIGNED_USER_ID"] != DBNull.Value
                                ? Convert.ToInt32(reader["ASSIGNED_USER_ID"])
                                : (int?)null;
                            taskBulundu = true;
                        }
                    }
                }

                if (!taskBulundu)
                {
                    mesaj = "Orijinal iş kaydı sistemde bulunamadı.";
                    return false;
                }

                    string insertQuery = @"
                    INSERT INTO HBK_PLANLAMA_ISLERI
                    (REFERANS_TASK_ID, ACIKLAMA, PROJE_ADI, ORGANIZASYON, KATEGORI, BILDIREN_KULLANICI_ID,
                     ATANAN_PERSONEL_ID, ONEM_DERECESI, ONCELIK, DURUM_ID, ACILIS_TARIHI, SON_BITIS_TARIHI, FLAG)
                    VALUES
                    (:p_ref, :p_aciklama, :p_proje, :p_org, :p_kat, :p_bildiren,
                     :p_atanan, :p_onem, :p_oncelik, :p_durum, NULL, :p_son_bitis, 'H')";

                using (OracleCommand insertCmd = new OracleCommand(insertQuery, connection))
                {
                    insertCmd.BindByName = true;
                    insertCmd.Parameters.Add(new OracleParameter("p_ref", taskId));
                    insertCmd.Parameters.Add(new OracleParameter("p_aciklama", shadowPlan.Aciklama));
                    insertCmd.Parameters.Add(new OracleParameter("p_proje", shadowPlan.ProjeAdi));
                    insertCmd.Parameters.Add(new OracleParameter("p_org", shadowPlan.Organizasyon));
                    insertCmd.Parameters.Add(new OracleParameter("p_kat", shadowPlan.Kategori));
                    insertCmd.Parameters.Add(new OracleParameter("p_bildiren", bildirenKullaniciId));

                    insertCmd.Parameters.Add(new OracleParameter("p_atanan", OracleDbType.Int32)).Value =
                        shadowPlan.AtananPersonelId.HasValue ? (object)shadowPlan.AtananPersonelId.Value : DBNull.Value;

                    insertCmd.Parameters.Add(new OracleParameter("p_onem", shadowPlan.OnemDerecesi));
                    insertCmd.Parameters.Add(new OracleParameter("p_oncelik", shadowPlan.Oncelik));
                    insertCmd.Parameters.Add(new OracleParameter("p_durum", shadowPlan.DurumId));
                    insertCmd.Parameters.Add(new OracleParameter("p_son_bitis", OracleDbType.Date)).Value =
                    shadowPlan.SonBitisTarihi.HasValue ? (object)shadowPlan.SonBitisTarihi.Value : DBNull.Value;

                    bool basarili = insertCmd.ExecuteNonQuery() > 0;
                    mesaj = basarili
                        ? $"#{taskId} numaralı operasyonel iş başarıyla stratejik planlama listesine gölge kopya (Shadow Copy) olarak aktarıldı."
                        : "İş kopyalama esnasında bir hata oluştu.";
                    return basarili;
                }
            }
        }





        // Modaldan Direkt Planlı Görev Ekleme (Açılış Tarihi Formdan Alınacak Şekilde Güncellendi)
        public bool YeniPlanEkle(PlanlamaIs plan, DateTime? acilisTarihi, DateTime? sonBitisTarihi, int bildirenKullaniciId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            string insertQuery = @"
        INSERT INTO HBK_PLANLAMA_ISLERI 
        (REFERANS_TASK_ID, ACIKLAMA, PROJE_ADI, ORGANIZASYON, KATEGORI, BILDIREN_KULLANICI_ID, 
         ATANAN_PERSONEL_ID, ONEM_DERECESI, ONCELIK, DURUM_ID, ACILIS_TARIHI, SON_BITIS_TARIHI, FLAG) 
        VALUES 
        (NULL, :p_aciklama, :p_proje, :p_org, :p_kat, :p_bildiren, 
         :p_atanan, :p_onem, :p_oncelik, :p_durum, :p_acilis, :p_deadline, 'P')";

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (OracleCommand insertCmd = new OracleCommand(insertQuery, connection))
                {
                    insertCmd.BindByName = true;
                    insertCmd.Parameters.Add(new OracleParameter("p_aciklama", plan.Aciklama));
                    insertCmd.Parameters.Add(new OracleParameter("p_proje", plan.ProjeAdi ?? "-"));
                    insertCmd.Parameters.Add(new OracleParameter("p_org", plan.Organizasyon ?? "-"));
                    insertCmd.Parameters.Add(new OracleParameter("p_kat", plan.Kategori ?? "-"));
                    insertCmd.Parameters.Add(new OracleParameter("p_bildiren", bildirenKullaniciId));
                    insertCmd.Parameters.Add(new OracleParameter("p_atanan", plan.AtananPersonelId.HasValue ? plan.AtananPersonelId.Value : DBNull.Value));
                    insertCmd.Parameters.Add(new OracleParameter("p_onem", plan.OnemDerecesi ?? "Normal"));
                    insertCmd.Parameters.Add(new OracleParameter("p_oncelik", plan.Oncelik ?? "Düşük"));
                    insertCmd.Parameters.Add(new OracleParameter("p_durum", plan.DurumId));
                    insertCmd.Parameters.Add(new OracleParameter("p_acilis", OracleDbType.Date)).Value = acilisTarihi?.Date;
                    insertCmd.Parameters.Add(new OracleParameter("p_deadline", OracleDbType.Date)).Value = sonBitisTarihi?.Date;

                    return insertCmd.ExecuteNonQuery() > 0;
                }
            }
        }






        //Sonradan Tarih Belirleyip Planlama Metodu
        public bool PlaniAktiflestir(int planIsId, DateTime? acilisTarihi, DateTime? sonBitisTarihi)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            string updateQuery = @"
        UPDATE HBK_PLANLAMA_ISLERI 
        SET ACILIS_TARIHI = :p_acilis, 
            SON_BITIS_TARIHI = :p_deadline, 
            FLAG = 'P' 
        WHERE PLAN_IS_ID = :p_id";

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                connection.Open();
                using (OracleCommand cmd = new OracleCommand(updateQuery, connection))
                {
                    cmd.BindByName = true;
                    cmd.Parameters.Add(new OracleParameter("p_acilis", OracleDbType.Date)).Value = acilisTarihi?.Date;
                    cmd.Parameters.Add(new OracleParameter("p_deadline", OracleDbType.Date)).Value = sonBitisTarihi?.Date;
                    cmd.Parameters.Add(new OracleParameter("p_id", planIsId));

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // Plan silme
        public bool PlanSil(int planIsId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            string deleteQuery = "DELETE FROM HBK_PLANLAMA_ISLERI WHERE PLAN_IS_ID = :p_id";

            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand cmd = new OracleCommand(deleteQuery, connection))
            {
                cmd.BindByName = true;
                cmd.Parameters.Add(new OracleParameter("p_id", planIsId));
                connection.Open();

                return cmd.ExecuteNonQuery() > 0;
            }
        }
    }
}
    
