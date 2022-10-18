using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class VisualContainerConfig
{
	[DataMember(Name = "name", EmitDefaultValue = false)]
	public string Name { get; set; }

    [DataMember(Name = "singleVisual", EmitDefaultValue = false)]
	public SingleVisualConfig SingleVisual { get; set; }
}
