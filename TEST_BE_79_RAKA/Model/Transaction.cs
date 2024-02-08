using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace TEST_BE_79_RAKA.Model
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }
        public float Amount { get; set; }

        public DateTime Date = DateTime.Now;
        public string Description { get; set; } = String.Empty;

        public TransactionType Type { get; set; } 
    }
}
