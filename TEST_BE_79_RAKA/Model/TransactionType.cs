using System.ComponentModel.DataAnnotations;

namespace TEST_BE_79_RAKA.Model
{
    public class TransactionType
    {
        [Key]
        public int Id { get; set; }
        private Char[] AllowedChars = new Char[] { 'C', 'D' };
        public Char DebitCredit 
        {
            get { return this.DebitCredit; }
            set
            {
                if (!AllowedChars.Any(x => x == value))
                    throw new ArgumentException("Not valid Char");
                this.DebitCredit = value;
            }
        }
        
    }
}
