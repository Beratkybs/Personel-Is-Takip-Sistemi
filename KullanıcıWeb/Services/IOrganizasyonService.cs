using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IOrganizasyonService
    {
        
        List < Organizasyon > GetAllOrganizasyonlar();

        void OrganizasyonEkle(string orgName);

        void OrganizasyonSil(int id);
    }
}
