using FacebookLiveVoting.Models;
using FacebookLiveVoting.Tools;
using System.Net.Sockets;
using System.Text.Json;
using System.Xml.Linq;

public class CasparCgClient
{
	private readonly string _host;
	private readonly int _port;


	public CasparCgClient(string host, int port)
	{
		_host = host;
		_port = port;
	}


	public async Task SendDataToTemplateAsync(string templatePath, VoteSummaryModel data)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		using var stream = client.GetStream();

		var writer = new StreamWriter(stream) { AutoFlush = true };
		var reader = new StreamReader(stream);

		// 1. ADD the template (if already loaded, CasparCG just ignores)
		string addCommand = $"CG 2-20 ADD 1 \"{templatePath}\" 1 \"<template-data/>\"\r\n";
		Console.WriteLine(addCommand);
		await writer.WriteAsync(addCommand);
		await reader.ReadLineAsync(); // read response

		// 2. Send UPDATE using XML
		string updateXml = $@"
<template-data>
    <componentData id='totalVotes'><data id='text' value='{data.TotalVotes}' /></componentData>
    <componentData id='a'><data id='text' value='{data.A}' /></componentData>
    <componentData id='b'><data id='text' value='{data.B}' /></componentData>
    <componentData id='c'><data id='text' value='{data.C}' /></componentData>
    <componentData id='d'><data id='text' value='{data.D}' /></componentData>
</template-data>".Trim();

		string cleanedXml = updateXml.Replace("\r", "").Replace("\n", "").Replace("\"", "\\\"");

		string updateCommand = $"CG 2-20 UPDATE 1 \"{cleanedXml}\" \r\n";
		Console.WriteLine(updateCommand);
		await writer.WriteAsync(updateCommand);
		await reader.ReadLineAsync(); // read response
	}

	public async Task SendDataToTemplateAsyncJSON(string templatePath, object data)
	{
		using var client = new TcpClient();
		await client.ConnectAsync(_host, _port);
		using var stream = client.GetStream();

		var writer = new StreamWriter(stream) { AutoFlush = true };
		var reader = new StreamReader(stream);

		// 1. ADD the template (if already loaded, CasparCG just ignores)
		string addCommand = $"CG 2-20 ADD 1 \"{templatePath}\" 1 \"<template-data/>\"\r\n";
		Console.WriteLine(addCommand);
		await writer.WriteAsync(addCommand);
		await reader.ReadLineAsync(); // read response

		// 2. Serialize data and send UPDATE
		var json = JsonSerializer.Serialize(data);

		var convertedXml = JSONToXMLConverter.ConvertJsonToXml(json);





		string updateCommand = $"CG 2-20 UPDATE 1 \"{convertedXml}\"\r\n";
		Console.WriteLine(updateCommand);
		await writer.WriteAsync(updateCommand);
		await reader.ReadLineAsync(); // read response
	}
}
