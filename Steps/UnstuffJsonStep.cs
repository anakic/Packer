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
            {
                Unstuff(pageFile.JObj, "config");
                Unstuff(pageFile.JObj, "filters");
            }

            Unstuff(model.LayoutFile!.JObj, "config");
        }

        private static void Unstuff(JObject root, string propName)
        {
            var props = root.Descendants().OfType<JProperty>().Where(jp => jp.Name == propName).ToArray();
            foreach (var jp in props)
            {
                jp.Replace(new JProperty("#" + jp.Name, JToken.Parse(jp.Value.ToString())));
            }
        }
    }
}
