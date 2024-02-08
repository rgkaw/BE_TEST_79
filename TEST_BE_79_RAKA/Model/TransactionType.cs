using System.ComponentModel.DataAnnotations;

namespace TEST_BE_79_RAKA.Model
{
    public class TransactionType
    {
        [Key]
        public int Id { get; set; }
        public Char DebitCredit { get; set; }
        
    }
}
