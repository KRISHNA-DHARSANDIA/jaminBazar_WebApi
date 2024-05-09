using jaminBazar_Backend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Text.Json;
using SQLHelper;
using System.Security.Cryptography;
using System;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace jaminBazar_Backend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class ReactlyAddedfarms : ControllerBase
    {
        public readonly IConfiguration _Configuration;

        SqlConnection conn;

        public ReactlyAddedfarms(IConfiguration Configuration)
        {
            _Configuration = Configuration;

            if (_Configuration != null)
            {
                SqlConnection con = new SqlConnection(_Configuration.GetConnectionString("DefaultConnection").ToString());

                if (con != null)
                {
                    conn = con;
                }
            }
        }


        [HttpPost]
        [Route("GetReactAddedFarms")]
        public IActionResult GetReactAddedFarms([FromBody] Dictionary<string, string> Params)
        {
            try
            {
                if (Params != null && Params.ContainsKey("phnumber"))
                {
                    string phonenumber = Params["phnumber"];

                    string query = " select pd.* , (select userid from userRegisterdata where phonenumber = @phnumber) as userid ," +
                                   " CASE WHEN fp.pid IS NOT NULL THEN 'true' ELSE 'flase' END AS is_favorite " +
                                   " FROM propertydata pd " +
                                   " LEFT JOIN favoriteProperty fp ON fp.pid = pd.pid AND fp.userid = (select userid from userRegisterdata where phonenumber = @phnumber) " +
                                   " WHERE pd.posting_date >= DATEADD(DAY, -390, GETDATE()) order by pd.posting_date desc;";

                    SqlParameter para = new SqlParameter("@phnumber", phonenumber);

                    using (SqlDataReader reader = SQLHelper.SqlHelper.ExecuteReader(conn, CommandType.Text, query, para))
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        string json = JsonConvert.SerializeObject(dataTable);

                        return Ok(json);
                    }
                }
                else
                {
                    return BadRequest("Parameter NUll");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }

            return BadRequest("");
        }

        [HttpPost]
        [Route("getusefavoritefarm")]
        public IActionResult getusefavoritefarm([FromBody] Dictionary<string, string> objE)
        {
            try
            {
                string uuid = objE["uuid"];

                if (uuid != null)
                {

                    string query = "select userid from userRegisterdata where UUID = @uuid";

                    SqlParameter para2 = new SqlParameter("@uuid", uuid);

                    string? userid = SQLHelper.SqlHelper.ExecuteScalar(conn, CommandType.Text, query, para2).ToString();

                    query = "select favoriteProperty.userid as curruserid, " +
                                   " propertydata.posting_date,propertydata.pid,propertydata.ownerid ,propertydata.ptype,city,propertydata.location," +
                                   " propertydata.areasize,\r\npropertydata.totalprice, propertydata.possession, propertydata.ptitle, propertydata.pdescription, propertydata.coverimagepath," +
                                   " propertydata.email, propertydata.contectone, propertydata.contecttwo from propertydata" +
                                   " left join favoriteProperty on propertydata.pid = favoriteProperty.pid where favoriteProperty.userid = @userid;";

                    SqlParameter para = new SqlParameter("@userid", userid);

                    using (SqlDataReader reader = SQLHelper.SqlHelper.ExecuteReader(conn, CommandType.Text, query, para))
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        string json = JsonConvert.SerializeObject(dataTable);

                        return Ok(json);
                    }
                }
                else
                {
                    //Send Empty DataList
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"getusefavoritefarm: {ex.Message}");
            }
        }


        [HttpPost]
        [Route("storeuserfavorite")]
        public IActionResult StoreUserFavorite([FromBody] Dictionary<string, string> Params)
        {
            try
            {
                if (Params != null && Params.ContainsKey("pid"))
                {
                    int propertyid = Convert.ToInt32(Params["pid"]);
                    int userid = Convert.ToInt32(Params["userid"]);

                    //if (!int.TryParse(propertyid, out _))
                    //{
                    //    return BadRequest("Invalid propertyid format");
                    //}

                    conn.Open();

                    SqlParameter[] para = new SqlParameter[]
                    {
                        new SqlParameter("@propertyid",SqlDbType.Int) {Value = propertyid},
                        new SqlParameter("@userid", SqlDbType.Int) {Value = userid},
                    };


                    string favCheckQuery = $"select favid from favoriteProperty where userid = @userid and pid = @propertyid;";
                    bool isFavorite = Convert.ToBoolean(SQLHelper.SqlHelper.ExecuteScalar(conn, CommandType.Text, favCheckQuery, para));

                    string updateQuery = "";
                    if (isFavorite)
                    {
                        updateQuery = $"delete from favoriteProperty where userid = @userid and pid = @propertyid; ";
                    }
                    else
                    {
                        updateQuery = $"insert into favoriteProperty(pid,userid) values(@propertyid,@userid);";
                    }

                    int count = SQLHelper.SqlHelper.ExecuteNonQuery(conn, CommandType.Text, updateQuery, para);

                    if (count > 0)
                    {
                        return Ok("Favorite updated successfully");
                    }
                    else
                    {
                        return BadRequest("Something went wrong in StoreUserFavorite");
                    }
                }
                else
                {
                    return BadRequest("Invalid or missing farmid parameter");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"StoreUserFavorite: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("storeproperty")]
        public IActionResult StoreProperty([FromBody] JsonElement Params)
        {
            try
            {
                //Dictionary<string, string> Params = null;

                //if (obj != null)
                //{
                //    Params = obj as Dictionary<string, string>;
                //}


                if (Params.ValueKind == JsonValueKind.Object)
                {
                    //string phonenumber = Convert.ToString(Params?["phnumber"]) ?? "";
                    int? pid = Params.TryGetProperty("pid", out JsonElement pidElement) ? pidElement.GetInt32() : null;
                    string? purposeValue = Params.TryGetProperty("purpose", out var purposeElement) ? purposeElement.GetString() : null;
                    string? ptypeValue = Params.TryGetProperty("ptype", out var ptypeElement) ? ptypeElement.GetString() : null;
                    string? cityValue = Params.TryGetProperty("city", out var cityElement) ? cityElement.GetString() : null;
                    string? locationValue = Params.TryGetProperty("location", out var locationElement) ? locationElement.GetString() : null;
                    string? areasizeValue = Params.TryGetProperty("areasize", out var areasizeElement) ? areasizeElement.GetString() : null;
                    string? totalpriceValue = Params.TryGetProperty("totalprice", out var totalpriceElement) ? totalpriceElement.GetString() : null;
                    string? possessionValue = Params.TryGetProperty("possession", out var possessionElement) ? possessionElement.GetString() : null;
                    string? ptitleValue = Params.TryGetProperty("ptitle", out var ptitleElement) ? ptitleElement.GetString() : null;
                    string? pdescriptionValue = Params.TryGetProperty("pdescription", out var pdescriptionElement) ? pdescriptionElement.GetString() : null;
                    string? emailValue = Params.TryGetProperty("email", out var emailElement) ? emailElement.GetString() : null;
                    string? contectoneValue = Params.TryGetProperty("contectone", out var contectoneElement) ? contectoneElement.GetString() : null;
                    string? contecttwoValue = Params.TryGetProperty("contecttwo", out var contecttwoElement) ? contecttwoElement.GetString() : null;
                    string? longitude = Params.TryGetProperty("logitude", out var logitudeelement) ? logitudeelement.GetString() : null;
                    string? latitude = Params.TryGetProperty("latitude", out var latitudeelemnt) ? latitudeelemnt.GetString() : null;

                    string? imagpathValue = Params.TryGetProperty("imagePath", out var imagpathElement) ? imagpathElement.GetString() : null;
                    string? coverimagepathValue = Params.TryGetProperty("coverimagepath", out var coverimagepathElement) ? coverimagepathElement.GetString() : null;

                    conn.Open();

                    string? query = "select userid from userRegisterdata where UUID = @uuid";
                    SqlParameter newpara = new SqlParameter("@uuid", Params.GetProperty("uuid").GetString());
                    string? ownerid = SQLHelper.SqlHelper.ExecuteScalar(conn, CommandType.Text, query, newpara).ToString();

                    if (pid != 0)
                    {
                        query = @"UPDATE propertydata 
                                  SET ownerid = @ownerid,
                                  purpose = @purpose,
                                  ptype = @ptype,
                                  city = @city,
                                  location = @location,
                                  areasize = @areasize,
                                  totalprice = @totalprice,
                                  possession = @possession,
                                  ptitle = @ptitle,
                                  pdescription = @pdescription,
                                  imagpath = @imagpath,
                                  email = @email,
                                  contectone = @contectone,
                                  contecttwo = @contecttwo,
                                  posting_date = GETDATE(),
                                  coverimagepath = @coverimagepath,
                                  longitude = @longitude,
                                  latitude = @latitude
                                  WHERE pid = @pid";
                    }
                    else
                    {
                        query = @"INSERT INTO propertydata (ownerid, purpose, ptype, city, location, areasize,
                                 totalprice, possession, ptitle, pdescription, imagpath, email, contectone, 
                                 contecttwo, posting_date,coverimagepath,longitude,latitude)
                                 VALUES (@ownerid, @purpose, @ptype, @city, @location, @areasize, 
                                 @totalprice, @possession, @ptitle, @pdescription, @imagpath, @email,
                                 @contectone, @contecttwo, GETDATE(),@coverimagepath,@longitude,@latitude)";
                    }

                    SqlParameter[] para = new SqlParameter[]
                    {
                        new SqlParameter("@ownerid", SqlDbType.Int ) {Value =  ownerid },
                        new SqlParameter("@pid", SqlDbType.Int ) {Value =  pid},
                        new SqlParameter("@purpose", SqlDbType.VarChar) { Value = purposeValue },
                        new SqlParameter("@ptype", SqlDbType.VarChar) { Value = ptypeValue },
                        new SqlParameter("@city", SqlDbType.VarChar) { Value = cityValue },
                        new SqlParameter("@location", SqlDbType.VarChar) { Value =  locationValue },
                        new SqlParameter("@areasize", SqlDbType.Int) { Value =  areasizeValue},
                        new SqlParameter("@totalprice", SqlDbType.Int) { Value =  totalpriceValue},
                        new SqlParameter("@possession", SqlDbType.Bit) { Value =  true },
                        new SqlParameter("@ptitle", SqlDbType.VarChar) { Value = ptitleValue },
                        new SqlParameter("@pdescription", SqlDbType.VarChar) { Value =  pdescriptionValue },
                        new SqlParameter("@imagpath", SqlDbType.VarChar) { Value =  imagpathValue},
                        new SqlParameter("@email", SqlDbType.VarChar) { Value =  emailValue },
                        new SqlParameter("@contectone", SqlDbType.VarChar) { Value =  contectoneValue},
                        new SqlParameter("@contecttwo", SqlDbType.VarChar) { Value =  contecttwoValue},
                        new SqlParameter("@longitude", SqlDbType.Float) { Value =  longitude},
                        new SqlParameter("@latitude", SqlDbType.Float) { Value =  latitude},
                        new SqlParameter("@coverimagepath", SqlDbType.VarChar) { Value = coverimagepathValue },
                    };

                    int rowsAffected = SQLHelper.SqlHelper.ExecuteNonQuery(conn, CommandType.Text, query, para);

                    if (rowsAffected > 0)
                    {
                        return Ok("saved");
                    }
                    else
                    {
                        return BadRequest("Something went wrong in storeproperty");
                    }

                }
                else
                {
                    return BadRequest("Invalid or missing farmid parameter");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"storeproperty: {ex.Message}");
            }
        }

        [HttpPost]
        [Route("deleteuserproperty")]

        public IActionResult deleteUserProperty([FromBody] Dictionary<string, string> Params)
        {
            try
            {
                if (Params != null && Params.ContainsKey("pid"))
                {
                    int propertyid = Convert.ToInt32(Params["pid"]);

                    conn.Open();

                    SqlParameter[] para = new SqlParameter[]
                    {
                        new SqlParameter("@propertyid",SqlDbType.Int) {Value = propertyid},
                    };

                    string delqury = "delete from propertydata where pid = @propertyid; " +
                                     "delete from favoriteProperty where pid = @propertyid; ";

                    int rowsAffected = SQLHelper.SqlHelper.ExecuteNonQuery(conn, CommandType.Text, delqury, para);

                    if (rowsAffected > 0)
                    {
                        return Ok("deleted");
                    }
                    else
                    {
                        return BadRequest("something going wrong inside propertydelete");
                    }
                }
                else
                {
                    return BadRequest("Invalid or missing pid parameter where delete property");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"storeproperty: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("getSpecificPropertyData")]
        public IActionResult GetSpecificPropertyData(string pid)
        {
            try
            {
                if (!string.IsNullOrEmpty(pid))
                {
                    string query = "select * from propertydata where pid = @propertyid";

                    SqlParameter para = new SqlParameter("@propertyid", pid);

                    using (SqlDataReader reader = SQLHelper.SqlHelper.ExecuteReader(conn, CommandType.Text, query, para))
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        string json = JsonConvert.SerializeObject(dataTable);

                        return Ok(json);
                    }
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }

        [HttpGet]
        [Route("getUserProperty")]
        public IActionResult GetUserProperty(string uuid)
        {
            try
            {
                if (!string.IsNullOrEmpty(uuid))
                {
                    string query = @"select pid,ptitle,pdescription,ptype,purpose,totalprice,coverimagepath,
                                    CONVERT(VARCHAR(12), propertydata.posting_date, 106) as posting_date 
                                    from propertydata left join userRegisterdata 
                                    on propertydata.ownerid = userRegisterdata.userid
                                    where userRegisterdata.UUID = @uuid
                                    order by propertydata.posting_date";

                    SqlParameter para = new SqlParameter("@uuid", uuid);

                    using (SqlDataReader reader = SQLHelper.SqlHelper.ExecuteReader(conn, CommandType.Text, query, para))
                    {
                        DataTable dataTable = new DataTable();
                        dataTable.Load(reader);

                        string json = JsonConvert.SerializeObject(dataTable);

                        return Ok(json);
                    }
                }
                else
                {
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error: {ex.Message}");
            }
        }
    }
}
