using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class ProjectionInfo
{
	[DataMember(Name = "role", EmitDefaultValue = false)]
	public string Role { get; set; }

	[DataMember(Name = "queryRef", EmitDefaultValue = true)]
	public string QueryRef { get; set; }

	[DataMember(Name = "active", EmitDefaultValue = false)]
	public bool Active { get; set; }
}
