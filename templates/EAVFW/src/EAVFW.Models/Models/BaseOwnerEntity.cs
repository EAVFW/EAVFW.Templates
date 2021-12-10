using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DotNetDevOps.Extensions.EAVFramework.Shared;
using Newtonsoft.Json;

namespace EAVFW.Models
{
    [BaseEntity]
    [Serializable]
    public class BaseOwnerEntity : BaseIdEntity
    {
        [DataMember(Name = "ownerid")]
        [JsonProperty("ownerid")]
        [JsonPropertyName("ownerid")]
        public Guid? OwnerId { get; set; }

        [ForeignKey("OwnerId")]
        [DataMember(Name = "owner")]
        [JsonProperty("owner")]
        [JsonPropertyName("owner")]
        public Identity Owner { get; set; }
    }
}