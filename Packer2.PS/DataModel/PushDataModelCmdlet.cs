﻿using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Push, "TabularModel")]
    public class PushDataModelCmdlet : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("d")]
        public string Destination { get; set; }

        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Model { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelStore store = StoreHelper.GetDataModelStore(SessionState.Path.CurrentLocation.Path, Destination);
            store.Save(Model);
        }
    }
}