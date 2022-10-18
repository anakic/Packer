using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class TextRun
{
	[DataMember(Name = "value", EmitDefaultValue = false)]
	public string Value { get; set; }

	[DataMember(Name = "textStyle", EmitDefaultValue = false)]
	public TextStyle TextStyle { get; set; }
}
