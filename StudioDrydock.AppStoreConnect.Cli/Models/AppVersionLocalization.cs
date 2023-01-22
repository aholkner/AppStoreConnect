using StudioDrydock.AppStoreConnect.Api;

namespace StudioDrydock.AppStoreConnect.Cli.Models
{
    public class AppVersionLocalization
    {
        public string id { get; set; }
        public string locale { get; set; }
        public string description { get; set; }
        public string keywords { get; set; }
        public string promotionalText { get; set; }
        public string marketingUrl { get; set; }
        public string supportUrl { get; set; }
        public string whatsNew { get; set; }

        public AppVersionLocalization()
        { }

        public AppVersionLocalization(AppStoreClient.AppStoreVersionLocalizationsResponse.Data data)
        {
            this.id = data.id;
            this.locale = data.attributes.locale;
            this.description = data.attributes.description;
            this.keywords = data.attributes.keywords;
            this.promotionalText = data.attributes.promotionalText;
            this.marketingUrl = data.attributes.marketingUrl;
            this.supportUrl = data.attributes.supportUrl;
            this.whatsNew = data.attributes.whatsNew;
        }

        internal AppStoreClient.AppStoreVersionLocalizationUpdateRequest CreateUpdateRequest()
        {
            return new AppStoreClient.AppStoreVersionLocalizationUpdateRequest()
            {
                data = new AppStoreClient.AppStoreVersionLocalizationUpdateRequest.Data()
                {
                    id = this.id,
                    attributes = new AppStoreClient.AppStoreVersionLocalizationUpdateRequest.Data.Attributes()
                    {
                        description = this.description,
                        keywords = this.keywords,
                        marketingUrl = this.marketingUrl,
                        promotionalText = this.promotionalText,
                        supportUrl = this.supportUrl,
                        whatsNew = this.supportUrl
                    }
                }
            };
        }
    }

}