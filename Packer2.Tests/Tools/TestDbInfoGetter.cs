using Packer2.Library.MinifiedQueryParser.QueryTransforms;

namespace Packer2.Tests.Tools
{
    public class TestDbInfoGetter : IDbInfoGetter
    {
        public bool IsColumn(string tableName, string propertyName)
        {
            return !IsMeasure(tableName, propertyName);
        }

        public bool IsMeasure(string tableName, string propertyName)
        {
            return propertyName.StartsWith("m_");
        }
    }
}
