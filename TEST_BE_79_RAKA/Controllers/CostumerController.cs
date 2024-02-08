﻿using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using System.Data;
using TEST_BE_79_RAKA.Data.StoredProcedure;
using TEST_BE_79_RAKA.Model;
using TEST_BE_79_RAKA.Model.DTO;
using System.Diagnostics.Eventing.Reader;
using static System.Net.Mime.MediaTypeNames;
using System.Reflection.PortableExecutable;
using System.Drawing;

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
        [HttpGet("api/v1/user/{id}/point")]
        public IActionResult GetCustomerPoint(int id)
        {
            var res = getCustomerPoint(id);
            if (res>=0) { return Ok(res); }
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
        public int getCustomerPoint(int id) 
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
                return (int) totalPoint;

            }
            catch (Exception ex)
            {
                _con.Close();
                return -1;
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
    }
}