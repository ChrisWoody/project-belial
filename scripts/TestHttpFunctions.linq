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
private const string SpreadsheetId = "";

async Task Main()
{
    await HitGetBooksEndpoint();
	//await HitRefreshImagesEndpoint();
}

private async Task HitGetBooksEndpoint()
{
	var response = await HttpClient.GetAsync($"http://127.0.0.1:7071/api/GetBooks/{SpreadsheetId}");
    //var response = await httpClient.GetAsync($"http://127.0.0.1:7071/api/GetBooks/{SpreadsheetId}?code=<functionkey>", responseContent);

    var responseContent = (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    JsonConvert.DeserializeObject<Book>(responseContent).Dump();
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}

private async Task HitRefreshImagesEndpoint()
{
	var images = "[\"https://images-na.ssl-images-amazon.com/images/I/816K5KxglLL.jpg\",\"https://images-na.ssl-images-amazon.com/images/I/8147jfELDHL.jpg\"]";
	var requestContent = new StringContent(images, Encoding.UTF8, "application/json");

	var response = await HttpClient.PostAsync("http://127.0.0.1:7071/api/RefreshImages", requestContent);
	//var response = await httpClient.PostAsync("http://127.0.0.1:7071/api/RefreshImages?code=<functionkey>", responseContent);

	(await response.Content.ReadAsStringAsync()).Dump("Response Content");
	response.Dump("Full Response");
	response.EnsureSuccessStatusCode();
}