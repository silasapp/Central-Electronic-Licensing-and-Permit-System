using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ELPS.Domain.Entities
{
   public class vCompanyDirector:EntityBase

    {
       
        public int Company_Id { get; set; }

        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public int Address_Id { get; set; }

        [Required]
        [StringLength(20)]
        public string Telephone { get; set; }

        [Required]
        public int Nationality { get; set; }
        public string address_1 { get; set; }
        public string address_2 { get; set; }
        public string City { get; set; }
        public int Country_Id { get; set; }
        public int StateId { get; set; }
        public string CountryName { get; set; }
        public string StateName { get; set; }
        public string PostalCode { get; set; }

    }
}
