using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewLegendObjectDefinition
{
	[DataMember(Name = "properties", EmitDefaultValue = false, IsRequired = false)]
	public DataViewLegendPropertyDefinition Properties { get; set; }
}
