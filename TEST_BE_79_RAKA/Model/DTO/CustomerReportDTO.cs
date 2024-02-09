namespace TEST_BE_79_RAKA.Model.DTO
{
    public class CustomerReportDTO
    {
        public DateTime TransactionDate { get; set; }
        public String Description { get; set; }
        public Char Credit { get; set; }
        public Char Debit { get; set; }
        public Double Amount { get; set; }

    }
}
