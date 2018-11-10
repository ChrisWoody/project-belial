<Query Kind="Program">
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
</Query>

async Task Main()
{
    var httpClient = new HttpClient();
    
    var bookEntry = new BookEntry
    {
        Name = "The Purging of Kadillus"
    };
    var responseContent = new StringContent(JsonConvert.SerializeObject(bookEntry).Dump(), Encoding.UTF8, "application/json");
    
    var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry", responseContent);

    (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}

public class BookEntry
{
    public string Name { get; set; }
}