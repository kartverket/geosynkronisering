using System.Text.Json.Serialization;

namespace Provider_NetCore
{
    public enum Status
    {
        [JsonPropertyName("GET_LAST_TRANSNR")]
        GET_LAST_TRANSNR,
        [JsonPropertyName("NO_CHANGES")]
        NO_CHANGES,
        [JsonPropertyName("HAS_CHANGES")]
        HAS_CHANGES,
        [JsonPropertyName("GENERATE_CHANGES")] 
        GENERATE_CHANGES,
        [JsonPropertyName("GENERATE_CHANGES_FAILED")]
        GENERATE_CHANGES_FAILED,
        [JsonPropertyName("WRITE_CHANGES")] 
        WRITE_CHANGES,
        [JsonPropertyName("WRITE_CHANGES_OK")] 
        WRITE_CHANGES_OK,
        [JsonPropertyName("UNKNOWN_ERROR")] 
        UNKNOWN_ERROR
    }
}