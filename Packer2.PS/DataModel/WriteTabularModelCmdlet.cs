﻿using DataModelLoader.Report;
using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommunications.Write, "TabularModel")]
    public class WriteTabularModelCmdlet : StoreCmdletBase
    {
        [Parameter(Mandatory = true, Position = 0)]
        [Alias("d")]
        public string Destination { get; set; }

        [Parameter(ValueFromPipeline = true, Mandatory = true)]
        public Database Database { get; set; }

        protected override void ProcessRecord()
        {
            IDataModelStore store = GetDataModelStore(Destination);
            store.Save(Database);
        }
    }
}