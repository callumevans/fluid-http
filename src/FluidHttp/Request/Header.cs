namespace FluidHttp.Request
{
    public class Header
    {
        public string Key { get; set; }

        public string Value { get; set; }

        public Header(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }
    }
}
