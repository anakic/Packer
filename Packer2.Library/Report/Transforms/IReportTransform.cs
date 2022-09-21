using DataModelLoader.Report;
using System.Reflection;
using System.Reflection.Metadata;

namespace Packer2.Library.Report.Transforms
{
    public interface IReportTransform
    {
        PowerBIReport Transform(PowerBIReport model);
    }
}
