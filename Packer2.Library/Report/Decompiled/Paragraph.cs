using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class Paragraph
{
	[DataMember(Name = "textRuns", EmitDefaultValue = false)]
	public List<TextRun> TextRuns { get; set; }

	[DataMember(Name = "horizontalTextAlignment", EmitDefaultValue = false)]
	public string HorizontalTextAlignment { get; set; }
}
