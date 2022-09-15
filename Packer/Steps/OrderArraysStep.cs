using Newtonsoft.Json.Linq;
using Packer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.Steps
{
    internal class OrderArraysStep : StepBase
    {
        public override void ToHumanReadable(RepositoryModel model)
        {
            if (model.DataModelSchemaFile == null)
                return;

            OrderArray(model.DataModelSchemaFile!.JObj.SelectToken("model.relationships")!, "fromTable", "fromColumn", "toTable", "toColumn");
        }

        private void OrderArray(JToken array, params string [] properties)
        {
            var ordered = array.OrderBy(x => x[properties.First()]);
            foreach (var property in properties.Skip(1))
                ordered = ordered.ThenBy(x => x[property]);
            array.Replace(new JArray(ordered));
        }
    }
}
