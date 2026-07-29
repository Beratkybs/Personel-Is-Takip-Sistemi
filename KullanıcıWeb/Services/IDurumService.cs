using System.Collections.Generic;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IDurumService
    {
        List<Durum> GetAllDurumlar();
    }
}