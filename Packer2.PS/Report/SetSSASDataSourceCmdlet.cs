﻿using DataModelLoader.Report;
using Packer2.Library.Report.Transforms;
using System.Management.Automation;

namespace Packer2.PS.Report
{
    [Cmdlet(VerbsCommon.Set, "SSASDataSource")]
    [OutputType(typeof(PowerBIReport))]
    public class SetSSASDataSourceCmdlet : Cmdlet
    {
        [Parameter(Mandatory = false, ValueFromPipeline = false, Position = 0)]
        public string ConnectionString { get; set; }

        [Parameter(ValueFromPipeline = true)]
        public PowerBIReport Report { get; set; }

        protected override void ProcessRecord()
        {
            var transform = new RedirectToSSASTransform(ConnectionString);
            WriteObject(transform.Transform(Report));
        }
    }
}