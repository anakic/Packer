using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewDataPointPropertyDefinition
{
	[DataMember(Name = "fillRule", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition FillRule { get; set; }
}
