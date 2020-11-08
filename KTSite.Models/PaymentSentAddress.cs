using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace KTSite.Models
{
    public class PaymentSentAddress
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string UserNameId { get; set; }
        [ForeignKey("UserNameId")]
        public ApplicationUser ApplicationUser { get; set; }

        [Required]
        public string PaymentTypeAddress { get; set; }
        [Required]
        public string PaymentType { get; set; }// status Payoneer/Paypal
        public bool IsAdmin { get; set; }// Admin Address
    }
}
