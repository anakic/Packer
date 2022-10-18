using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewGeneralObjectDefinition
{
	[DataMember(Name = "selector", EmitDefaultValue = false, IsRequired = false)]
	internal Selector Selector { get; set; }

	[DataMember(Name = "properties", EmitDefaultValue = false, IsRequired = false)]
	internal DataViewGeneralPropertyDefinition Properties { get; set; }
}
