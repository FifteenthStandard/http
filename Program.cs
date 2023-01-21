using System.Net.Security;
using System.Net.Sockets;

var argSet = new HashSet<string>(args);
var isHttp = argSet.Remove("--http");
if (argSet.Count > 1)
{
    Console.Error.WriteLine("Usage: http [<request-file>] [--http]");
    return 1;
}
var requestFile = argSet.SingleOrDefault();
var useStdin = requestFile == null || requestFile == "-";

if (useStdin)
{
    requestFile = Path.GetTempFileName();
    using (var stream = new FileStream(requestFile, FileMode.Open))
    {
        await Console.OpenStandardInput().CopyToAsync(stream);
    }
}

var hostHeaderPrefix = "Host: ";
var hostHeader = (await File.ReadAllLinesAsync(requestFile!))
    .FirstOrDefault(line => line.StartsWith(hostHeaderPrefix));
if (hostHeader == null)
{
    Console.Error.WriteLine("Host header is required");
    return 1;
}

var parts = hostHeader.Substring(hostHeaderPrefix.Length).Split(':', 2);
var host = parts[0];
var port = parts.Length == 1
    ? (isHttp ? 80 : 443)
    : int.Parse(parts[1]);
if (port == 80) isHttp = true;

async Task<Stream> GetStream(TcpClient tcpClient, string host, bool isHttp)
{
    var networkStream = tcpClient.GetStream();
    if (isHttp) return networkStream;
    var sslStream = new SslStream(networkStream);
    await sslStream.AuthenticateAsClientAsync(host);
    return sslStream;
}

using (var tcpClient = new TcpClient(host, port))
using (var networkStream = await GetStream(tcpClient, host, isHttp))
using (var fileStream = new FileStream(requestFile!, FileMode.Open))
{
    await fileStream.CopyToAsync(networkStream);
    await networkStream.CopyToAsync(Console.OpenStandardOutput());
}

if (useStdin) File.Delete(requestFile!);

return 0;
