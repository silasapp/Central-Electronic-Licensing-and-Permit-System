using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
    public class vCompany : EntityBase
    {

        [Display(Name = "Company Email")]
        public string User_Id { get; set; }

        [Required, Display(Name = "Company Name")]
        [StringLength(100)]
        public string Name { get; set; }

        [Required, Display(Name = "Business Type")]
        [StringLength(25)]
        public string Business_Type { get; set; }
        [Display(Name = "Registered Address")]
        public int? Registered_Address_Id { get; set; }
        [Display(Name = "Operational Address")]
        public int? Operational_Address_Id { get; set; }

        [StringLength(100)]
        public string Affiliate { get; set; }

        [StringLength(50)]
        public string Nationality { get; set; }


        [StringLength(50), Display(Name = "Contact Person's First Name")]
        public string Contact_FirstName { get; set; }


        [StringLength(50), Display(Name = "Contact Person's Last Name")]
        public string Contact_LastName { get; set; }

        [Required]
        [StringLength(15), Display(Name = "Contact Person's Telephone")]
        public string Contact_Phone { get; set; }

        [Display(Name = "Year Incorporated")]
        public int? Year_Incorporated { get; set; }

        [Required, Display(Name = "Registration Number")]
        [StringLength(50)]
        public string RC_Number { get; set; }

        [StringLength(50), Display(Name = "TIN")]
        public string Tin_Number { get; set; }

        [Display(Name = "No. of Staff")]//, Range(0, 1000000)
        public int? No_Staff { get; set; }

        [Display(Name = "No. of Expertriates")]//, Range(0, 1000000)
        public int? No_Expatriate { get; set; }

        [StringLength(50), Display(Name = "Total Assets")]
        public string Total_Asset { get; set; }

        [StringLength(50), Display(Name = "Yearly Revenue")]
        public string Yearly_Revenue { get; set; }

        //public bool? Accident { get; set; }
        [Display(Name = "No. of Accident/Incident")]//, Range(0, 1000)
        public int Accident { get; set; }

        [Display(Name = "Accident/Incident Report")]
        public string Accident_Report { get; set; }

        [Display(Name = "Training Program")]
        public string Training_Program { get; set; }

        [Display(Name = "Mission & Vision")]
        public string Mission_Vision { get; set; }

        [Display(Name = "Health & Safety Environment (HSE)")]
        public string Hse { get; set; }
        public bool? HSEDoc { get; set; }
        public DateTime? Date { get; set; }
        public bool IsCompleted { get; set; }
        public string Address_1 { get; set; }
        public string address_2 { get; set; }
        public string city { get; set; }
        public int? StateId { get; set; }
        public string StateName { get; set; }
        public int? ParentCompanyId { get; set; }
        public string ParentCompanyName { get; set; }

    }
}
