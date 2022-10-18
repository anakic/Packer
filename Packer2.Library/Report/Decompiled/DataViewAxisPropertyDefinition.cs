using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewAxisPropertyDefinition
{
	[DataMember(Name = "axisType", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition AxisType { get; set; }
}
