using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewLabelsObjectDefinition
{
	[DataMember(Name = "properties", EmitDefaultValue = false, IsRequired = false)]
	public DataViewLabelsPropertyDefinition Properties { get; set; }
}
