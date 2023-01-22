using System.Text.Json.Serialization;
using StudioDrydock.AppStoreConnect.Api;

namespace StudioDrydock.AppStoreConnect.Cli.Models
{
    public static class EnumExtensions<Dest>
        where Dest : struct, Enum
    {
        public static Dest Convert<Src>(Src src)
            where Src : struct, Enum
        {
            string name = Enum.GetName<Src>(src);
            return Enum.Parse<Dest>(name);
        }
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum Platform
    {
        IOS,
        MAC_OS,
        TV_OS,
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum AppStoreState
    {
        ACCEPTED,
        DEVELOPER_REMOVED_FROM_SALE,
        DEVELOPER_REJECTED,
        IN_REVIEW,
        INVALID_BINARY,
        METADATA_REJECTED,
        PENDING_APPLE_RELEASE,
        PENDING_CONTRACT,
        PENDING_DEVELOPER_RELEASE,
        PREPARE_FOR_SUBMISSION,
        PREORDER_READY_FOR_SALE,
        PROCESSING_FOR_APP_STORE,
        READY_FOR_REVIEW,
        READY_FOR_SALE,
        REJECTED,
        REMOVED_FROM_SALE,
        WAITING_FOR_EXPORT_COMPLIANCE,
        WAITING_FOR_REVIEW,
        REPLACED_WITH_NEW_VERSION,
    }

    [JsonConverter(typeof(JsonStringEnumMemberConverter))]
    public enum ReleaseType
    {
        MANUAL,
        AFTER_APPROVAL,
        SCHEDULED,
    }
}