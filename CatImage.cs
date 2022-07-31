using System.Globalization;
using System.Net;
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace WebmResizer;

public class CatImage
{
    private class Response
    {
        public string id;
        public string url;
        public int width;
        public int height;
    }

    public const string Url = "https://api.thecatapi.com/v1/images/search";
    
    public static byte[]? GetCatImage()
    {
        try
        {
            var client = new HttpClient();
            var response = JsonConvert.DeserializeObject<Response[]>(client.GetStringAsync(Url).Result)!.First();
            var responseStream = client.GetStreamAsync(response.url).Result;
            var stream = new MemoryStream();
            responseStream.CopyTo(stream);
            return stream.ToArray();
        }
        catch
        {
            return null;
        }
    }
    
    //https://stackoverflow.com/questions/2031824/what-is-the-best-way-to-check-for-internet-connectivity-using-net
    public static bool CheckForInternetConnection(int timeoutMs = 10000, string? url = null)
    {
        try
        {
            url ??= CultureInfo.InstalledUICulture switch
            {
                { Name: var n } when n.StartsWith("fa") => // Iran
                    "http://www.aparat.com",
                { Name: var n } when n.StartsWith("zh") => // China
                    "http://www.baidu.com",
                _ =>
                    "http://www.gstatic.com/generate_204"
            };

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.KeepAlive = false;
            request.Timeout = timeoutMs;
            using var response = (HttpWebResponse)request.GetResponse();
            return true;
        }
        catch
        {
            return false;
        }
    }
}