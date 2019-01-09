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
    await HitGetBooksForUserEndpoint();
}

private async Task HitGetBooksForUserEndpoint()
{
	var response = await HttpClient.GetAsync($"http://127.0.0.1:7071/api/GetBooks/{SpreadsheetId}");
    //var response = await httpClient.GetAsync($"http://127.0.0.1:7071/api/GetBooks/{SpreadsheetId}?code=<functionkey>", responseContent);

    var responseContent = (await response.Content.ReadAsStringAsync()).Dump("Response Content");
    JsonConvert.DeserializeObject<Book>(responseContent).Dump();
    response.Dump("Full Response");
    response.EnsureSuccessStatusCode();
}