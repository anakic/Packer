﻿using Packer2.Library.DataModel.Transofrmations;
using System.Management.Automation;

namespace Packer2.PS.DataModel
{
    [Cmdlet(VerbsLifecycle.Register, "TabularModelMExpressions")]
    public class RegisterMExpressionsCmdlet : DataModelTransformCmdletBase
    {
        protected override IDataModelTransform CreateTransform() 
            => new PullUpExpressionsTranform(CreateLogger<PullUpExpressionsTranform>());
    }
}
