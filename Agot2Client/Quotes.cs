namespace Agot2Client
{
    public class Quotes
    {
        public string title { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string quotes { get; set; }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}: {3}", title, first_name, last_name, quotes);
        }
    }
}
