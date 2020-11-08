﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KTSite.Models
{
    public class Order
    {
        [Key]
        public long Id { get; set; }
        [Required]
        public string OrderStatus { get; set; }
        [Required]
        public int ProductId { get; set; }
        //[ForeignKey("ProductId")]
        //public Product Product { get; set; }
        public string UserNameId { get; set; }
      //  [ForeignKey("UserNameId")]
        //public ApplicationUser ApplicationUser { get; set; }
        public int StoreNameId { get; set; }
        //[ForeignKey("StoreNameId")]
        //public UserStoreName UserStoreName { get; set; }
        [Required]
        [Range(1,200)]
        public int Quantity { get; set; }
        [Required]
        public string CustName { get; set; }
        [Required]
        public string CustStreet1 { get; set; }
        public string CustStreet2 { get; set; }
        [Required]
        public string CustCity { get; set; }
        [Required]
        public string CustState { get; set; }
        [Required]
        public string CustZipCode { get; set; }
        public string CustPhone { get; set; }

        public double Cost { get; set; }

        public string Carrier { get; set; }
        public string TrackingNumber { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [DefaultValue("Now")]
        public DateTime UsDate { get; set; }
        public DateTime CreatedDate { get; set; }
        [Required]
        public bool IsAdmin { get; set; }





    }
}
