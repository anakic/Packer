using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class Selector
{
	[DataMember(Name = "metadata", EmitDefaultValue = false, IsRequired = false)]
	internal string Metadata { get; set; }
}
