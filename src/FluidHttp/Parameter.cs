using System.Collections.Generic;
using System.Linq;

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
            string output = string.Empty;

            if (Value == null)
            {
                output += $"{Name}";
            }                
            else if (Value is IEnumerable<object> enumerable)
            {
                foreach (var value in enumerable)
                {
                    output += $"{Name}[]={value.ToString()}";

                    if (value != enumerable.Last())
                        output += "&";
                }
            }
            else
            {
                output += $"{Name}={Value.ToString()}";
            }

            return output.Replace(' ', '+');
        }
    }
}
