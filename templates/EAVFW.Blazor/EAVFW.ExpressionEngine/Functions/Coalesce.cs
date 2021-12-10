using ExpressionEngine;
using ExpressionEngine.Functions.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ValueType = ExpressionEngine.ValueType;

namespace EAVFW.ExpressionEngine.Functions
{
    public class Coalesce : Function
    {
        public Coalesce() : base(nameof(Coalesce).ToLower())
        {
        }

        public override ValueTask<ValueContainer> ExecuteFunction(params ValueContainer[] parameters)
        {
            return new ValueTask<ValueContainer>(parameters.FirstOrDefault(x => x.Type() != ValueType.Null)??new ValueContainer());
        }
    }
}
