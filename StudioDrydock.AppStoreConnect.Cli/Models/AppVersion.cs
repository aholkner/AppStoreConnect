using StudioDrydock.AppStoreConnect.Api;

namespace StudioDrydock.AppStoreConnect.Cli.Models
{
    public class AppVersion
    {
        public string id { get; set; }
        public Platform platform { get; set; }
        public string versionString { get; set; }
        
        public AppStoreState appStoreState { get; set; }
        public string copyright { get; set; }
        
        public ReleaseType releaseType { get; set; }
        public string earliestReleaseDate { get; set; }
        public bool downloadable { get; set; }
        public string createdDate { get; set; }

        public AppVersionLocalization[] localizations { get; set; }

        public AppVersion()
        { }

        public AppVersion(AppStoreClient.AppStoreVersionsResponse.Data data)
        {
            this.id = data.id;
            this.platform = EnumExtensions<Platform>.Convert(data.attributes.platform.Value);
            this.versionString = data.attributes.versionString;
            this.appStoreState = EnumExtensions<AppStoreState>.Convert(data.attributes.appStoreState.Value);
            this.copyright = data.attributes.copyright;
            this.releaseType = EnumExtensions<ReleaseType>.Convert(data.attributes.releaseType.Value);
            this.earliestReleaseDate = data.attributes.earliestReleaseDate;
            this.downloadable = data.attributes.downloadable.Value;
            this.createdDate = data.attributes.createdDate;
        }
    }
}