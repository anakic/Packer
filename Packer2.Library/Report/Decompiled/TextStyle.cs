using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class TextStyle
{
	[DataMember(Name = "fontFamily", EmitDefaultValue = false)]
	public string FontFamily { get; set; }

	[DataMember(Name = "fontSize", EmitDefaultValue = false)]
	public string FontSize { get; set; }

	[DataMember(Name = "fontStyle", EmitDefaultValue = false)]
	public string FontStyle { get; set; }

	[DataMember(Name = "fontWeight", EmitDefaultValue = false)]
	public string FontWeight { get; set; }

	[DataMember(Name = "textDecoration", EmitDefaultValue = false)]
	public string TextDecoration { get; set; }
}
