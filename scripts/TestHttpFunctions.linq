<Query Kind="Program">
  <Reference Relative="..\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll">..\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Belial</Namespace>
</Query>

async Task Main()
{
    var httpClient = new HttpClient();
    
    var bookEntry = new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781844168965",
            Title = "The Purging of Kadillus"
        },
        UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C"),
        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
    };
    var responseContent = new StringContent(JsonConvert.SerializeObject(bookEntry).Dump(), Encoding.UTF8, "application/json");
    
    var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry", responseContent);
    //var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry?code=<functionkey>", responseContent);

    (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}