using NUnit.Framework;
using sim_engine;

namespace TestProject
{
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void TestGeneConversion()
        {
            uint gene1 = 0xFFFFFFFF;
            var gene1r = new Gene(gene1);
            Assert.AreEqual(gene1, gene1r.ToNumeric());
        }
    }
}