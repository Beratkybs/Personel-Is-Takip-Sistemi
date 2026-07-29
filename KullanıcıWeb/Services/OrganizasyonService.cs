using System;
using System.Collections.Generic;
using Oracle.ManagedDataAccess.Client;
using Microsoft.Extensions.Configuration;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public class OrganizasyonService : IOrganizasyonService
    {
        private readonly string _connectionString;

        public OrganizasyonService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("OracleConnection")!;
        }



        // Verileri Listeleyen Method
        public List<Organizasyon> GetAllOrganizasyonlar()
        {
            List<Organizasyon> orgListesi = new List<Organizasyon>();
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                // Yeni eklenenleri en üstte görmemiz için DESC kullandık
                string query = "SELECT ORG_ID, ORG_NAME FROM HBK_ORGANIZASYON_TABLE ORDER BY ORG_ID DESC";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    connection.Open();
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orgListesi.Add(new Organizasyon
                            {
                                OrgId = Convert.ToInt32(reader["ORG_ID"]),
                                OrgName = reader["ORG_NAME"].ToString()!
                            });
                        }
                    }
                }
            }
            return orgListesi;
        }




        // Yeni organizasyon ekleme için method
        public void OrganizasyonEkle(string orgName)
        {
            if (string.IsNullOrWhiteSpace(orgName)) return;

            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "INSERT INTO HBK_ORGANIZASYON_TABLE (ORG_NAME) VALUES (:p_name)";

                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    command.Parameters.Add(new OracleParameter("p_name", orgName.Trim()));
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        // Organizasyon silmek için method
        public void OrganizasyonSil(int id)
        {
            using (OracleConnection connection = new OracleConnection(_connectionString))
            {
                string query = "DELETE FROM HBK_ORGANIZASYON_TABLE WHERE ORG_ID = :p_id";

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