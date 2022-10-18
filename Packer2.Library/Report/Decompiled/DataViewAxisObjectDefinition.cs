using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewAxisObjectDefinition
{
	[DataMember(Name = "properties", EmitDefaultValue = false, IsRequired = false)]
	public DataViewAxisPropertyDefinition Properties { get; set; }
}
