using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public class DurumService : IDurumService
    {
        private readonly string _connectionString;

        public DurumService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection")!;
        }

        // Sadece formdaki seçim kutusunu doldurmak için tüm durumları çeker
        public List<Durum> GetAllDurumlar()
        {
            List<Durum> durumListesi = new List<Durum>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "SELECT DURUM_ID, DURUM_NAME FROM HBK_DURUM_TABLE ORDER BY DURUM_NAME ASC";
                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    connection.Open();
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
            }
            return durumListesi;
        }
    }
}