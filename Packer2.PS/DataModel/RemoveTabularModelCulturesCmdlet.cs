﻿using Microsoft.AnalysisServices.Tabular;
using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsCommon.Remove, "TabularModelCultures")]
    public class RemoveTabularModelCulturesCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform()
            => new StripCulturesTransform(CreateLogger<StripCulturesTransform>());
    }
}
