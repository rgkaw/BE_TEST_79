using System.Text.Json;
using TEST_BE_79_RAKA.Model.DTO;

namespace TEST_BE_79_RAKA.Model
{
    public static class Utils
    {
        public static JsonDocument CreateResponse(Response res)

        {
            return JsonSerializer.SerializeToDocument(res);
        }

        public static JsonDocument Serialize<T>(T obj)
        {
            return JsonSerializer.SerializeToDocument(obj);

        }
    }
}
