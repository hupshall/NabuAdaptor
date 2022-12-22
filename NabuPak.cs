namespace NabuAdaptor
{
    using System;

    /// <summary>
    /// Class to encapsulate a Nabu Pak
    /// I am defining a nabu PAK to be the main program - and it will have a collection of segments
    /// </summary>
    public class NabuPak
    {
        /// <summary>
        /// List of all the segments in this pak
        /// </summary>
        public NabuSegment[] Segments
        {
            get; set;
        }

        /// <summary>
        /// Name of the pak
        /// </summary>
        public string Name
        {
            get; set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NabuPak"/> class.
        /// </summary>
        /// <param name="segments">Segments for this pak</param>
        /// <param name="name">name of the pak</param>
        public NabuPak(NabuSegment[] segments, string name)
        {
            this.Segments = segments;
            this.Name = name;
        }
    }
}
