using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewLegendPropertyDefinition
{
	[DataMember(Name = "show", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Show { get; set; }

	[DataMember(Name = "position", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Position { get; set; }
}
