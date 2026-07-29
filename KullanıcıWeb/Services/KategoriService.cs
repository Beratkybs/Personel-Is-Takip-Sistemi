using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public class KategoriService : IKategoriService
    {
        private readonly string _connectionString;

        public KategoriService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection")!;
        }





        // Listeleme methodu
        public List<Kategori> GetAllKategoriler()
        {
            List<Kategori> kategoriListesi = new List<Kategori>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "SELECT CATEGORY_ID, CATEGORY_NAME FROM HBK_KATEGORI_TABLE ORDER BY CATEGORY_ID DESC";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    connection.Open();
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
                }
            }
            return kategoriListesi;
        }





        // Yeni kategori ekleme methodu
        public void KategoriEkle(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName)) return;

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "INSERT INTO HBK_KATEGORI_TABLE (CATEGORY_NAME) VALUES (:p_name)";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p_name", categoryName.Trim()));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }




        // Kategori silme methodu
        public void KategoriSil(int id)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "DELETE FROM HBK_KATEGORI_TABLE WHERE CATEGORY_ID = :p_id";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p_id", id));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}