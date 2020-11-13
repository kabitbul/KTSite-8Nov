﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KTSite.Models
{
    public class Complaints
    {
        [Key]
        public long Id { get; set; }
        public long? OrderId { get; set; }
        [Required]
        public string UserNameId { get; set; }
        [ForeignKey("UserNameId")]
        public ApplicationUser ApplicationUser { get; set; }
        [Required]
        public string Description { get; set; }
        public string SolutionDesc { get; set; }
        public string HandledBy { get; set; }
        [DefaultValue(false)]
        public bool Solved { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DefaultValue("Now")]
        public DateTime CreatedDate { get; set; }
        public string NewTrackingNumber { get; set; }
        [DefaultValue(false)]
        public bool IsAdmin { get; set; }
        
        public int StoreId { get; set; }
    }
}
