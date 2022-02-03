using Newtonsoft.Json.Linq;
using Packer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Steps
{
    internal class UnstuffJsonStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            foreach (var pageFile in model.ExtractedPageFiles)
                Unstuff(pageFile.JObj, "config");
            Unstuff(model.LayoutFile!.JObj, "config");
        }

        public override void ToMachineReadable(RepositoryModel model)
        {
            foreach (var pageFile in model.ExtractedPageFiles)
                Stuff(pageFile.JObj, "#config");
            Stuff(model.LayoutFile!.JObj, "#config");
        }

        public static void Stuff(JObject root, string propName)
        {
            var props = root.SelectTokens(propName).Select(t => t.Parent).Cast<JProperty>().ToArray();
            foreach (var jp in props)
                jp.Replace(new JProperty(jp.Name.Substring(1), jp.Value.ToString(Newtonsoft.Json.Formatting.None)));
        }

        public static void Unstuff(JObject root, string propName)
        {
            var props = root.SelectTokens(propName).Select(t => t.Parent).Cast<JProperty>().ToArray();
            foreach (var jp in props)
                jp.Replace(new JProperty("#" + jp.Name, JToken.Parse(jp.Value.ToString())));
        }
    }
}
