<Query Kind="Program">
  <Reference Relative="..\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.Common.dll">C:\Users\chris\Dropbox\Projects\GitProjects\project-belial\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.Common.dll</Reference>
  <Reference Relative="..\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll">C:\Users\chris\Dropbox\Projects\GitProjects\project-belial\src\Belial\bin\Debug\netcoreapp2.1\bin\Belial.dll</Reference>
  <Reference>&lt;RuntimeDirectory&gt;\System.Net.Http.dll</Reference>
  <NuGetReference>Newtonsoft.Json</NuGetReference>
  <Namespace>Belial</Namespace>
  <Namespace>Newtonsoft.Json</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>Belial.Common</Namespace>
</Query>

HttpClient HttpClient = new HttpClient();
public static Guid UserId = Guid.Parse("63CDBDDD-CE8C-411D-BA1E-0174FA19C05C");

async Task Main()
{
    await HitBookEntryEndpoint();
    //await HitGetBooksForUserEndpoint();
	//await HitUpdateBookForUserEndpoint();
}

private async Task HitBookEntryEndpoint()
{
    foreach (var bookEntryMessage in BookEntryMessages)
    {
        var requestContent = new StringContent(JsonConvert.SerializeObject(bookEntryMessage).Dump(), Encoding.UTF8, "application/json");

        var response = await HttpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry", requestContent);
        //var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/ManualBookEntry?code=<functionkey>", responseContent);

        (await response.Content.ReadAsStringAsync()).Dump("Response Content");
        response.Dump("Full Response");
        response.EnsureSuccessStatusCode();
    }
}

private async Task HitGetBooksForUserEndpoint()
{
    var response = await HttpClient.GetAsync("http://127.0.0.1:7071/api/GetBooksForUser/"+UserId.ToString());
    //var response = await httpClient.GetAsync("http://127.0.0.1:7071/api/GetBooksForUser?code=<functionkey>", responseContent);

    var responseContent = (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    JsonConvert.DeserializeObject<BooksForUser>(responseContent).Dump();
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}

private async Task HitUpdateBookForUserEndpoint()
{
	var message = new UpdateBookHttpMessage
	{
		UserId = UserId,
		Book = new Book
		{
			Isbn = "9781844168965",
			Title = "The Purging of Kadillus",
			ImageFilename = $"{Guid.NewGuid()}.jpg",
			HasRead = true
		}
	}.Dump();
	
	var requestContent = new StringContent(JsonConvert.SerializeObject(message).Dump(), Encoding.UTF8, "application/json");

	var response = await HttpClient.PostAsync("http://127.0.0.1:7071/api/UpdateBookForUser", requestContent);
	//var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/UpdateBookForUser?code=<functionkey>", responseContent);

	(await response.Content.ReadAsStringAsync()).Dump("Response Content");
	response.Dump("Full Response");
	response.EnsureSuccessStatusCode();
}

private BookEntryHttpMessage[] BookEntryMessages = new[]
{
    new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781844168033",
            Title = "Rynn's World"
        },
        UserId = UserId,
        ImageUrl = "http://wh40k.lexicanum.com/mediawiki/images/7/77/Rynn%27s_World_novel_cover.jpg"
    },

    new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781844165148",
            Title = "The Hunt For Voldorius"
        },
        UserId = UserId,
        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/61zIqx5PxtL.jpg"
    },
    new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781844168965",
            Title = "The Purging of Kadillus"
        },
        UserId = UserId,
        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg"
    },
    new BookEntryHttpMessage
    {
        Book = new Book
        {
            Isbn = "9781849700412",
            Title = "Fall Of Damnos"
        },
        UserId = UserId,
        ImageUrl = "https://images-na.ssl-images-amazon.com/images/I/61U3Mt3r2aL.jpg"
    },
};