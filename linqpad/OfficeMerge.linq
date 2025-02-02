<Query Kind="Program">
  <NuGetReference Version="2.0.0-alpha0002" Prerelease="true">Gotenberg.Sharp.API.Client</NuGetReference>
  <Namespace>Gotenberg.Sharp.API.Client</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Builders.Faceted</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Domain.Requests</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Extensions</Namespace>
  <Namespace>Gotenberg.Sharp.API.Client.Infrastructure</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>


static Random Rand = new Random(Math.Abs((int)DateTime.Now.Ticks));
static string ResourcePath = @$"{Path.GetDirectoryName(Util.CurrentQueryPath)}\Resources\OfficeDocs";

async Task Main()
{
	var destination = @"D:\Gotenberg\Dumps";
	var p = await DoOfficeMerge(ResourcePath, destination);

	ResourcePath.Dump();

	var info = new ProcessStartInfo { FileName = p, UseShellExecute = true };
	Process.Start(info);
	
	p.Dump("The path");
}

public async Task<string> DoOfficeMerge(string sourceDirectory, string destinationDirectory)
{
	var client = new GotenbergSharpClient("http://localhost:3000");

	var builder = new MergeOfficeBuilder()
		.WithAsyncAssets(async b => b.AddItems(await GetDocsAsync(sourceDirectory)))
		.SetPdfFormat(PdfFormats.A2b)
		 .UseNativePdfFormat()
		.ConfigureRequest(n => 
			n.SetPageRanges("1-3") //Only one of the files has more than 1 page.
		);

	var request = await builder.BuildAsync();
	request.ApiPath.Dump();
	
	request.ToHttpContent()
		   .ToDumpFriendlyFormat()
		   .Dump();

	var response = await client.MergeOfficeDocsAsync(request).ConfigureAwait(false);

	var mergeResultPath = @$"{destinationDirectory}\GotenbergOfficeMerge-{Rand.Next()}.pdf";
	
	using (var destinationStream = File.Create(mergeResultPath))
	{
		await response.CopyToAsync(destinationStream).ConfigureAwait(false);
	}

	return mergeResultPath;
}

async Task<IEnumerable<KeyValuePair<string, byte[]>>> GetDocsAsync(string sourceDirectory)
{
	var paths = Directory.GetFiles(sourceDirectory, "*.*", SearchOption.TopDirectoryOnly);
	var names = paths.Select(p => new FileInfo(p).Name);
	var tasks = paths.Select(f => File.ReadAllBytesAsync(f));

	var docs = await Task.WhenAll(tasks);

	return names.Select((name, index) => KeyValuePair.Create(name, docs[index]))
	.Take(10);
}