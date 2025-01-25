using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace KafApp.Models
{
    public class Supplier : Person, ICanFavorite
    {


        [JsonPropertyName("ConstCredit"), Column("ConstCredit")]
        public long ConstCredit { get; set; }


        [JsonPropertyName("Paid"), Column("Paid")]
        public long Paid { get; set; }

        [JsonPropertyName("IsFav"), Column("IsFav")]
        public bool IsFav { get; set; }
        [JsonIgnore, NotMapped]
        public bool IsOverDebt { get { return Credit > MaxCredit; } }


        [JsonPropertyName("MaxCredit"), Column("MaxCredit")]
        public int MaxCredit { get; set; }


        [NotMapped, JsonPropertyName("Credit")]
        public long Credit => SupplyTotal + ConstCredit - SupplierReturnTotal - Paid;

        [JsonPropertyName("SupplyTotal"), Column("SupplyTotal")]
        public long SupplyTotal { get; set; }

        [JsonPropertyName("SupplierReturnTotal"), Column("SupplierReturnTotal")]
        public long SupplierReturnTotal { get; set; }


        public List<Supply> SupplysList { get; set; }
        public List<SupplierReturn> SupplierReturnsList { get; set; }
        public List<SupplierPayment> SupplierPaymentsList { get; set; }


    }
}
