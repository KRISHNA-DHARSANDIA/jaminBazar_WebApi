using jaminBazar_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace jaminBazar_Backend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        public readonly IConfiguration _Configuration;

        SqlConnection conn;

        public RegistrationController(IConfiguration Configuration)
        {
            _Configuration = Configuration;

            if (_Configuration != null)
            {
                conn = new SqlConnection(_Configuration.GetConnectionString("DefaultConnection").ToString());
            }
        }

        [HttpPost]
        [Route("registration")]

        public IActionResult registration([FromBody] Dictionary<string, string> Params)
        {
            try
            {
                if (conn != null)
                {
                    if (Params != null && Params.ContainsKey("phnumber"))
                    {
                        string Phonenumber = Params["phnumber"];

                        conn.Open();

                        // Check if the user already exists
                        SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM userRegisterdata WHERE phonenumber = @phonenumber", conn);
                        cmd.Parameters.AddWithValue("@phonenumber", Phonenumber);
                        //SQLHelper.SqlHelper.ExecuteDataset(conn,"SELECT COUNT(*) FROM userRegisterdata WHERE phonenumber = @phonenumber");

                        int uidCount = Convert.ToInt32(cmd.ExecuteScalar());

                        if (uidCount > 0)
                        {
                            // User exists, update islogin
                            cmd.CommandText = "UPDATE userRegisterdata SET isLogin = 1 WHERE phonenumber = @phonenumber";
                        }
                        else
                        {
                            // User does not exist, insert new record
                            cmd.CommandText = "INSERT INTO userRegisterdata (phonenumber, isLogin,UUID) VALUES (@phonenumber, @islogin,NEWID())";
                            cmd.Parameters.AddWithValue("@islogin", true);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // new UUID 

                        cmd.CommandText = "SELECT UUId from userRegisterdata WHERE phonenumber = @phonenumber";

                        SqlParameter[] para = new SqlParameter[]
                        {
                        new SqlParameter("@phonenumber",SqlDbType.VarChar) {Value = Phonenumber},
                        };

                        object result = cmd.ExecuteScalar();

                        string uuid = "";

                        if(result != null)
                        {
                            uuid = result.ToString();
                        }

                        conn.Close();

                        var response = new
                        {
                            UUID = uuid,
                            Message = "Data stored successfully"
                        };

                        if (rowsAffected > 0)
                        {
                            return Ok(response);
                        }
                        else
                        {
                            return BadRequest("Error");
                        }
                    }
                    else
                    {
                        return BadRequest("Parameter NUll");
                    }
                }
                else
                {
                    return BadRequest("Connection Problem");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Exception generated");
            }
        }

        [HttpPost]
        [Route("logoutuser")]

        public IActionResult logoutUser([FromBody] Dictionary<string, string> Params) 
        {
            try
            {
                if (conn != null)
                {
                    if (Params != null && Params.ContainsKey("phnumber"))
                    {
                        string Phonenumber = Params["phnumber"];

                        conn.Open();
                        SqlCommand cmd = new SqlCommand("update userRegisterdata set isLogin = 0 where phonenumber = @phonenumber", conn);
                        cmd.Parameters.AddWithValue("@phonenumber", Phonenumber);

                        int i = cmd.ExecuteNonQuery();
                        conn.Close();
                        if (i > 0)
                        {
                            return Ok("User Logout Sccessfully");
                        }
                        else
                        {
                            return BadRequest("Error in logout");
                        }
                    }
                    else
                    {
                        return BadRequest("Parameter NUll");
                    }
                }
                else
                {
                    return BadRequest("Connection Problem");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            return BadRequest("Error");
        }


        [HttpPost]
        [Route("AuthTeachLogin")]

        public IActionResult AuthTeachLogin([FromBody] Dictionary<string, string> Params)
        {
            try
            {
                if (conn != null)
                {
                    if (Params != null && Params.ContainsKey("phnumber"))
                    {
                        string Phonenumber = Params["phnumber"];

                        conn.Open();

                        // Check if the user already exists
                        SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM userRegisterdata WHERE phonenumber = @phonenumber", conn);
                        cmd.Parameters.AddWithValue("@phonenumber", Phonenumber);
                        //SQLHelper.SqlHelper.ExecuteDataset(conn,"SELECT COUNT(*) FROM userRegisterdata WHERE phonenumber = @phonenumber");

                        int uidCount = Convert.ToInt32(cmd.ExecuteScalar());

                        if (uidCount > 0)
                        {
                            // User exists, update islogin
                            cmd.CommandText = "UPDATE userRegisterdata SET isLogin = 1 WHERE phonenumber = @phonenumber";
                        }
                        else
                        {
                            // User does not exist, insert new record
                            cmd.CommandText = "INSERT INTO userRegisterdata (phonenumber, isLogin,UUID) VALUES (@phonenumber, @islogin,NEWID())";
                            cmd.Parameters.AddWithValue("@islogin", true);
                        }

                        int rowsAffected = cmd.ExecuteNonQuery();

                        // new UUID 

                        cmd.CommandText = "SELECT UUId from userRegisterdata WHERE phonenumber = @phonenumber";

                        SqlParameter[] para = new SqlParameter[]
                        {
                        new SqlParameter("@phonenumber",SqlDbType.VarChar) {Value = Phonenumber},
                        };

                        object result = cmd.ExecuteScalar();

                        string uuid = "";

                        if (result != null)
                        {
                            uuid = result.ToString();
                        }

                        conn.Close();

                        var response = new
                        {
                            UUID = uuid,
                            Message = "Data stored successfully"
                        };

                        if (rowsAffected > 0)
                        {
                            return Ok(response);
                        }
                        else
                        {
                            return BadRequest("Error");
                        }
                    }
                    else
                    {
                        return BadRequest("Parameter NUll");
                    }
                }
                else
                {
                    return BadRequest("Connection Problem");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return BadRequest("Exception generated");
            }
        }
    }

    
}
