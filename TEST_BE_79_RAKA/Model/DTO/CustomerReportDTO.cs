namespace TEST_BE_79_RAKA.Model.DTO
{
    public class CustomerReportDTO
    {
        public String TransactionDate { get; set; } = DateTime.Now.ToShortDateString();
        public String Description { get; set; } = String.Empty;
        public Char Credit { get; set; }
        public Char Debit { get; set; }
        public Double Amount { get; set; }

    }
}
