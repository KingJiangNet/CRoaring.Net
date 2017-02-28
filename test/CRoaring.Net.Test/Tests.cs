using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace CRoaring
{
    [TestClass]
    public class Tests
    {
        [TestMethod]
        public void TestAdd()
        {

            uint[] values = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint max = values.Max() + 1;

            using (var rb1 = new RoaringBitmap())
            using (var rb2 = new RoaringBitmap())
            using (var rb3 = RoaringBitmap.FromValues(values))
            {
                for (int i = 0; i < values.Length; i++)
                    rb1.Add(values[i]);
                rb1.Optimize();

                rb2.Add(values);
                rb2.Optimize();

                rb3.Optimize();

                Assert.AreEqual(rb1.Cardinality, (uint)values.Length);
                Assert.AreEqual(rb2.Cardinality, (uint)values.Length);
                Assert.AreEqual(rb3.Cardinality, (uint)values.Length);

                for (uint i = 0; i < max; i++)
                {
                    if (values.Contains(i))
                    {
                        Assert.IsTrue(rb1.Contains(i));
                        Assert.IsTrue(rb2.Contains(i));
                        Assert.IsTrue(rb3.Contains(i));
                    }
                    else
                    {
                        Assert.IsFalse(rb1.Contains(i));
                        Assert.IsFalse(rb2.Contains(i));
                        Assert.IsFalse(rb3.Contains(i));
                    }
                }
            }
        }

        [TestMethod]
        public void TestRemove()
        {
            uint[] initialValues = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] removeValues = new uint[] { 2, 3 };
            uint[] finalValues = initialValues.Except(removeValues).ToArray();
            uint max = initialValues.Max() + 1;

            using (var rb = RoaringBitmap.FromValues(initialValues))
            {
                rb.Remove(removeValues);
                rb.Optimize();

                Assert.AreEqual(rb.Cardinality, (uint)finalValues.Length);

                for (uint i = 0; i < max; i++)
                {
                    if (finalValues.Contains(i))
                        Assert.IsTrue(rb.Contains(i));
                    else
                        Assert.IsFalse(rb.Contains(i));
                }
            }
        }

        [TestMethod]
        public void TestNot()
        {
            uint[] values = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint max = values.Max() + 1;

            using (var source = RoaringBitmap.FromValues(values))
            using (var result = source.Not(0, max))
            {
                for (uint i = 0; i < max; i++)
                {
                    if (values.Contains(i))
                        Assert.IsFalse(result.Contains(i));
                    else
                        Assert.IsTrue(result.Contains(i));
                }
            }
        }

        [TestMethod]
        public void TestOr()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var source1 = RoaringBitmap.FromValues(values1))
            using (var source2 = RoaringBitmap.FromValues(values2))
            using (var source3 = RoaringBitmap.FromValues(values3))
            using (var result1 = source1.Or(source2))
            using (var result2 = source2.Or(source3))
            using (var result3 = result1.Or(source3))
            {
                Assert.AreEqual(result1.Cardinality, OrCount(values1, values2));
                Assert.AreEqual(result2.Cardinality, OrCount(values2, values3));
                Assert.AreEqual(result3.Cardinality, OrCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestIOr()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var result = RoaringBitmap.FromValues(values1))
            using (var source1 = RoaringBitmap.FromValues(values2))
            using (var source2 = RoaringBitmap.FromValues(values3))
            {
                result.IOr(source1);
                result.IOr(source2);
                Assert.AreEqual(result.Cardinality, OrCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestAnd()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var source1 = RoaringBitmap.FromValues(values1))
            using (var source2 = RoaringBitmap.FromValues(values2))
            using (var source3 = RoaringBitmap.FromValues(values3))
            using (var result1 = source1.And(source2))
            using (var result2 = source2.And(source3))
            using (var result3 = result1.And(source3))
            {
                Assert.AreEqual(result1.Cardinality, AndCount(values1, values2));
                Assert.AreEqual(result2.Cardinality, AndCount(values2, values3));
                Assert.AreEqual(result3.Cardinality, AndCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestIAnd()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var result = RoaringBitmap.FromValues(values1))
            using (var source1 = RoaringBitmap.FromValues(values2))
            using (var source2 = RoaringBitmap.FromValues(values3))
            {
                result.IAnd(source1);
                result.IAnd(source2);
                Assert.AreEqual(result.Cardinality, AndCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestAndNot()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var source1 = RoaringBitmap.FromValues(values1))
            using (var source2 = RoaringBitmap.FromValues(values2))
            using (var source3 = RoaringBitmap.FromValues(values3))
            using (var result1 = source1.AndNot(source2))
            using (var result2 = source2.AndNot(source3))
            using (var result3 = result1.AndNot(source3))
            {
                Assert.AreEqual(result1.Cardinality, AndNotCount(values1, values2));
                Assert.AreEqual(result2.Cardinality, AndNotCount(values2, values3));
                Assert.AreEqual(result3.Cardinality, AndNotCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestIAndNot()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var result = RoaringBitmap.FromValues(values1))
            using (var source1 = RoaringBitmap.FromValues(values2))
            using (var source2 = RoaringBitmap.FromValues(values3))
            {
                result.IAndNot(source1);
                result.IAndNot(source2);
                Assert.AreEqual(result.Cardinality, AndNotCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestXor()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var source1 = RoaringBitmap.FromValues(values1))
            using (var source2 = RoaringBitmap.FromValues(values2))
            using (var source3 = RoaringBitmap.FromValues(values3))
            using (var result1 = source1.Xor(source2))
            using (var result2 = source2.Xor(source3))
            using (var result3 = result1.Xor(source3))
            {
                Assert.AreEqual(result1.Cardinality, XorCount(values1, values2));
                Assert.AreEqual(result2.Cardinality, XorCount(values2, values3));
                Assert.AreEqual(result3.Cardinality, XorCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestIXor()
        {
            uint[] values1 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values2 = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };
            uint[] values3 = new uint[] { 3, 4, 5, 7, 100, 1020 };

            using (var result = RoaringBitmap.FromValues(values1))
            using (var source1 = RoaringBitmap.FromValues(values2))
            using (var source2 = RoaringBitmap.FromValues(values3))
            {
                result.IXor(source1);
                result.IXor(source2);
                Assert.AreEqual(result.Cardinality, XorCount(values1, values2, values3));
            }
        }

        [TestMethod]
        public void TestEnumerator()
        {
            uint[] values = new uint[] { 1, 2, 3, 4, 5, 100, 1000 };

            using (var result = RoaringBitmap.FromValues(values))
                Assert.IsTrue(Enumerable.SequenceEqual(result, values));
        }

        [TestMethod]
        public void TestSerialization()
        {
            using (var rb1 = new RoaringBitmap())
            {
                rb1.Add(1, 2, 3, 4, 5, 100, 1000);
                rb1.Optimize();

                var s1 = rb1.Serialize(SerializationFormat.Normal);
                var s2 = rb1.Serialize(SerializationFormat.Portable);

                using (var rb2 = RoaringBitmap.Deserialize(s1, SerializationFormat.Normal))
                using (var rb3 = RoaringBitmap.Deserialize(s2, SerializationFormat.Portable))
                {
                    Assert.IsTrue(rb1.Equals(rb2));
                    Assert.IsTrue(rb1.Equals(rb3));
                }
            }
        }

        [TestMethod]
        public void TestStats()
        {
            var bitmap = new RoaringBitmap();
            bitmap.Add(1, 2, 3, 4, 6, 7);
            bitmap.Add(999991, 999992, 999993, 999994, 999996, 999997);
            var stats = bitmap.GetStatistics();

            Assert.AreEqual(stats.Cardinality, bitmap.Cardinality);
            Assert.AreEqual(stats.ContainerCount, 2U);
            Assert.AreEqual(stats.ArrayContainerCount, 2U);
            Assert.AreEqual(stats.RunContainerCount, 0U);
            Assert.AreEqual(stats.BitsetContainerCount, 0U);
        }

        private static ulong OrCount(params IEnumerable<uint>[] values)
        {
            var set = values[0];
            for (int i = 1; i < values.Length; i++)
                set = set.Union(values[i]);
            return (ulong)set.LongCount();
        }
        private static ulong AndCount(params IEnumerable<uint>[] values)
        {
            var set = values[0];
            for (int i = 1; i < values.Length; i++)
                set = set.Intersect(values[i]);
            return (ulong)set.LongCount();
        }
        private static ulong AndNotCount(params IEnumerable<uint>[] values)
        {
            var set = values[0];
            for (int i = 1; i < values.Length; i++)
                set = set.Except(values[i]);
            return (ulong)set.LongCount();
        }
        private static ulong XorCount(params IEnumerable<uint>[] values)
        {
            var set = values[0];
            for (int i = 1; i < values.Length; i++)
                set = set.Except(values[i]).Union(values[i].Except(set));
            return (ulong)set.LongCount();
        }
    }
}
