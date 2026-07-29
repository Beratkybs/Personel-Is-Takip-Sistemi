using System;

namespace KullanıcıWeb.Models
{

    // burası entity gibi davranı databasedeki bilgiler ialır ve rame koyarak kodumuzda işlem yapmamızı sağlar.
    public class Kullanici
    {
        public int USER_ID { get; set; }
        public string USERNAME { get; set; } = string.Empty; 
        public string EMAIL { get; set; } = string.Empty;
        public string FIRST_NAME { get; set; } = string.Empty;
        public string LAST_NAME { get; set; } = string.Empty;
        public string PHONE { get; set; } = string.Empty;
        public string IS_ACTIVE { get; set; } = "E";
        public int ROLE_ID { get; set; }
        public DateTime CREATED_AT { get; set; }
        public string FirstLogin { get; set; } = "E";
        public bool IsAdmin => ROLE_ID == Roller.Admin;
        //ROLE_TABLE tablosundan
        public string ROLE_NAME { get; set; } = string.Empty;
        public string ROLE_CODE { get; set; } = string.Empty;
        public string DESCRIPTION { get; set; } = string.Empty;

        // Oganizasyon_TABLE tablosundan
        public int? OrgId { get; set; }
        public string OrganizationName { get; set; } = string.Empty;
    }
}