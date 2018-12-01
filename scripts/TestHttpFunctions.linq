<Query Kind="Program">
  <Reference Relative="..\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll">C:\Users\chris\Dropbox\Projects\GitProjects\project-belial\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Belial</Namespace>
</Query>

HttpClient HttpClient = new HttpClient();
Guid UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C");

async Task Main()
{
    //await HitBookEntryEndpoint();
    await HitGetBooksForUserEndpoint();
}

private async Task HitBookEntryEndpoint()
{
    var bookEntry = new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781844168965",
            Title = "The Purging of Kadillus"
        },
        UserId = UserId,
        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
    };
    var requestContent = new StringContent(JsonConvert.SerializeObject(bookEntry).Dump(), Encoding.UTF8, "application/json");

    var response = await HttpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry", requestContent);
    //var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry?code=<functionkey>", responseContent);

    (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}

private async Task HitGetBooksForUserEndpoint()
{
    var getBooksForUser = new GetBooksForUserHttpMessage
    {
        UserId = UserId
    };
    var requestContent = new StringContent(JsonConvert.SerializeObject(getBooksForUser).Dump(), Encoding.UTF8, "application/json");

    var response = await HttpClient.GetAsync("http://127.0.0.1:7071/api/GetBooksForUser/"+UserId.ToString());
    //var response = await httpClient.GetAsync("http://127.0.0.1:7071/api/GetBooksForUser?code=<functionkey>", responseContent);

    var responseContent = (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    JsonConvert.DeserializeObject<BooksForUser>(responseContent).Dump();
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}