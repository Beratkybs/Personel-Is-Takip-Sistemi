using KullanıcıWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KullanıcıWeb.Services
{
    public class IsTakipService : IIsTakipService
    {

        private readonly IConfiguration _configuration;
        public IsTakipService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // sisteme ekran geldiğinde verileri getirmek için yazdığımız metot
        public (List<IsTakip> isListesi, List<Kullanici> personelListesi, List<Proje> projeListesi, List<Organizasyon> organizasyonListesi, List<Kategori> kategoriListesi, List<Durum> durumListesi) GetIndexData(string filtre, string loginUsername, int? loginUserId)
        {

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            List<IsTakip> isListesi = new List<IsTakip>();
            List<Kullanici> personelListesi = new List<Kullanici>();
            List<Proje> projeListesi = new List<Proje>();
            List<Organizasyon> organizasyonListesi = new List<Organizasyon>();
            List<Kategori> kategoriListesi = new List<Kategori>();
            List<Durum> durumListesi = new List<Durum>();


            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                // Görev havuzunu atanmış personeli ve projesini getirmek için left joın yaptığımız alandır
                string taskQuery = @"
                    SELECT t.TASK_ID,
                           t.MASTER_TASK_ID,
                           t.FLAG,
                           t.TASK_TITLE,
                           t.ASSIGNED_USER_ID, 
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
                           (k.FIRST_NAME || ' ' || k.LAST_NAME) AS ATANAN_PERSONEL_AD_SOYAD,
                           p.PROJECT_NAME AS PROJE_ADI,
                           cat.CATEGORY_NAME AS KATEGORI_ADI,
                           d.DURUM_NAME AS DURUM_ADI
                    FROM HBK_IS_TAKIP_TABLE t
                    LEFT JOIN HBK_KULLANICI_TABLE k ON t.ASSIGNED_USER_ID = k.USER_ID
                    LEFT JOIN HBK_PROJE_TABLE p ON t.PROJECT_ID = p.PROJECT_ID
                    LEFT JOIN HBK_KATEGORI_TABLE cat ON t.CATEGORY_ID = cat.CATEGORY_ID
                    LEFT JOIN HBK_DURUM_TABLE d ON t.DURUM_ID = d.DURUM_ID
                    WHERE 1=1
                    ORDER BY CASE WHEN t.FLAG = 'E' THEN 1 ELSE 0 END ASC, t.START_DATE DESC";

                // Seçim kutusunu doldurmak için aktif personelleri getiren sorgular
                string userQuery = "SELECT USER_ID, FIRST_NAME, LAST_NAME FROM HBK_KULLANICI_TABLE WHERE IS_ACTIVE = 'E' ORDER BY FIRST_NAME ASC";
                // Proje seçim kutusunu doldurmak için projeleri getiren sorgu
                string projeQuery = "SELECT PROJECT_ID, PROJECT_NAME FROM HBK_PROJE_TABLE ORDER BY PROJECT_NAME ASC";
                // Organizasyon seçim kutusunu doldurmak için organizasyonları getiren sorgu
                string orgQuery = "SELECT ORG_ID, ORG_NAME FROM HBK_ORGANIZASYON_TABLE ORDER BY ORG_NAME ASC";
                // Kategori seçim kutusunu doldurmak için kategorileri getiren sorgu
                string katQuery = "SELECT CATEGORY_ID, CATEGORY_NAME FROM HBK_KATEGORI_TABLE ORDER BY CATEGORY_NAME ASC";
                // Durum seçim kutusunu doldurmak için durumları getiren sorgu
                string durQuery = "SELECT DURUM_ID, DURUM_NAME FROM HBK_DURUM_TABLE ORDER BY DURUM_NAME ASC";

                connection.Open();

                using (OracleCommand command = new OracleCommand(taskQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {

                    //satırlar bitene kadar iş takibi verilerinin okunduğu döngü
                    while (reader.Read())
                    {
                        isListesi.Add(new IsTakip
                        {

                            TaskId = Convert.ToInt32(reader["TASK_ID"]),
                            MasterTaskId = reader["MASTER_TASK_ID"] != DBNull.Value ? Convert.ToInt32(reader["MASTER_TASK_ID"]) : (int?)null,
                            Flag = reader["FLAG"]?.ToString() ?? "H",
                            TaskTitle = reader["TASK_TITLE"].ToString()!,

                            ProjectName = reader["PROJE_ADI"]?.ToString() ?? "Projesiz İş",
                            OrganizationName = reader["ORGANIZASYON_ADI"]?.ToString() ?? "Organizasyonsuz İş",
                            AssignedUserId = reader["ASSIGNED_USER_ID"] != DBNull.Value ? Convert.ToInt32(reader["ASSIGNED_USER_ID"]) : (int?)null,
                            CategoryId = reader["CATEGORY_ID"] != DBNull.Value ? Convert.ToInt32(reader["CATEGORY_ID"]) : (int?)null,
                            CategoryName = reader["KATEGORI_ADI"]?.ToString() ?? "Kategorisiz İş",

                            DurumId = reader["DURUM_ID"] != DBNull.Value ? Convert.ToInt32(reader["DURUM_ID"]) : (int?)null,
                            DurumName = reader["DURUM_ADI"] != DBNull.Value ? reader["DURUM_ADI"].ToString() : "Durumsuz İş",


                            ReportedBy = reader["REPORTED_BY"]?.ToString() ?? "Sistem",
                            StartDate = Convert.ToDateTime(reader["START_DATE"]),
                            LastUpdatedBy = reader["LAST_UPDATED_BY"] != DBNull.Value ? reader["LAST_UPDATED_BY"].ToString() : "-",
                            LastUpdateDate = reader["LAST_UPDATE_DATE"] != DBNull.Value ? Convert.ToDateTime(reader["LAST_UPDATE_DATE"]) : (DateTime?)null,

                            ImportanceLevel = reader["IMPORTANCE_LEVEL"]?.ToString() ?? "Normal",
                            Priority = reader["PRIORITY"]?.ToString() ?? "Düşük",
                            StId = reader["ST_ID"] != DBNull.Value ? Convert.ToInt32(reader["ST_ID"]) : null,
                            AssignedUserFullName = reader["ATANAN_PERSONEL_AD_SOYAD"]?.ToString() ?? "Atanmamış"
                        });
                    }
                }
                using (OracleCommand command = new OracleCommand(userQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {

                    //satırlar bitene kadar personellerin okunduğu döngü
                    while (reader.Read())
                    {
                        personelListesi.Add(new Kullanici
                        {
                            USER_ID = Convert.ToInt32(reader["USER_ID"]),
                            FIRST_NAME = reader["FIRST_NAME"].ToString()!,
                            LAST_NAME = reader["LAST_NAME"].ToString()!
                        });
                    }
                }

                // Organizasyon seçim kutusunu doldurmak için organizasyonları getiren sorgu
                using (OracleCommand command = new OracleCommand(orgQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        organizasyonListesi.Add(new Organizasyon
                        {
                            OrgId = Convert.ToInt32(reader["ORG_ID"]),
                            OrgName = reader["ORG_NAME"].ToString()!
                        });
                    }
                }

                using (OracleCommand command = new OracleCommand(projeQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {

                    //satırlar bitene kadar projelerin okunduğu döngü
                    while (reader.Read())
                    {
                        projeListesi.Add(new Proje
                        {
                            ProjectId = Convert.ToInt32(reader["PROJECT_ID"]),
                            ProjectName = reader["PROJECT_NAME"].ToString()!
                        });
                    }
                }
                using (OracleCommand command = new OracleCommand(katQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        kategoriListesi.Add(new Kategori
                        {
                            CategoryId = Convert.ToInt32(reader["CATEGORY_ID"]),
                            CategoryName = reader["CATEGORY_NAME"].ToString()!
                        });
                    }
                }

                using (OracleCommand command = new OracleCommand(durQuery, connection))
                using (OracleDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        durumListesi.Add(new Durum
                        {
                            DurumId = Convert.ToInt32(reader["DURUM_ID"]),
                            DurumName = reader["DURUM_NAME"].ToString()!
                        });
                    }
                }

                    }
            // Filtreleme mekanizması
            List<IsTakip> filtrelenmisIsListesi = filtre switch
            {
                "aktif-hatalar" => isListesi.Where(x => x.Flag == "H").ToList(),

                "eski-hatalar" => isListesi.Where(x => x.Flag == "E").ToList(),

                "tum-hatalar" => isListesi,

                "bana-tum-atanan-islem" => isListesi
                    .Where(x => x.AssignedUserId == loginUserId)
                    .ToList(),

                "bana-atanan-islem" => isListesi
                    .Where(x => x.AssignedUserId == loginUserId && x.DurumId != 6 && x.DurumId != 5) // Örn: 4=Tamamlandı, 5=İptal
                    .ToList(),

                "prod-gunu-bekleyen" => isListesi
                    .Where(x => x.Flag == "H"
                      && x.DurumId == 9
                      && ((x.LastUpdatedBy != null && x.LastUpdatedBy.Equals(loginUsername, StringComparison.OrdinalIgnoreCase))
                      || x.AssignedUserId == loginUserId))
                      .ToList(),

                "son-bir-hafta-aksiyon" => isListesi
                    .Where(x => x.LastUpdatedBy != null
                     && x.LastUpdatedBy.Equals(loginUsername, StringComparison.OrdinalIgnoreCase)
                     && x.LastUpdateDate >= DateTime.Now.AddDays(-7))
                    .ToList(),


                "aksiyon-aldiklarim" => isListesi
                    .Where(x => (x.LastUpdatedBy != null && x.LastUpdatedBy.Equals(loginUsername, StringComparison.OrdinalIgnoreCase))
                        || (x.AssignedUserId == loginUserId))
                    .ToList(),


                "aksiyon-alinmayan" => isListesi
                    .Where(x => x.DurumId == 1 && string.IsNullOrEmpty(x.LastUpdatedBy))
                    .ToList(),

                "bana-atanan-prodsuz" => isListesi
                    .Where(x => x.AssignedUserId == loginUserId && x.Flag == "H")
                    .ToList(),

                
                "prod-iptal-tamam-haric" => isListesi
                    .Where(x => x.Flag != "E" && x.DurumId != 6 && x.DurumId != 5)
                    .ToList(),

                "prod-ve-iptal-haric" => isListesi
                    .Where(x => x.Flag != "E" && x.DurumId != 5)
                    .ToList(),

                _ => isListesi
                  .Where(x => x.AssignedUserId == loginUserId && x.DurumId != 6 && x.DurumId != 5 && x.Flag == "H")
                  .ToList()
            

        };

            



            // frontend tarafına iş listesinin gönderildiği yer
            return (filtrelenmisIsListesi, personelListesi, projeListesi, organizasyonListesi, kategoriListesi, durumListesi);

        }



        // yeni iş ekleme işlemi için yazdığımız metot

        public void IsEkle(string taskTitle, int? projectId, int? organizationId, string organizationName, int? categoryId, string importanceLevel, string priority, int? assignedUserId, int? stId, string reportedBy, int? durumId, int? masterTaskId)
        {
            if (assignedUserId <= 0) { assignedUserId = null; }
            if (projectId <= 0) { projectId = null; }
            if (categoryId <= 0) { categoryId = null; }
            if (masterTaskId <= 0) { masterTaskId = null; }



            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                // Yeni iş kaydı yapan sorgu. SYSTIMESTAMP ile o anki sistem saatini basıyor
                string insertQuery = @"
            INSERT INTO HBK_IS_TAKIP_TABLE 
            (TASK_TITLE, PROJECT_ID, ORGANIZATION_NAME, CATEGORY_ID, IMPORTANCE_LEVEL, PRIORITY, ASSIGNED_USER_ID, DURUM_ID, ST_ID, REPORTED_BY, START_DATE, MASTER_TASK_ID) 
            VALUES (:p1, :p_proj, :p3, :p4, :p5, :p6, :p7, :p10, :p8, :p9, SYSTIMESTAMP, :p_master)";

                using (OracleCommand command = new OracleCommand(insertQuery, connection))
                {
                    command.BindByName = true;

                    command.Parameters.Add(new OracleParameter("p1", taskTitle));
                    if (projectId.HasValue && projectId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p_proj", OracleDbType.Int32)).Value = projectId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p_proj", OracleDbType.Int32)).Value = DBNull.Value;
                    command.Parameters.Add(new OracleParameter("p3", organizationName ?? (object)DBNull.Value));
                    if (categoryId.HasValue && categoryId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p4", OracleDbType.Int32)).Value = categoryId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p4", OracleDbType.Int32)).Value = DBNull.Value;

                    command.Parameters.Add(new OracleParameter("p5", importanceLevel ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p6", priority ?? "Düşük"));


                    if (assignedUserId.HasValue && assignedUserId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p7", OracleDbType.Int32)).Value = assignedUserId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p7", OracleDbType.Int32)).Value = DBNull.Value;

                    command.Parameters.Add(new OracleParameter("p8", stId ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p9", !string.IsNullOrEmpty(reportedBy) ? reportedBy : "SYSTEM_ADMIN"));


                    if (durumId.HasValue && durumId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p10", OracleDbType.Int32)).Value = durumId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p10", OracleDbType.Int32)).Value = DBNull.Value;

                    if (masterTaskId.HasValue && masterTaskId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p_master", OracleDbType.Int32)).Value = masterTaskId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p_master", OracleDbType.Int32)).Value = DBNull.Value;


                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }

        // Kullanıcıyı ID üzerinden getiren metot
        public Kullanici GetKullaniciById(int userId)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            Kullanici kullanici = null;

            string query = "SELECT USER_ID, FIRST_NAME, LAST_NAME, IS_ACTIVE FROM HBK_KULLANICI_TABLE WHERE USER_ID = :p1";

            using (OracleConnection connection = new OracleConnection(connectionString))
            using (OracleCommand command = new OracleCommand(query, connection))
            {
                command.Parameters.Add(new OracleParameter("p1", userId));
                connection.Open();

                using (OracleDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        kullanici = new Kullanici
                        {
                            USER_ID = Convert.ToInt32(reader["USER_ID"]),
                            FIRST_NAME = reader["FIRST_NAME"].ToString()!,
                            LAST_NAME = reader["LAST_NAME"].ToString()!,
                            IS_ACTIVE = reader["IS_ACTIVE"]?.ToString() ?? "E"
                        };
                    }
                }
            }
            return kullanici;
        }


        // excel methodu
        public List<IsTakip> GetExcelFiltreliIsListesi(string personelId, string durum, string organizasyonName, string loginUsername, int? loginUserId)
        {

            if (string.IsNullOrEmpty(personelId) && string.IsNullOrEmpty(durum) && string.IsNullOrEmpty(organizasyonName))
            {
                return new List<IsTakip>();
            }


            var tumData = GetIndexData("tum-hatalar", loginUsername, loginUserId);

            var sorgu = tumData.isListesi.AsQueryable();

            if (!string.IsNullOrEmpty(personelId) && int.TryParse(personelId, out int parsedUserId))
            {
                sorgu = sorgu.Where(x => x.AssignedUserId == parsedUserId);
            }

            if (!string.IsNullOrEmpty(durum))
            {
                sorgu = sorgu.Where(x => x.DurumName != null && x.DurumName.Equals(durum, StringComparison.OrdinalIgnoreCase));
            }

            if (!string.IsNullOrEmpty(organizasyonName))
            {
                sorgu = sorgu.Where(x => x.OrganizationName != null && x.OrganizationName.Equals(organizasyonName, StringComparison.OrdinalIgnoreCase));
            }

            return sorgu.ToList();
        }

    }
}

