using System.Text.Json.Serialization;
using ARI.DTOs;

namespace ARI.Helpers;

[JsonSerializable(typeof(CreateARIDTO))]
[JsonSerializable(typeof(ARIDTO))]
internal partial class AppJsonSerializerContext : JsonSerializerContext;