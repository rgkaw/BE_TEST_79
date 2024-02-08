namespace TEST_BE_79_RAKA.Model.DTO
{
    public class NewTransactionDTO
    {
        public int AccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public String Description { get; set; } = String.Empty;
        public char DebitCreditStatus { get; set; }

        public double Amount { get; set; }
    }
}
