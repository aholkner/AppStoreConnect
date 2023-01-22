using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using StudioDrydock.AppStoreConnect.ApiGenerator;

OpenApiDocument document;
using (var stream = File.OpenRead("app_store_connect_api_2.1_openapi.json"))
    document = new OpenApiStreamReader().Read(stream, out _);

using (var stream = File.Create("../StudioDrydock.AppStoreConnect.Api/AppStoreClient.g.cs"))
using (var writer = new StreamWriter(stream))
using (var api = new ApiWriter(writer))
{
    api.cs.BeginBlock("public partial class AppStoreClient");
    foreach (var kv in document.Paths)
    {
        var path = kv.Key;
        var pathItem = kv.Value;
        foreach (var operation in pathItem.Operations)
            api.GenerateOperation(path, pathItem, operation.Key, operation.Value);
    }
    api.cs.EndBlock();

}