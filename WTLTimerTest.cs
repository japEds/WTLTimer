using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using Newtonsoft.Json;
namespace WTLTIMER
{
    public class WTLTimerTest
    {
        [FunctionName("WTLTimerTest")]
        public void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log)
        {
            try
            {
            // Set up the API key header
        string apiKey = Environment.GetEnvironmentVariable("eyJhbGciOiJIUzUxMiJ9.eyJVTklRVUVfSUQiOiJvVTVOcE5waUhWM3VPMlAiLCJFTlZJUk9OTUVOVCI6InByb2QiLCJESVNQTEFZX05BTUUiOiJBZG1pbiIsIlRPS0VOX05BTUUiOiJhcGktd2VzdHNob3JlLWNvbS1wcm9kIiwiUk9MRSI6MSwiR0VORVJBVElPTl9EQVRFIjoxNjc5OTAwNDAwMDAwLCJUT0tFTl9UWVBFIjoiYXBpX3Rva2VuIn0.083WHawLWKxc6So0_Iv8NHazKO9OGxpI1Fvc0bg1wPNCHBADSNMhN0xxJE_Ra4cldnGwbTj4XtMf2dKhFNwFQA");
        HttpClient c = new HttpClient();
        c.DefaultRequestHeaders.Add("X-API-TOKEN", apiKey);

                    // Make the curl request to the API
        HttpResponseMessage response = c.GetAsync("https://api.westshore.com/v1/operations/piles").Result;
        response.EnsureSuccessStatusCode();
        string responseBody = response.Content.ReadAsStringAsync().Result;
         // Convert the response body to a list of dictionaries
        List<Dictionary<string, string>> data = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(responseBody);

        // Connect to your SQL database
        string connectionString = Environment.GetEnvironmentVariable("Server=tcp:wtl.database.windows.net,1433;Initial Catalog=WTL;Persist Security Info=False;User ID=esgsa;Password=.Esg12345;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;");
       
        using (SqlConnection connection = new SqlConnection(connectionString))
        {
        connection.Open();

                        // Create a DataTable to hold the data
        DataTable dataTable = new DataTable();
        dataTable.Columns.Add("pilekey", typeof(string));
        dataTable.Columns.Add("pileLine", typeof(string));
        dataTable.Columns.Add("pileStart", typeof(string));
        dataTable.Columns.Add("pileEnd", typeof(string));
        dataTable.Columns.Add("MaxTonnes", typeof(string));
        dataTable.Columns.Add("WTLtonnes", typeof(string));
        dataTable.Columns.Add("PileTypeKey", typeof(string));
        dataTable.Columns.Add("isDeleted", typeof(string));
        dataTable.Columns.Add("CreatedTime", typeof(string));
        dataTable.Columns.Add("DeleteTime", typeof(string));
        dataTable.Columns.Add("AngleOfRepos", typeof(string));
        dataTable.Columns.Add("DefinedWidth", typeof(string));
        dataTable.Columns.Add("Seperation", typeof(string));
        dataTable.Columns.Add("Density", typeof(string)); dataTable.Columns.Add("PlanSessionName", typeof(string));
        dataTable.Columns.Add("pileName", typeof(string));
        dataTable.Columns.Add("IsDozerTrapPile", typeof(string));
        dataTable.Columns.Add("Note", typeof(string));
        dataTable.Columns.Add("DumpOnly", typeof(string));
        dataTable.Columns.Add("ReclaimOnly", typeof(string));
        dataTable.Columns.Add("IsInactive", typeof(string));

                        // Add the data to the DataTable
        foreach (Dictionary<string, string> item in data)
        {
            dataTable.Rows.Add(item["pilekey"], item["pileLine"], item["pileStart"], item["pileEnd"], item["MaxTonnes"], item["WTLtonnes"], item["pileTypeKey"], item["isDeleted"], item["CreatedTime"], item["DeleteTime"], item["AngleOfRepos"]
        , item["DefinedWidth"], item["Seperation"], item["Density"], item["PlanSessionName"], item["pileName"], item["IsDozerTrapPile"], item["Note"], item["DumpOnly"], item["ReclaimOnly"], item["IsInactive"]);
        }

                        // Call the stored procedure to insert the data into the SQL table
        using (SqlCommand command = new SqlCommand("InsertData", connection))
        {
        command.CommandType = CommandType.StoredProcedure;
        command.Parameters.AddWithValue("@data", dataTable);
        command.ExecuteNonQuery();
        }
        }

        log.LogInformation("Data inserted successfully");
        }
        catch (Exception ex)
        {
            log.LogError(ex.ToString());
        }
 
        }
    }
}

