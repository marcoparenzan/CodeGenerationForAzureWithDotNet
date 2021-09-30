using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SwaggerProxyGenerator.Models
{
    public partial class SwaggerSchema
    {
        [JsonPropertyName("openapi")]
        public string Openapi { get; set; }

        [JsonPropertyName("info")]
        public SwaggerSchemaInfo Info { get; set; }

        [JsonPropertyName("paths")]
        public Dictionary<string, Dictionary<string,SwaggerSchemaPath>> Paths { get; set; }

        [JsonPropertyName("components")]
        public Components Components { get; set; }
    }

    public partial class SwaggerSchemaInfo
    {
        [JsonPropertyName("title")]
        public string Title { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }
    }

    public partial class SwaggerSchemaPath
    {
        [JsonPropertyName("tags")]
        public string[] Tags { get; set; }

        [JsonPropertyName("requestBody")]
        public RequestBody RequestBody { get; set; }

        [JsonPropertyName("responses")]
        public Dictionary<string, SchemaResponse> Responses { get; set; }
    }

    public partial class RequestBody
    {
        [JsonPropertyName("content")]
        public Dictionary<string, BodyContent> Contents { get; set; }
    }

    public partial class BodyContent
    {
        [JsonPropertyName("schema")]
        public RequestBodyContentSchema Schema { get; set; }
    }

    public partial class RequestBodyContentSchema
    {
        [JsonPropertyName("$ref")]
        public string Ref { get; set; }
    }

    public partial class Components
    {
        [JsonPropertyName("schemas")]
        public Dictionary<string, ComponentSchemaDesc> Schemas { get; set; }
    }

    public partial class ComponentSchemaDesc
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("properties")]
        public Dictionary<string, ComponentSchemaDescProperty> Properties { get; set; }

        [JsonPropertyName("additionalProperties")]
        public bool AdditionalProperties { get; set; }
    }

    public partial class ComponentSchemaDescProperty
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("nullable")]
        public bool? Nullable { get; set; }

        [JsonPropertyName("format")]
        public string? Format { get; set; }
    }

    public partial class SchemaResponse
    {
        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("content")]
        public Dictionary<string, BodyContent> Content { get; set; }
    }
}
