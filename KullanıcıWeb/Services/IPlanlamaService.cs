using System;
using System.Collections.Generic;
using KullanıcıWeb.Models;

namespace KullanıcıWeb.Services
{
    public interface IPlanlamaService
    {
        List<PlanlamaIs> GetAktifPlanlar();

        bool IsTakipIsiniPlanlamayaKopyala(int taskId, int bildirenKullaniciId, out string mesaj);
        bool YeniPlanEkle(PlanlamaIs plan, DateTime? acilisTarihi, DateTime? sonBitisTarihi, int bildirenKullaniciId);
        bool PlaniAktiflestir(int planIsId, DateTime? acilisTarihi, DateTime? sonBitisTarihi);
        bool PlanSil(int planIsId);
        List<PlanlamaIs> GetPersonelTakvim(int personelId);

    }
}