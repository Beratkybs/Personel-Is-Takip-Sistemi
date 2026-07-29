using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IProjeService
    {
        List<Proje> GetProjeListesi(string searchString);

        void ProjeEkle(string projectName);

        void ProjeSil(int id);
    }
}
