using System.Runtime.Serialization;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class FilterContainer
{
	[DataMember(Name = "field")]
	public SQFieldDef Field { get; set; }

	[DataMember(Name = "filter", EmitDefaultValue = false)]
	public FilterDefinition Filter { get; set; }

	[DataMember(Name = "name", EmitDefaultValue = false)]
	public string Name { get; set; }

	[DataMember(Name = "type", EmitDefaultValue = false)]
	public string Type { get; set; }

	[DataMember(Name = "expression", EmitDefaultValue = false)]
	public QueryExpressionContainer Expression { get; set; }
}
