using System;
using System.Text.Json;
using System.Threading.Tasks;
using ExpressionEngine;
using ExpressionEngine.Functions.Base;
using EAVFW.ExpressionEngine.Auxiliary;

namespace EAVFW.ExpressionEngine.Functions
{
    public class FormDataAccessor : Function
    {
        private ValueContainer _data;

        public FormDataAccessor() : base("formData")
        {
        }

        public override ValueTask<ValueContainer> ExecuteFunction(params ValueContainer[] parameters)
        {
            if (parameters is { Length: > 0 })
            {
                throw new Exception("formData(): Does not take any parameters");
            }

            return new ValueTask<ValueContainer>(_data);
        }

        public async void SetData(JsonElement json)
        {
            _data = await ValueContainerCustomExtension.CreateValueContainerFromJsonElement(json);
        }

        public void SetData(ValueContainer vc)
        {
            _data = vc;
        }
    }
}