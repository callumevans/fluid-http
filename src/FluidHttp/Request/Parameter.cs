namespace FluidHttp.Request
{
    public class Parameter
    {
        public string Name { get; }

        public object Value { get; }

        public ParameterType Type { get; set; }

        public Parameter(string name, object value)
            : this(name, value, ParameterType.Query)
        {
        }

        public Parameter(string name, object value, ParameterType type)
        {
            this.Name = name;
            this.Value = value;
            this.Type = type;
        }
    }
}
