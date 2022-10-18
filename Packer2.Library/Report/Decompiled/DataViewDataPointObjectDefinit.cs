using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewDataPointObjectDefinition
{
	[DataMember(Name = "properties", EmitDefaultValue = false, IsRequired = false)]
	public DataViewDataPointPropertyDefinition Properties { get; set; }
}
