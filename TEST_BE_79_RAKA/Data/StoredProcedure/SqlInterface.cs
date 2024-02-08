using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using TEST_BE_79_RAKA.Model;

namespace TEST_BE_79_RAKA.Data.StoredProcedure
{
    public class SqlInterface 
    {
        private readonly IConfiguration _configuration;
        private readonly String _connStr;
        private SqlConnection con = null;
        public SqlInterface(IConfiguration configuration)
        {
            _configuration = configuration;
            _connStr = configuration.GetConnectionString("WindowsConnection");
            System.Console.WriteLine(_connStr);
        }

        

        


    }
}
