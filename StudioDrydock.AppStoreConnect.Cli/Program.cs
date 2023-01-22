using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StudioDrydock.AppStoreConnect.Api;

// --config=<config.json>
var configOption = new Option<FileInfo>("config", 
    getDefaultValue: () => new FileInfo(Environment.ExpandEnvironmentVariables("%USERPROFILE%/.config/AppStoreConnect.json")),
    description: "JSON configuration file for authorization");

var rootCommand = new RootCommand();
rootCommand.Description = 
@"Demonstration of AppStoreConnect. To set up authorization, you will need a config.json
(by default in ~/.config/AppStoreConnect.json) with the following structure:

  {
    ""keyId"": ""xxxxxxxxxx"",
    ""keyPath"": ""AppStoreConnect_xxxxxxxxxx.p8"",
    ""issuerId"": ""xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx""
  }

You can obtain these values, and the key file, from the Keys section in 
https://appstoreconnect.apple.com/access/api";

rootCommand.AddOption(configOption);
rootCommand.SetHandler(Main);
await rootCommand.InvokeAsync(args);

async Task Main(InvocationContext context)
{
    // Read config.json
    FileInfo configFile = context.ParseResult.GetValueForOption(configOption);
    var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile.FullName));
    if (config == null)
        throw new Exception($"Failed to parse {configFile}");
    string configDirectory = configFile.Directory.FullName;

    // Create client
    var api = new AppStoreClient(new StreamReader(Path.Combine(configDirectory, config.keyPath)), config.keyId, config.issuerId);

    // Get list of apps
    var apps = await api.GetApps();
    foreach (var app in apps.data)
    {
        Console.WriteLine($"{app.id}: {app.attributes.bundleId}");
    }
}