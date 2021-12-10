using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using DotNetDevOps.Extensions.EAVFramework;
using DotNetDevOps.Extensions.EAVFramework.Shared;
using Newtonsoft.Json;

namespace EAVFW.Models
{
    [BaseEntity]
    [Serializable]
    public class BaseIdEntity : DynamicEntity
    {

        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [DataMember(Name = "modifiedbyid")]
        [JsonProperty("modifiedbyid")]
        [JsonPropertyName("modifiedbyid")]
        public Guid? ModifiedById { get; set; }

        [ForeignKey("ModifiedById")]
        [JsonProperty("modifiedby")]
        [JsonPropertyName("modifiedby")]
        [DataMember(Name = "modifiedby")]
        public Identity ModifiedBy { get; set; }

        [DataMember(Name = "createdbyid")]
        [JsonProperty("createdbyid")]
        [JsonPropertyName("createdbyid")]
        public Guid? CreatedById { get; set; }

        [ForeignKey("CreatedById")]
        [JsonProperty("createdby")]
        [JsonPropertyName("createdby")]
        [DataMember(Name = "createdby")]
        public Identity CreatedBy { get; set; }

        [DataMember(Name = "modifiedon")]
        [JsonProperty("modifiedon")]
        [JsonPropertyName("modifiedon")]
        public DateTime? ModifiedOn { get; set; }

        [DataMember(Name = "createdon")]
        [JsonProperty("createdon")]
        [JsonPropertyName("createdon")]
        public DateTime? CreatedOn { get; set; }

        [DataMember(Name = "rowversion")]
        [JsonProperty("rowversion")]
        [JsonPropertyName("rowversion")]
        public byte[] RowVersion { get; set; }

    }
}
