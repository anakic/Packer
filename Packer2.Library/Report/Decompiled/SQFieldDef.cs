using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class SQFieldDef
{
	[DataMember(Name = "entity")]
	public string EntityName { get; set; }

	[DataMember(Name = "column", EmitDefaultValue = false)]
	public string FieldName { get; set; }
}
