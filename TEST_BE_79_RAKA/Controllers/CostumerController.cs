﻿using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using TEST_BE_79_RAKA.Model;
using TEST_BE_79_RAKA.Model.DTO;
using System.Text.Json;
using Swashbuckle.AspNetCore.Annotations;

namespace TEST_BE_79_RAKA.Controllers
{
    public class CostumerController : Controller//Base
    {
        private IConfiguration _config;
        private SqlConnection _con;

        //Constructor
        public CostumerController(IConfiguration config)
        {
            _config = config;
            _con = new SqlConnection(_config["ConnectionStrings:WindowsConnection"]);
        }
        [HttpPost("api/v1/user/register")]
        [SwaggerOperation(Summary = "API for Registering new Customer, only receive NAME, while ID is generated by database")]
        public IActionResult Register([FromBody] RegCustomerReqDTO cs)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 400,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Invalid Data"))
                }
                ));
            }
            Customer res = InsertCustomer(cs);
            if (res.Id == -1) { return BadRequest(); }
            else if (InsertCustomer(cs).Id!=0) 
            {
                return Ok(JsonSerializer.SerializeToDocument(
                    new Response()
                    {
                        code = 200,
                        status = "Success",
                        message =JsonSerializer.SerializeToDocument(res)
                    }
                    ));
            }
            return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 400,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Name is Already Exist"))
                }
                ));
        }


        [HttpPost("api/v1/transaction/entry")]
        [SwaggerOperation(Summary = "API for creating new transaction")]
        public IActionResult NewTransaction([FromBody] NewTransactionDTO trs)
        {
            if (!ModelState.IsValid) {
                return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 400,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Invalid Data"))
                }
                ));
            }
            if (InsertTransaction(trs))
            {
                return Ok(JsonSerializer.SerializeToDocument(
                    new Response()
                    {
                        code = 200,
                        status = "Success",
                        message = JsonSerializer.SerializeToDocument(trs)

                    }
                    ));
            }
            return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 500,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Failure When Inserting Data"))
                }
                ));
        }

        [HttpGet("api/v1/user/point/")]
        [SwaggerOperation(Summary = "API to get point of each customer")]
        public IActionResult getCustPoint()
        {
            List<Customer> src = GetCustomerList();
            List<TotalPointResponseDTO> res = new List<TotalPointResponseDTO>();
            foreach (Customer c in src )
            {
                res.Add(getCustPoint(c.Id));
                
            }
            if (res != null)
            {
                return Ok(JsonSerializer.SerializeToDocument(
                    new Response()
                    {
                        code = 200,
                        status = "Success",
                        message = JsonSerializer.SerializeToDocument(res)
                    }
                    ));
            }
            return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 500,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Server Error"))
                }
                ));
        }
        [HttpGet("api/v1/user/point/{id}")]
        [SwaggerOperation(Summary = "API to get point of a single customer")]
        public IActionResult GetCustomerPoint(int id)
        {
            var res = getCustPoint(id);
            if (res!=null)
            {
                return Ok(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 200,
                    status = "Success",
                    message = JsonSerializer.SerializeToDocument(res)
                }
                ));
            }
            return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 400,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Invalid Input Parameters"))
                }
                ));
        }



        [HttpGet("api/v1/user/report/")]
        [SwaggerOperation(Summary = "API to get transaction report of a single customer within a period of time")]

        public IActionResult GetCustomerReport(CustomerReportReqDTO req)
        {
            if(!ModelState.IsValid)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(JsonSerializer.SerializeToDocument(
                    new Response()
                    {
                        code = 400,
                        status = "Failed",
                        message = JsonSerializer.SerializeToDocument(new ErrorMessage("Invalid Data"))
                    }
                    ));
                }
            }
            List<CustomerReportDTO> res = GetCustReport(req.Id,req.DateStart,req.DateEnd);
            if(res!=null) {
                return Ok(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 200,
                    status = "Success",
                    message = JsonSerializer.SerializeToDocument(res)
                }
                ));
            }
            return BadRequest(JsonSerializer.SerializeToDocument(
                new Response()
                {
                    code = 400,
                    status = "Failed",
                    message = JsonSerializer.SerializeToDocument(new ErrorMessage("Invalid Input Parameters"))
                }
                ));
        }


        public Customer InsertCustomer(RegCustomerReqDTO cs)
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
                cmd.Parameters.Add("@NEW_ID", SqlDbType.Int).Direction = ParameterDirection.Output;
                cmd.ExecuteNonQuery();
                trs.Commit();
                _con.Close();
                var x = cmd.Parameters["@NEW_ID"].Value;
                return new Customer()
                {
                    Id = x is int ? (int)x : -1,
                    Name = (string)cmd.Parameters["@NAME"].Value
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                trs.Rollback();
                _con.Close();
                return new Customer() { Id = -1 };
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
                        TransactionDate = ((DateTime) reader["TRS_DATE"]).ToShortDateString(),
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
