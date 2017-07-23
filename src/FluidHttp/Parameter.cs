using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FluidHttp
{
    public class Parameter
    {
        public string Name { get; }

        public object Value { get; }

        public Parameter(string name, object value)
        {
            this.Name = name;
            this.Value = value;
        }

        public override string ToString()
        {
            var parameterString = new StringBuilder();

            if (Value == null)
            {
                parameterString.Append($"{Name}");
            }                
            else if (Value is IEnumerable<object> enumerable)
            {
                foreach (var value in enumerable)
                {
                    parameterString.Append($"{Name}[]={value.ToString()}");

                    if (value != enumerable.Last())
                        parameterString.Append("&");
                }
            }
            else
            {
                parameterString.Append(
                    $"{Name}={Value.ToString()}");
            }

            return parameterString.ToString().Replace(' ', '+');
        }
    }
}
