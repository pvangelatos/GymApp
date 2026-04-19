using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace GymApp.Models
{
    public enum PaymentMethod
    {
        Cash,
        Card,
        Transfer
    }

    public class Payment
    {
        public int Id { get; set; }

        public int SubscriptionId { get; set; }

        [Required]
        public DateTime PaymentDate { get; set; } = DateTime.Today;

        [Required]
        [Precision(10, 2)]
        public decimal Amount { get; set; }

        [Required]
        public PaymentMethod PaymentMethod { get; set; }

        public bool HasReceipt { get; set; } = false;

        public string? Notes { get; set; }

        // Navigation properties
        public Subscription Subscription { get; set; } = null!;
    }
}
