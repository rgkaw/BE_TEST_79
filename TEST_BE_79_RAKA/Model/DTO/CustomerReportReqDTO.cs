namespace TEST_BE_79_RAKA.Model.DTO
{
    public class CustomerReportReqDTO
    {
        public int Id { get; set; }
        public DateOnly DateStart {  get; set; }
        public DateOnly DateEnd { get; set; }
    }
}
