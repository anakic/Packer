using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewGeneralPropertyDefinition
{
	[DataMember(Name = "totals", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Totals { get; set; }

	[DataMember(Name = "rowSubtotals", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition RowSubtotals { get; set; }

	[DataMember(Name = "columnSubtotals", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition ColumnSubtotals { get; set; }

	[DataMember(Name = "formatString", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition FormatString { get; set; }

	[DataMember(Name = "paragraphs", EmitDefaultValue = false, IsRequired = false)]
	public List<Paragraph> Paragraphs { get; set; }

	[DataMember(Name = "imageUrl", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition ImageUrl { get; set; }

	[DataMember(Name = "filter", EmitDefaultValue = false, IsRequired = false)]
	public DataViewObjectPropertyDefinition Filter { get; set; }
}
