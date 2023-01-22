# AppStoreConnect

This is a .NET implementation of the [App Store Connect API](https://developer.apple.com/documentation/appstoreconnectapi).

The authorization component is lifted directly from https://github.com/dersia/AppStoreConnect. The client APIs are generated from the [App Store Connect OpenAPI specification](https://developer.apple.com/sample-code/app-store-connect/app-store-connect-openapi-specification.zip).

This library has not been extensively tested and should not be used for production changes without first taking extreme care. _Note that misuse or a bug in this library could result in unintended pricing, availability or legal status changes to your apps._

## Repository Overview

There are three projects in the repo:

* **StudioDrydock.AppStoreConnect.Api** 
  The actual class library that contains the API for interacting with the App Store. This is the only library you need to link against for your own projects.
* **StudioDrydock.AppStoreConnect.ApiGenerator** 
  Command-line app that regenerates the source of `StudioDrydock.AppStoreConnect.Api`. Only required when the OpenAPI specification published by Apple changes (or to fix bugs in the generation).
* **StudioDrydock.AppStoreConnect.Cli**
  Command-line app demonstrating usage of the API. It simply lists the ID and Bundle ID of the apps you manage.

## Authorization

The library requires an API key which you can generate at the [Users and Access](https://appstoreconnect.apple.com/access/api) page on the App Store Connect portal. You will need:

* Issuer ID
* Key ID
* Certificate (.p8 file)

To use this authorization with the sample app, save this information in `~/.config/AppStoreConnect.json`:

```
{
  "keyId": "xxxxxxxxxx",
  "keyPath": "AppStoreConnect_xxxxxxxxxx.p8",
  issuerId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"
}
```

## Sample Code

The following sample code lists all apps with their IDs and bundle IDs:

```
using System.IO;
using StudioDrydock.AppStoreConnect.Api;

var api = new AppStoreClient(new StreamReader("path/to/key.p8"), "<key-id>", "<issuer-id>");

var apps = await api.GetApps();
foreach (var app in apps.data)
{
    Console.WriteLine($"{app.id}: {app.attributes.bundleId}");
}
```

## Usage

In general, to find the API for a particular endpoint, search `AppStoreClient.g.cs` for 
the endpoint you are looking for (e.g., `/v1/apps`), and this will reveal the corresponding
API method. From there you can find the request and response object types and supported
parameters.