﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace KTSite.Models
{
    public class ChinaOrder
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public int ProductId { get; set; }
        [Required]
        public int Quantity { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DefaultValue("Now")]
        public DateTime DateOrdered { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [AllowNull]
        public DateTime? DateReceived { get; set; }

        [AllowNull]
        public int QuantityReceived { get; set; }
        [AllowNull]
        public bool IgnoreMissingQuantity { get; set; }
        [DefaultValue(false)]
        public bool ReceivedAll { get; set; }
    }
}
