using System.Text.Json;

namespace TEST_BE_79_RAKA.Model.DTO
{
    public class Response
    {
        public int code { get; set; }
        public string status { get; set; } = String.Empty;
        public JsonDocument message { get; set; } 
    }

    public class ErrorMessage
    {
        public ErrorMessage(string s) { this.Error = s; }
        public string Error { get; set; }
    }

}
