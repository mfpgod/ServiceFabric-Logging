namespace Services.Contracts.Adapters.DocumentDbDependency
{
    using Newtonsoft.Json;

    public class UserEntity
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        [JsonProperty("name")]
        public string Name { get; set; }
        [JsonProperty("processed")]
        public bool Processed { get; set; }
    }
}
