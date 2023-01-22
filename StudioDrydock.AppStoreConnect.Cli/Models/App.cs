using StudioDrydock.AppStoreConnect.Api;

namespace StudioDrydock.AppStoreConnect.Cli.Models
{
    public class App
    {
        public string id { get; set; }
        public string bundleId { get; set; }
        public string name { get; set; }
        public string sku { get; set; }
        public string primaryLocale { get; set; }

        public App(AppStoreClient.AppsResponse.Data data)
        {
            this.id = data.id;
            this.bundleId = data.attributes.bundleId;
            this.name = data.attributes.name;
            this.sku = data.attributes.sku;
            this.primaryLocale = data.attributes.primaryLocale;
        }
    }
}