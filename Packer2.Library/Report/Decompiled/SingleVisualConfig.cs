using System.Collections.Generic;
using System.Runtime.Serialization;
using Microsoft.InfoNav.Data.Contracts.Internal;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class SingleVisualConfig
{
	[DataMember(Name = "title", EmitDefaultValue = false)]
	public VisualTitle Title { get; set; }

	[DataMember(Name = "visualType", EmitDefaultValue = false)]
	public string VisualType { get; set; }

	//[DataMember(Name = "projections", EmitDefaultValue = false)]
	//public List<ProjectionInfo> Projections { get; set; }

	//[DataMember(Name = "showAllRoles", EmitDefaultValue = false)]
	//public List<string> ShowAllRoles { get; set; }

	[DataMember(Name = "prototypeQuery", EmitDefaultValue = false)]
	public QueryDefinition PrototypeQuery { get; set; }

	//[DataMember(Name = "objects", EmitDefaultValue = false, IsRequired = false)]
	//public DataViewObjectDefinitions Objects { get; set; }
}
