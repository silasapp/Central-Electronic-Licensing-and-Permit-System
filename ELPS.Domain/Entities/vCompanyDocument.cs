namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("address")]
   
    public partial class vCategoryDocument : EntityBase
    {

        public string Name { get; set; }
        public int Category_Id { get; set; }
    }
}
