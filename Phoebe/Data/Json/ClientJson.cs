using System;
using Newtonsoft.Json;

namespace Toggl.Phoebe.Data.Json
{
    public class ClientJson : CommonJson
    {
        [JsonProperty ("name")]
        public string Name { get; set; }

        [JsonProperty ("wid")]
        public long WorkspaceId { get; set; }
    }
}
