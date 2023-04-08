namespace sim_engine
{
    public class SpeciesParameters
    {
        public float Direction { get; set; }//direction in radians
        public float X { get; set; }
        public float Y { get; set; }
        public float Energy { get; set; }
        public float Life { get; set; }
        /// <summary>
        /// Ability to clone. Init with 0, on reaching some threshold - do clone, reduce this value by some factor
        /// </summary>
        public float Fertilation { get; set; }
    }
}