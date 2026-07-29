namespace KullanıcıWeb.Models
{



    public class IsTakip
    {

        public int TaskId { get; set; }
        public int? MasterTaskId { get; set; }
        public string Flag { get; set; } = "H";                      
        public string TaskTitle { get; set; } = string.Empty;
        public int? ProjectId { get; set; }
        public string ProjectName { get; set; } = string.Empty; 
        
        public string OrganizationName { get; set; } = string.Empty;
        public int? OrganizationId { get; set; }

        public int? CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public int? DurumId { get; set; }
        public string DurumName { get; set; } = string.Empty;

        public string ReportedBy { get; set; } = "SYSTEM_ADMIN";      
        public string ImportanceLevel { get; set; } = string.Empty;    
        public string Priority { get; set; } = "Düşük";                
                   

        
        public int? AssignedUserId { get; set; }                       

        
        public string AssignedUserFullName { get; set; } = string.Empty;

        
        public DateTime StartDate { get; set; } = DateTime.Now;        
        public string? LastUpdatedBy { get; set; }                    
        public DateTime? LastUpdateDate { get; set; }                  

       
        public double? ManDays { get; set; }                           
        public int? StId { get; set; }










    }
}
