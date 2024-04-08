using Newtonsoft.Json;
using System.Data;

namespace jaminBazar_Backend.Models
{

    //public class JsonConverter
    //{
    //    public static string ConvertToJson(object data)
    //    {
    //        try
    //        {
    //            // Check the type of the input data and convert accordingly
    //            if (data is DataTable dataTable)
    //            {
    //                // Convert DataTable to JSON
    //                return JsonConvert.SerializeObject(dataTable);
    //            }
    //            else if (data is DataSet dataSet)
    //            {
    //                // Convert DataSet to JSON
    //                return JsonConvert.SerializeObject(dataSet);
    //            }
    //            else
    //            {
    //                // Convert other types to JSON
    //                return JsonConvert.SerializeObject(data);
    //            }
    //        }
    //        catch (Exception ex)
    //        {
    //            // Handle the exception, log it, or throw it if needed
    //            Console.WriteLine($"Error: {ex.Message}");
    //            return ex.ToString(); // or return an error response
    //        }
    //    }
    //}

    public class Registration
    {
        public string? Phonenumber { get; set; }
        public bool IsLogin { get; set; }
    }

    public class ReactlyAddFram
    {
        public string? farmid { get; set; }
        public string? farmname { get; set; }
        public string? description { get; set; }
        public string? address { get; set; }
        public bool favorite { get; set; }
        public string? imagpath { get; set; }
    }

}
