using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace DataModelLoader.Report
{
    public interface IResourceFile
    {
        string Name { get; }
        byte[] Read();
        void Write(byte[] bytes);
    }

    public interface JsonFile
    {
        JObject JsonObj { get; set; }
    }

    interface Bookmark : JsonFile
    {
        // todo: would be good if this was a model (not json)
        // and in the folder (git) view we'd see it in a
        // simple-as-possible view (e.g. name/show(ids)+hide(ids),
        // while in the pbit/x it would be the json.

        // the problem is I don't know what the model would look like
        // how to convert to/from json
    }

    interface ReportPageFile : JsonFile
    {
        List<Bookmark> Bookmarks { get; }
    }

    interface LayoutFile : JsonFile
    {
        List<ReportPageFile> Pages { get; }
    }

    public class ReportModel
    {
        public List<IResourceFile> Resources { get; set; }  = new List<IResourceFile>();

        public JsonFile LayoutFile { get; }

        public List<JsonFile> PageFiles { get; }
    }
}
