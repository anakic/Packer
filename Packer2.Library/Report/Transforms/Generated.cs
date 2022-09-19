using Newtonsoft.Json;

namespace Packer2.Library.Report.Transforms
{
    public class ResourceItem
    {
        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("path")]
        public string Path { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Pod
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("boundSection")]
        public string BoundSection { get; set; }

        [JsonProperty("parameters")]
        public string Parameters { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }
    }

    public class ResourcePackageContainer
    {
        [JsonProperty("resourcePackage")]
        public ResourcePackage ResourcePackage { get; set; }
    }

    public class ResourcePackage
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public int Type { get; set; }

        [JsonProperty("items")]
        public List<ResourceItem> Items { get; set; }

        [JsonProperty("disabled")]
        public bool Disabled { get; set; }
    }

    public class ReportLayout
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("resourcePackages")]
        public List<ResourcePackageContainer> ResourcePackages { get; set; }

        [JsonProperty("sections")]
        public List<Section> Sections { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("layoutOptimization")]
        public int LayoutOptimization { get; set; }

        [JsonProperty("publicCustomVisuals")]
        public List<string> PublicCustomVisuals { get; set; }

        [JsonProperty("pods")]
        public List<Pod> Pods { get; set; }
    }

    public class Section
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("ordinal")]
        public int Ordinal { get; set; }

        [JsonProperty("visualContainers")]
        public List<VisualContainer> VisualContainers { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("displayOption")]
        public int DisplayOption { get; set; }

        [JsonProperty("width")]
        public int Width { get; set; }

        [JsonProperty("height")]
        public int Height { get; set; }

        [JsonProperty("id")]
        public int? Id { get; set; }
    }

    public class VisualContainer
    {
        [JsonProperty("x")]
        public double X { get; set; }

        [JsonProperty("y")]
        public double Y { get; set; }

        [JsonProperty("z")]
        public int Z { get; set; }

        [JsonProperty("width")]
        public double Width { get; set; }

        [JsonProperty("height")]
        public double Height { get; set; }

        [JsonProperty("config")]
        public string Config { get; set; }

        [JsonProperty("tabOrder")]
        public int TabOrder { get; set; }

        [JsonProperty("filters")]
        public string Filters { get; set; }

        [JsonProperty("query")]
        public string Query { get; set; }

        [JsonProperty("dataTransforms")]
        public string DataTransforms { get; set; }
    }
}
