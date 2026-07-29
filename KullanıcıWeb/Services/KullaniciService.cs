using KullanıcıWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using KullanıcıWeb.Helpers;


namespace KullanıcıWeb.Services
{
    public class KullaniciService : IKullaniciService
    {
        private readonly IConfiguration _configuration;
        public KullaniciService(IConfiguration configuration) 
        {
            _configuration = configuration;
        }








        // sisteme ekran geldiğinde verileri getirmek için yazdığımız metot
        public List<Kullanici> GetKullaniciListesi(string searchString)
        {
            List<Kullanici> kullaniciListesi = new List<Kullanici>();
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                // ınner joın yaptığımız alandır
                string query = @"SELECT k.USER_ID, k.USERNAME, k.EMAIL, k.FIRST_NAME, k.LAST_NAME, 
                                k.PHONE, k.IS_ACTIVE, k.ROLE_ID, k.CREATED_AT, k.ORG_ID,
                                r.ROLE_NAME, r.ROLE_CODE, r.DESCRIPTION,
                                o.ORG_NAME AS ORGANIZASYON_ADI 
                            FROM HBK_KULLANICI_TABLE k
                            INNER JOIN HBK_ROLE_TABLE r ON k.ROLE_ID = r.ROLE_ID
                            LEFT JOIN HBK_ORGANIZASYON_TABLE o ON k.ORG_ID = o.ORG_ID";

                if (!string.IsNullOrEmpty(searchString))
                {
                    query += " WHERE LOWER(k.USERNAME) LIKE :search OR LOWER(k.FIRST_NAME) LIKE :search OR LOWER(k.LAST_NAME) LIKE :search";
                }

                query += " ORDER BY k.USER_ID DESC";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        command.Parameters.Add(new OracleParameter("search", $"%{searchString.ToLower()}%"));
                    }

                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        //satırlar bitene kadar verinin okunduğu döngü
                        while (reader.Read())
                        {
                            //oluşturduğumuz listenin içine verielrin kaydeldiği yer
                            kullaniciListesi.Add(new Kullanici
                            {
                                USER_ID = Convert.ToInt32(reader["USER_ID"]),
                                USERNAME = reader["USERNAME"].ToString(),
                                EMAIL = reader["EMAIL"] != DBNull.Value ? reader["EMAIL"].ToString() : string.Empty,
                                FIRST_NAME = reader["FIRST_NAME"] != DBNull.Value ? reader["FIRST_NAME"].ToString() : string.Empty,
                                LAST_NAME = reader["LAST_NAME"] != DBNull.Value ? reader["LAST_NAME"].ToString() : string.Empty,
                                PHONE = reader["PHONE"] != DBNull.Value ? reader["PHONE"].ToString() : string.Empty,
                                IS_ACTIVE = reader["IS_ACTIVE"].ToString(),
                                ROLE_ID = Convert.ToInt32(reader["ROLE_ID"]),
                                CREATED_AT = Convert.ToDateTime(reader["CREATED_AT"]),
                                ROLE_NAME = reader["ROLE_NAME"].ToString(),
                                ROLE_CODE = reader["ROLE_CODE"].ToString(),
                                DESCRIPTION = reader["DESCRIPTION"] != DBNull.Value ? reader["DESCRIPTION"].ToString() : "Gorev tanimi yuklenemedi.",
                                OrgId = reader["ORG_ID"] != DBNull.Value ? Convert.ToInt32(reader["ORG_ID"]) : (int?)null,
                                OrganizationName = reader["ORGANIZASYON_ADI"] != DBNull.Value ? reader["ORGANIZASYON_ADI"].ToString() : "Organizasyonsuz"

                            });
                        }
                    }
                }
            }

          
            // frontend tarafına verilerin gönderildiği yer
            return kullaniciListesi;
        }









        // ekleme işlemi için yazdığımız metot
        public void Ekle(string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId, string ekleyenKullanici)
        {
            // kullanıcı adı oluşturmak için isim ve soyisim alanlarının boş olup olmadığını kontrol ediyor
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            {
                return;
            }


            string temizIsim = firstName.Trim().ToLower()
                                .Replace("ç", "c")
                                .Replace("ğ", "g")
                                .Replace("ı", "i")
                                .Replace("ö", "o")
                                .Replace("ş", "s")
                                .Replace("ü", "u");

            string temizSoyisim = lastName.Trim().ToLower()
                                   .Replace("ç", "c")
                                   .Replace("ğ", "g")
                                   .Replace("ı", "i")
                                   .Replace("ö", "o")
                                   .Replace("ş", "s")
                                   .Replace("ü", "u");

            // "isim.soyisim" formatını oluşturuyor
            string otomatikUsername = $"{temizIsim}.{temizSoyisim}";

            //şifre hashlenmesi
            string defaultSifre = $"{otomatikUsername}123";
            string sifreHashli = HashHelper.ComputeSha256Hash(defaultSifre);

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                string insertQuery = @"INSERT INTO HBK_KULLANICI_TABLE 
                               (USERNAME, EMAIL, FIRST_NAME, LAST_NAME, PHONE, IS_ACTIVE, ROLE_ID, ORG_ID, CREATED_BY, PASSWORD_HASH, FIRST_LOGIN) 
                               VALUES (:p1, :p2, :p3, :p4, :p5, :p6, :p7, :p9, :p8, :p10, :p11)";

                using (OracleCommand command = new OracleCommand(insertQuery, connection))
                {
                    command.BindByName = true;

                    // SQL Injection önlemi için tam parametrik backend yapısı
                    command.Parameters.Add(new OracleParameter("p1", otomatikUsername));
                    command.Parameters.Add(new OracleParameter("p2", email ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p3", firstName ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p4", lastName ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p5", phone ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p6", isActive)); // Seçim kutusundan 'E' veya 'H' gelecek
                    command.Parameters.Add(new OracleParameter("p7", roleId));   // Seçim kutusundan seçilen Rolün ID'si gelecek

                    if (orgId.HasValue && orgId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p9", OracleDbType.Int32)).Value = orgId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p9", OracleDbType.Int32)).Value = DBNull.Value;

                    command.Parameters.Add(new OracleParameter("p8", ekleyenKullanici ?? "SYSTEM_ADMIN")); // Audit Log: Ekleyen kişi şimdilik el ile sabit

                    command.Parameters.Add(new OracleParameter("p10", sifreHashli));
                    command.Parameters.Add(new OracleParameter("p11", "E")); // Yeni kullanıcı için ilk giriş zorunlu şifre değişimi aktif

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }







        // veri silme işlemi için yazdığımız metot
        public void Sil(int id)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                string deleteQuery = "DELETE FROM HBK_KULLANICI_TABLE WHERE USER_ID = :p1";

                using (OracleCommand command = new OracleCommand(deleteQuery, connection))
                {
                    command.Parameters.Add(new OracleParameter("p1", id));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }








        // veriyi güncelleme işlemi için yazdığımız metot
        public void Guncelle(int userId, string email, string firstName, string lastName, string phone, string isActive, int roleId, int? orgId, string guncelleyenKullanici)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                string updateQuery = @"UPDATE HBK_KULLANICI_TABLE 
                       SET EMAIL = :p1, 
                           FIRST_NAME = :p2, 
                           LAST_NAME = :p3, 
                           PHONE = :p4, 
                           IS_ACTIVE = :p5, 
                           ROLE_ID = :p6,
                            ORG_ID = :p8,
                           UPDATED_AT = SYSTIMESTAMP,
                           UPDATED_BY = :p10
                       WHERE USER_ID = :p7";

                using (OracleCommand command = new OracleCommand(updateQuery, connection))
                {
                    command.BindByName = true;

                    command.Parameters.Add(new OracleParameter("p1", email ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p2", firstName ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p3", lastName ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p4", phone ?? (object)DBNull.Value));
                    command.Parameters.Add(new OracleParameter("p5", isActive));
                    command.Parameters.Add(new OracleParameter("p6", roleId));
                    command.Parameters.Add(new OracleParameter("p7", userId));

                    if (orgId.HasValue && orgId.Value > 0)
                        command.Parameters.Add(new OracleParameter("p8", OracleDbType.Int32)).Value = orgId.Value;
                    else
                        command.Parameters.Add(new OracleParameter("p8", OracleDbType.Int32)).Value = DBNull.Value;

                    command.Parameters.Add(new OracleParameter("p10", guncelleyenKullanici ?? "SYSTEM_ADMIN"));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

        }
    }
}
