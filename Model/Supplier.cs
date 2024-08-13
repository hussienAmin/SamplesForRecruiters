using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KafApp.Models
{
    public class Supplier 
    {
 
        [Key,JsonPropertyName("Id"),Column("Id")]        
        public int Id { get; set; }
        
        
        [MaxLength(30),JsonPropertyName("Name"),Column("Name")]
        public string Name { get; set; }


        [MaxLength(11),JsonPropertyName("Mail"),Column("Mail")]
        public string? Mail { get; set; }

        [MaxLength(50),JsonPropertyName("Address"),Column("Address")]
        public string? Address { get; set; }

        [MaxLength(11),JsonPropertyName("Phone"),Column("Phone")]
        public string? Phone { get; set; }

        [MaxLength(100),JsonPropertyName("Notes"),Column("Notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("IsArchived"),Column("IsArchived")]
        public bool IsArchived { get; set; } = false;

        [JsonPropertyName("Perior"),Column("Perior")]
        public decimal Perior { get; set; }

        [JsonPropertyName("Paid"),Column("Paid")]
        public decimal Paid { get; set; }


        [JsonPropertyName("IsFav"),Column("IsFav")]
        public bool IsFav { get; set; }


        [NotMapped,JsonPropertyName("Credit")]
        public decimal Credit => Perior+SupplyTotal - SupplierReturnTotal - Paid;
       
        [JsonPropertyName("SupplyTotal"),Column("SupplyTotal")]
        public decimal SupplyTotal { get; set; }
        
        [JsonPropertyName("SupplierReturnTotal"),Column("SupplierReturnTotal")]
        public decimal SupplierReturnTotal { get; set; }


        public List<Supply> SupplysList { get; set; }
        public List<DeletedSupply> DeletedSupplysList { get; set; }
        public List<SupplierReturn> SupplierReturnsList { get; set; }
        public List<SupplierPayment> SupplierPaymentsList { get; set; }

      
    }


}
