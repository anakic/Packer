using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class LinearGradient2
{
	[DataMember(Name = "max", EmitDefaultValue = false, IsRequired = false)]
	public RuleColorStop Max { get; set; }

	[DataMember(Name = "min", EmitDefaultValue = false, IsRequired = false)]
	public RuleColorStop Min { get; set; }
}
