using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewLabelsPropertyDefinition
{
	[DataMember(Name = "show", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Show { get; set; }
}
