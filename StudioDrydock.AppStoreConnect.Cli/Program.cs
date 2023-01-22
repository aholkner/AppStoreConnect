using System.CommandLine;
using System.CommandLine.Invocation;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using StudioDrydock.AppStoreConnect.Api;
using StudioDrydock.AppStoreConnect.Cli.Models;

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

// --config=<config.json>
var configOption = new Option<FileInfo>("--config", 
    getDefaultValue: () => new FileInfo(Environment.ExpandEnvironmentVariables("%USERPROFILE%/.config/AppStoreConnect.json")),
    description: "JSON configuration file for authorization");
rootCommand.AddOption(configOption);

// --verbose/-v
var verboseOption = new Option<bool>("--verbose", "Enable trace logging of HTTP requests");
verboseOption.AddAlias("-v");
rootCommand.AddOption(verboseOption);

// --output/-o
var outputOption = new Option<FileInfo>("--output", "Specify file to write output to, defaults to stdout");
outputOption.AddAlias("-o");

// get-apps
var getAppsCommand = new Command("get-apps", "Get list of all apps");
getAppsCommand.AddOption(outputOption);
getAppsCommand.SetHandler(GetApps);
rootCommand.AddCommand(getAppsCommand);

// get-app-versions --appId=xxx [--platform=xxx] [--state=xxx]
var getAppVersionsCommand = new Command("get-app-versions", "Get information about specific app versions");
var appIdOption = new Option<string>("--appId") { IsRequired = true };
var platformOption = new Option<AppStoreClient.GetAppsAppStoreVersionsFilterPlatform?>("--platform");
var appStoreStateOption = new Option<AppStoreClient.GetAppsAppStoreVersionsFilterAppStoreState?>("--state");
getAppVersionsCommand.AddOption(appIdOption);
getAppVersionsCommand.AddOption(platformOption);
getAppVersionsCommand.AddOption(appStoreStateOption);
getAppVersionsCommand.AddOption(outputOption);
getAppVersionsCommand.SetHandler(GetAppVersions);
rootCommand.AddCommand(getAppVersionsCommand);

await rootCommand.InvokeAsync(args);

AppStoreClient CreateClient(InvocationContext context)
{
    if (context.ParseResult.GetValueForOption(verboseOption))
        Trace.Listeners.Add(new ConsoleTraceListener(useErrorStream: true));  

    // Read config.json
    FileInfo configFile = context.ParseResult.GetValueForOption(configOption);
    var config = JsonSerializer.Deserialize<Config>(File.ReadAllText(configFile.FullName));
    if (config == null)
        throw new Exception($"Failed to parse {configFile}");
    string configDirectory = configFile.Directory.FullName;

    // Create client
    return new AppStoreClient(new StreamReader(Path.Combine(configDirectory, config.keyPath)), config.keyId, config.issuerId);
}

void Output(InvocationContext context, object result)
{
    string text = JsonSerializer.Serialize(result, new JsonSerializerOptions() { WriteIndented = true });
    FileInfo output = context.ParseResult.GetValueForOption(outputOption);
    if (output == null)
        Console.WriteLine(text);
    else
        File.WriteAllText(output.FullName, text);
}

// Convenience for creating array of one item if the value is defined; otherwise null.
T[] Filter<T>(T? t) 
    where T : struct
{
    if (t.HasValue)   
        return new T[] { t.Value };
    else
        return null;
}

// get-apps
async Task GetApps(InvocationContext context)
{
    var api = CreateClient(context);
    var response = await api.GetApps();
    var apps = response.data.Select(x => new App(x)).ToArray();
    Output(context, apps);
}

// get-app-versions
async Task GetAppVersions(InvocationContext context)
{
    string appId = context.ParseResult.GetValueForOption(appIdOption);
    var platform = context.ParseResult.GetValueForOption(platformOption);
    var appStoreState = context.ParseResult.GetValueForOption(appStoreStateOption);

    var api = CreateClient(context);
    var response = await api.GetAppsAppStoreVersions(appId, 
        filterAppStoreState: Filter(appStoreState), 
        filterPlatform: Filter(platform));

    var versions = response.data.Select(x => new AppVersion(x)).ToArray();
    foreach (var version in versions)
    {
        var localizationResponse = await api.GetAppStoreVersionsAppStoreVersionLocalizations(version.id);
        version.localizations = localizationResponse.data.Select(x => new AppVersionLocalization(x)).ToArray();
    }

    Output(context, versions);
}
