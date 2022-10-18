using System.Runtime.Serialization;
using Microsoft.InfoNav.Data.Contracts.Internal;
using Microsoft.InfoNav.Data.PrimitiveValues;

namespace Microsoft.InfoNav.Explore.VisualContracts;

[DataContract]
internal sealed class DataViewObjectPropertyDefinition
{
	[DataMember(Name = "expr", EmitDefaultValue = false, IsRequired = false)]
	public QueryExpressionContainer Expression { get; set; }

	[DataMember(Name = "filter", EmitDefaultValue = false, IsRequired = false)]
	public FilterDefinition Filter { get; set; }

	[DataMember(Name = "linearGradient2", EmitDefaultValue = false, IsRequired = false)]
	public LinearGradient2 LinearGradient2 { get; set; }

	public static implicit operator DataViewObjectPropertyDefinition(bool value)
	{
		return new DataViewObjectPropertyDefinition
		{
			Expression = Literal(value)
		};
	}

	public static implicit operator DataViewObjectPropertyDefinition(string value)
	{
		if (value != null)
		{
			return new DataViewObjectPropertyDefinition
			{
				Expression = Literal(value)
			};
		}
		return null;
	}

	public static implicit operator DataViewObjectPropertyDefinition(FilterDefinition filterDefinition)
	{
		return new DataViewObjectPropertyDefinition
		{
			Filter = filterDefinition
		};
	}

	public static DataViewObjectPropertyDefinition ToPropertyDefinition(bool? value)
	{
		if (value.HasValue)
		{
			return value.Value;
		}
		return null;
	}

	private static QueryLiteralExpression Literal(PrimitiveValue value)
	{
		return new QueryLiteralExpression
		{
			Value = PrimitiveValueEncoding.ToTypeEncodedString(value)
		};
	}
}
