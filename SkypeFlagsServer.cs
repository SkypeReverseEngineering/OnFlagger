using System;
using System.Text;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#nullable disable
public class SkypeFlagsServer
{
    public const string SERVER_VERSION = "1.0"; 
    public const string LISTEN_HOST = "flagsapi.skype.com";
    public const int LISTEN_PORT = 80;
    public const string FLAGS_FILE = "flags.json";

    public static SkypeFlagsServer Instance { get; private set; }
    public bool IsRunning { get; private set; }
    public HttpListener Server;
    public Thread HandleRequests;
    public List<int> Flags = new List<int>();

    public static void Main(string[] args)
    {
        (Instance = new SkypeFlagsServer()).ServerMain();
    }

    public void ServerMain()
    {
        try
        {
            Logger.Info("Loading saved flags...");
            if (File.Exists(FLAGS_FILE)) LoadFlags();

            SaveFlags();
            Logger.Info($"List of flags: {GetFlags()}");

            IsRunning = true;
            Logger.Info($"Starting SkypeFlagsServer on {LISTEN_HOST}:{LISTEN_PORT}...");

            Server = new HttpListener();
            Server.Prefixes.Add($"http://{LISTEN_HOST}:{LISTEN_PORT}/");
            Server.Start();
        } 
        catch (Exception ex)
        {
            Logger.Error($"Unable to start the server: {ex}");
            Logger.Error($"Is there another server running?");
            Console.WriteLine("Press enter to exit...");
            Console.ReadLine();
            return;
        }

        HandleRequests = new Thread(HandleRequestsThread_Func);
        HandleRequests.Start();

        while (IsRunning)
            // Sleep 1ms to reduce CPU usage
            Thread.Sleep(1);
    }

    private void HandleRequestsThread_Func()
    {
        while (IsRunning)
        {
            try
            {
                HttpListenerContext context = Server.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;

                // Sane content information defaults, to not have to specify them every time
                response.ContentType = "text/html";
                response.ContentEncoding = Encoding.UTF8;

                byte[] resp = HandleRequest(request, response);
                Logger.Info($"{request.RemoteEndPoint}: {request.HttpMethod} {request.Url.AbsolutePath} ->" + 
                $" {response.StatusCode} {response.StatusDescription}");

                response.ContentLength64 = resp.Length;
                response.OutputStream.Write(resp, 0, resp.Length);
                response.Close();
            }
            catch (Exception ex)
            {
                Logger.Error($"Unable to handle a HTTP request: {ex}");
            }
        }
    }

    private byte[] HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        string requestPath = request.Url.AbsolutePath;

        if (requestPath.Equals("/flags/v1") || requestPath.Equals("/flags/v1/"))
        {
            string flags = GetFlags();
            response.ContentType = "application/json";
            Logger.Info($"Flags were requested... List of flags: {flags}");
            return Encoding.UTF8.GetBytes(flags);
        }
        else if (requestPath.StartsWith("/flags/v1/"))
        {
            int flag;
            response.ContentType = "text/plain";

            if (request.HttpMethod != "PUT" && request.HttpMethod != "DELETE") 
            {
                response.StatusCode = 405;
                goto empty_page;
            }

            if (!int.TryParse(requestPath.Split('/', 4)[3], out flag))
            {
                response.StatusCode = 400;
                goto empty_page;
            }

            switch (request.HttpMethod)
            {
                case "PUT":
                    if (Flags.Contains(flag))
                    {
                        Logger.Warning($"Attempted to put an existing flag! ({flag})");
                        response.StatusCode = 400;
                        goto empty_page;
                    }

                    Flags.Add(flag);
                    SaveFlags();
                    Logger.Info($"Put the flag {flag}, list of flags: {GetFlags()}");

                    break;
                case "DELETE":
                    if (!Flags.Contains(flag))
                    {
                        Logger.Warning($"Attempted to delete a non-existing flag! ({flag})");
                        response.StatusCode = 400;
                        goto empty_page;
                    }

                    Flags.Remove(flag);
                    SaveFlags();
                    Logger.Info($"Deleted the flag {flag}, list of flags: {GetFlags()}");

                    break;
            }

            goto empty_page;
        }

        switch (requestPath)
        {
            case "/":
            case "/index":
                return Encoding.UTF8.GetBytes(StaticHTML.HTML_INFO_PAGE);
            default:
                response.StatusCode = 404;
                return Encoding.UTF8.GetBytes(StaticHTML.HTML_404);
        }

        empty_page: return new byte[0];
    }

    private string GetFlags() => JsonConvert.SerializeObject(Flags.ToArray());
    private void LoadFlags() => Flags = JsonConvert.DeserializeObject<List<int>>(File.ReadAllText(FLAGS_FILE));
    private void SaveFlags() => File.WriteAllText(FLAGS_FILE, GetFlags());
}