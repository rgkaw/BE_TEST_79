using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using TEST_BE_79_RAKA.Data.StoredProcedure;
using TEST_BE_79_RAKA.Model;
using TEST_BE_79_RAKA.Model.DTO;
using System.Diagnostics.Eventing.Reader;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.PortableExecutable;
using System.Drawing;
using System.Text.Json;

namespace TEST_BE_79_RAKA.Controllers
{
    public class CostumerController : Controller//Base
    {
        private IConfiguration _config;
        private SqlConnection _con;

        
        public CostumerController(IConfiguration config)
        {
            _config = config;
            _con = new SqlConnection(_config["ConnectionStrings:WindowsConnection"]);
        }
        [HttpPost("api/v1/user/register")]
        public IActionResult Register([FromBody] RegCustomerReqDTO cs)
        {

            Console.WriteLine("============================\n\n");
            if (InsertCustomer(cs)) 
            {
                return Ok();
            }
            return StatusCode(500);
        }


        [HttpPost("api/v1/transaction/register")]
        public IActionResult NewTransaction([FromBody] NewTransactionDTO trs)
        {
            if (InsertTransaction(trs)) { return Ok(); }
            return StatusCode(500);
        }

        [HttpGet("api/v1/user/point/")]
        public IActionResult getCustPoint()
        {
            List<Customer> src = GetCustomerList();
            List<TotalPointResponseDTO> res = new List<TotalPointResponseDTO>();
            foreach (Customer c in src )
            {
                res.Add(getCustPoint(c.Id));
                
            }
            if (res != null) { return Ok(JsonSerializer.SerializeToDocument(res)); }
            return StatusCode(500);
        }
        [HttpGet("api/v1/user/point/{id}")]
        public IActionResult GetCustomerPoint(int id)
        {
            var res = getCustPoint(id);
            if (res!=null) { return Ok(JsonSerializer.SerializeToDocument(res)); }
            return StatusCode(500);
        }



        [HttpGet("api/v1/user/report/")]
        public IActionResult GetCustomerReport(CustomerReportReqDTO req)
        {
            if(!ModelState.IsValid) { return BadRequest(); }
            List<CustomerReportDTO> res = GetCustReport(req.Id,req.DateStart,req.DateEnd);
            if(res!=null) return Ok(JsonSerializer.SerializeToDocument(res));
            return StatusCode(500);
        }


        public Boolean InsertCustomer(RegCustomerReqDTO cs)
        {
            _con.Open();
            SqlCommand cmd;
            SqlTransaction trs;
            trs = _con.BeginTransaction();
            try
            {
                cmd = new SqlCommand("INS_CST", _con);
                cmd.Transaction = trs;
                cmd.Connection = _con;
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@NAME", SqlDbType.NVarChar).Value = cs.Name;
                cmd.ExecuteNonQuery();
                trs.Commit();
                _con.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                trs.Rollback();
                _con.Close();
                return false;
            }
        }
        public Boolean InsertTransaction(NewTransactionDTO trs) 
        {
            _con.Open();
            SqlCommand cmd;
            try
            {
                cmd = new SqlCommand("INS_TRS", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CST_ID", SqlDbType.Int).Value = trs.AccountId;
                cmd.Parameters.AddWithValue("@TRS_DATE", SqlDbType.Date).Value = trs.TransactionDate;
                cmd.Parameters.AddWithValue("@TRS_DSC", SqlDbType.NVarChar).Value = trs.Description;
                cmd.Parameters.AddWithValue("@TRS_AMT", SqlDbType.Decimal).Value = trs.Amount;
                cmd.Parameters.AddWithValue("@TRS_DBT_CRD", SqlDbType.Char).Value = trs.DebitCreditStatus;
                cmd.ExecuteNonQuery();
                _con.Close();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _con.Close();
                return false;
            }
        }
        public TotalPointResponseDTO getCustPoint(int id) 
        {
            _con.Open();
            SqlCommand cmd;
            try
            {
                cmd = new SqlCommand("GET_CST_TRS", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CST_ID", SqlDbType.Char).Value = id;


                SqlDataReader reader = cmd.ExecuteReader();
                double totalPoint = 0;
                while (reader.Read())
                {
                    if (reader["TRS_TYPE"].ToString().Equals("Beli Pulsa")) {totalPoint+= calcPointPulsa((double) reader["TRS_AMT"]); }
                    if (reader["TRS_TYPE"].ToString().Equals("Bayar Listrik")) { totalPoint += calcPointListrik((double) reader["TRS_AMT"]); }
                }
                _con.Close();
                
                _con.Open();
                reader = null;
                cmd = new SqlCommand("SEL_CST", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@ID", SqlDbType.Char).Value = id;
                reader = cmd.ExecuteReader();
                TotalPointResponseDTO totalPointResponse = new TotalPointResponseDTO();
                if (reader.Read()) {
                    totalPointResponse = new TotalPointResponseDTO()
                    {
                        AccountId = (int)reader["ID"],
                        Name = (string)reader["Name"],
                        Points = (int)totalPoint

                    };
                }
                 
                _con.Close();
                return totalPointResponse;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _con.Close();
                return new TotalPointResponseDTO();
            }
        }
        public static double calcPoint(double x, int limit1, int limit2, int tsh)
        {
            double point = 0;
            
            if (x > limit2) { double m = x > limit1 ? limit2 : x - limit2; point += (1 * (m / tsh)); }
            if (x > limit1) { double m = x - limit1; point += (2 * (m / tsh)); }
            return point;
        }
        public  static double calcPointPulsa(double pulsa) 
        {
            return calcPoint(pulsa, 30000,10000,1000);

        }
        public  static double calcPointListrik(double listrik)
        {
            return calcPoint(listrik,100000,50000,2000);

        }

        public List<CustomerReportDTO> GetCustReport(int id, DateOnly startTime, DateOnly endTime)
        {
            _con.Open();
            SqlCommand cmd;
            try
            {
                cmd = new SqlCommand("CST_TRS_RPT", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@CST_ID", SqlDbType.Int).Value = id;
                cmd.Parameters.AddWithValue("@DATE_START", SqlDbType.Date).Value = startTime;
                cmd.Parameters.AddWithValue("@DATE_END", SqlDbType.Date).Value = endTime;


                SqlDataReader reader = cmd.ExecuteReader();
                List<CustomerReportDTO> result = new List<CustomerReportDTO>();
                CustomerReportDTO r = new CustomerReportDTO();
                while (reader.Read())
                {
                    r = new CustomerReportDTO()
                    {
                        TransactionDate = (DateOnly)reader["TRS_DATE"],
                        Description = (String)reader["DSC"],
                        Credit = Char.Parse(reader["CRD"].ToString()),
                        Debit = Char.Parse(reader["DBT"].ToString()),
                        Amount = (Double)reader["TRS_AMT"],
                    };
                    result.Add(r);

                }
                _con.Close();
                return result;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _con.Close();
                return new List<CustomerReportDTO>();
            }
        }

        public List<Customer> GetCustomerList()
        {
            _con.Open();
            SqlDataReader reader;
            try 
            {

                SqlCommand cmd = new SqlCommand("SEL_CST", _con);
                cmd.CommandType = CommandType.StoredProcedure;
                reader = cmd.ExecuteReader();

                List<Customer> result = new List<Customer>();
                while (reader.Read())
                {
                    result.Add(new Customer()
                    {
                        Id = (int)reader["ID"],
                        Name = (String)reader["Name"]
                    });
                }
                _con.Close();
                return result;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                _con.Close();
                return new List<Customer>();
            }
           

        }
    }
}
