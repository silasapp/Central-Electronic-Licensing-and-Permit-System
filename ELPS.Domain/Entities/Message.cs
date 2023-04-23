namespace ELPS.Domain.Entities
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    

    //[Table("message")]
    public partial class Message:EntityBase
    {

        [Required]
        [StringLength(250)]
        public string Subject { get; set; }

        [Required]
        public string Content { get; set; }

        public int Read { get; set; }

        public int Company_Id { get; set; }

        [Display(Name="Sender")]
        public string Sender_Id { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime Date { get; set; }
    }
}
