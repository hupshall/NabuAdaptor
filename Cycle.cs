namespace NabuAdaptor
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    [Serializable()]
    public class Cycle
    {
        public enum Target
        {
            Cycle,
            Program,
            Arcade
        }

        public Target TargetType { get; set; }
        public string Name { get; set; }
        public string Url { get; set; }

        public Cycle()
        {
        }

        public Cycle(string name, string url, Target targetType)
        {
            this.Name = name;
            this.Url = url;
            this.TargetType = targetType;
        }

        public override string ToString()
        {
            return this.Name;
        }
    }
}
