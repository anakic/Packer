using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class VisualTitle
{
	[DataMember(Name = "show", EmitDefaultValue = true)]
	public bool Show { get; set; }
}
