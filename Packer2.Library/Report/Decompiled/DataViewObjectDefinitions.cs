using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewObjectDefinitions
{
	[DataMember(Name = "general", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewGeneralObjectDefinition> General { get; set; }

	[DataMember(Name = "categoryAxis", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewAxisObjectDefinition> CategoryAxis { get; set; }

	[DataMember(Name = "legend", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewLegendObjectDefinition> Legend { get; set; }

	[DataMember(Name = "labels", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewLabelsObjectDefinition> Labels { get; set; }

	[DataMember(Name = "categoryLabels", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewLabelsObjectDefinition> CategoryLabels { get; set; }

	[DataMember(Name = "dataPoint", EmitDefaultValue = false, IsRequired = false)]
	public List<DataViewDataPointObjectDefinition> DataPoint { get; set; }
}
