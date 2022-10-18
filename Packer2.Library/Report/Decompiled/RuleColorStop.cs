using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class RuleColorStop
{
	[DataMember(Name = "color", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Color { get; set; }
}
