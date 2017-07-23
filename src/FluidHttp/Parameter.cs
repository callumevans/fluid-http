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
            var parameterStringBuilder = new StringBuilder();

            if (Value == null)
            {
                parameterStringBuilder.Append($"{Name}");
            }                
            else if (Value is IEnumerable<object> enumerable)
            {
                foreach (var value in enumerable)
                {
                    parameterStringBuilder.Append($"{Name}[]={value.ToString()}");

                    if (value != enumerable.Last())
                        parameterStringBuilder.Append("&");
                }
            }
            else
            {
                parameterStringBuilder.Append(
                    $"{Name}={Value.ToString()}");
            }

            return parameterStringBuilder.ToString().Replace(' ', '+');
        }
    }
}
