using KullanıcıWeb.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace KullanıcıWeb.Services
{
    public class ProjeService : IProjeService
    {

        private readonly IConfiguration _configuration;
        public ProjeService(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        // sisteme ekran geldiğinde verileri getirmek için yazdığımız metot
        public List<Proje> GetProjeListesi(string searchString)
        {

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;
            List<Proje> projeListesi = new List<Proje>();

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string query = "SELECT PROJECT_ID, PROJECT_NAME FROM HBK_PROJE_TABLE";

                if (!string.IsNullOrEmpty(searchString))
                {

                    query += " WHERE LOWER(PROJECT_NAME) LIKE :search";
                }

                query += " ORDER BY PROJECT_NAME ASC";


                using (OracleCommand command = new OracleCommand(query, connection))
                {
                    if (!string.IsNullOrEmpty(searchString))
                    {
                        command.Parameters.Add(new OracleParameter("search", $"%{searchString.ToLower()}%"));
                    }
                    connection.Open();




                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        //   satırlar bitene kadar verinin okunduğu döngü
                        while (reader.Read())
                        {
                            projeListesi.Add(new Proje
                            {

                                ProjectId = Convert.ToInt32(reader["PROJECT_ID"]),
                                ProjectName = reader["PROJECT_NAME"].ToString()!
                            });
                        }
                    }

                }
            }

            

            return (projeListesi);
        }




        // yeni proje ekleme işlemi için yazdığımız metot
        public void ProjeEkle(string projectName)
        {

            if (string.IsNullOrWhiteSpace(projectName))
            {
                return;
            }

            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {

                string insertQuery = @"INSERT INTO HBK_PROJE_TABLE (PROJECT_NAME) VALUES (:p1)";

                using (OracleCommand command = new OracleCommand(insertQuery, connection))
                {

                    command.Parameters.Add(new OracleParameter("p1", projectName.Trim()));


                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }



        // proje silme işlemi için yazdığımız metot
        public void ProjeSil(int id)
        {
            string connectionString = _configuration.GetConnectionString("OracleConnection")!;

            using (OracleConnection connection = new OracleConnection(connectionString))
            {
                string deleteQuery = "DELETE FROM HBK_PROJE_TABLE WHERE PROJECT_ID = :p_id";



                using (OracleCommand command = new OracleCommand(deleteQuery, connection))
                {

                    command.Parameters.Add(new OracleParameter("p_id", id));

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }

         
        }
    }
}
