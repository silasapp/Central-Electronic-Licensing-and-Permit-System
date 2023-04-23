using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class vAddress:EntityBase
    {
       [Display(Name = "Address 1")]
        public string address_1 { get; set; }
       [Display(Name = "Address 2")]
        public string address_2 { get; set; }
        public string City { get; set; }
        public int Country_Id { get; set; }
        public int StateId { get; set; }
       [Display(Name = "Country")]
        public string CountryName { get; set; }
       [Display(Name = "State")]
        public string StateName { get; set; }
       [Display(Name="Post Code")]
        public string postal_code { get; set; }

        [NotMapped]
        public string Type { get; set; }

    }
}
