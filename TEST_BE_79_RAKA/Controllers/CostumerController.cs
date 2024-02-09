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
        public IActionResult Register(RegCustomerReqDTO cs)
        {

            Console.WriteLine("============================\n\n");
            if (InsertCustomer(cs)) 
            {
                return Ok();
            }
            return StatusCode(500);
        }
        [HttpPost("api/v1/transaction/register")]
        public IActionResult NewTransaction(NewTransactionDTO trs)
        {
            if (InsertTransaction(trs)) { return Ok(); }
            return StatusCode(500);
        }
        [HttpGet("api/v1/user/point/{id}")]
        public IActionResult GetCustomerPoint(int id)
        {
            var res = getCustomerPoint(id);
            if (res!=null) { return Ok(JsonSerializer.Serialize(res)); }
            return StatusCode(500);
        }

        [HttpGet("api/v1/user/report/")]
        public IActionResult GetCustomerReport(CustomerReportReqDTO req)
        {
            if(!ModelState.IsValid) { return BadRequest(); }
            List<CustomerReportDTO> res = GetCustReport(req.Id,req.DateStart,req.DateEnd);
            if(res!=null) return Ok(JsonSerializer.Serialize(res));
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
            catch (Exception e)
            {
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
                _con.Close();
                return false;
            }
        }
        public TotalPointResponseDTO getCustomerPoint(int id) 
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
                _con.Close();
                return new TotalPointResponseDTO();
            }
        }
        public  static double calcPointPulsa(double pulsa) 
        {
            double point = 0;
            double spent = 0;
            while (spent<pulsa && pulsa>=1000) 
            { 
                if (10000 < spent && spent <=30000) { point++; }
                else if(spent > 30000) { point++; point++; }
                spent += 1000;
            }
            return point;

        }
        public  static double calcPointListrik(double listrik)
        {
            double point = 0;
            double spent = 0;
            while (spent < listrik && listrik >= 2000)
            {
                if (50000 < spent && spent <= 100000) { point++; }
                else if (spent > 100000) { point++; point++; }
                spent += 2000;
            }
            return point;

        }

        public List<CustomerReportDTO> GetCustReport(int id, DateTime startTime, DateTime endTime)
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
                        TransactionDate = (DateTime)reader["TRS_DATE"],
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
                _con.Close();
                return new List<CustomerReportDTO>();
            }
        }
    }
}
