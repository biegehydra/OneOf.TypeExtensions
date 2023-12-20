using SomeNamespace;

namespace OneOf.TypeExtensions.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TwoTypes()
        {
            OneOf<int, string> oneOf = 1;
            Assert.IsTrue(oneOf.IsInt());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsInt());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            Assert.AreEqual(2, oneOf.MapInt(x => x + 1).AsInt());
            oneOf.TryPickInt(out int value, out string remainder);
            Assert.AreEqual(1, value);
            Assert.IsNull(remainder);
            OneOf<int, string> oneOf2 = "1";
            Assert.IsFalse(oneOf2.IsInt());
            Assert.IsTrue(oneOf2.IsString());
        }

        [TestMethod]
        public void FourTypes()
        {
            OneOf<int, string, char, double> oneOf = 1;
            Assert.IsTrue(oneOf.IsInt());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsInt());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            Assert.AreEqual(2, oneOf.MapInt(x => x + 1).AsInt());
            oneOf.TryPickInt(out int value, out OneOf<string, char, double> remainder);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Lists()
        {
            OneOf<string, List<int>> oneOf = new List<int>() { 1 };
            Assert.IsTrue(oneOf.IsListOfInt());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsListOfInt().First());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            var result = oneOf.MapListOfInt(x => x.Select(y => y + 1).ToList());
            Assert.AreEqual(2, result.AsListOfInt().First());
            oneOf.TryPickListOfInt(out List<int> value, out string remainder);
            Assert.AreEqual(1, value.First());
            Assert.IsNull(remainder);
        }

        [TestMethod]
        public void Dictionary()
        {
            OneOf<string, Dictionary<string, int>> oneOf = new Dictionary<string, int>() { { "test", 1 } };
            Assert.IsTrue(oneOf.IsDictionaryOfString_Int());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsDictionaryOfString_Int().First().Value);
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            var result = oneOf.MapDictionaryOfString_Int(x => x.ToDictionary(kvp => kvp.Key, kvp => kvp.Value + 1));
            Assert.AreEqual(2, result.AsDictionaryOfString_Int().First().Value);
            oneOf.TryPickDictionaryOfString_Int(out Dictionary<string, int> value, out string remainder);
            Assert.AreEqual(1, value.First().Value);
            Assert.IsNull(remainder);
        }

        [TestMethod]
        public void NestedCustomType()
        {
            OneOf<string, CustomType.NestedCustomType> oneOf = new CustomType.NestedCustomType();
            Assert.IsTrue(oneOf.IsNestedCustomType());
            Assert.IsFalse(oneOf.IsString());
            Assert.IsNotNull(oneOf.AsNestedCustomType());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            var result = oneOf.MapNestedCustomType(x => x);
            Assert.IsNotNull(result.AsNestedCustomType());
            oneOf.TryPickNestedCustomType(out CustomType.NestedCustomType value, out string remainder);
            Assert.IsNotNull(value);
            Assert.IsNull(remainder);
        }

        [TestMethod]
        public void NullableTypes()
        {
            OneOf<string, int?> oneOf = 1;
            Assert.IsTrue(oneOf.IsNullableInt());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsNullableInt());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            var result = oneOf.MapNullableInt(x => x + 1);
            Assert.AreEqual(2, result.AsNullableInt());
            oneOf.TryPickNullableInt(out int? value, out string remainder);
            Assert.AreEqual(1, value);
            Assert.IsNull(remainder);
        }

        [TestMethod]
        public void LotsOfTypes()
        {
            OneOf<List<string>, int?, Dictionary<string, double?>> oneOf = 1;
            Assert.IsTrue(oneOf.IsNullableInt());
            Assert.IsFalse(oneOf.IsListOfString());
            Assert.IsFalse(oneOf.IsDictionaryOfString_NullableDouble());
            Assert.AreEqual(1, oneOf.AsNullableInt());
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsListOfString());
            var result = oneOf.MapNullableInt(x => x + 1);
            Assert.AreEqual(2, result.AsNullableInt());
            oneOf.TryPickNullableInt(out int? value, out var remainder);
            Assert.AreEqual(1, value);
        }

        [TestMethod]
        public void Tuples()
        {
            OneOf<string, (int?, string, char?)> oneOf = (1, "test", 'c');
            Assert.IsTrue(oneOf.IsValueTupleOfNullableInt_String_NullableChar());
            Assert.IsFalse(oneOf.IsString());
            Assert.AreEqual(1, oneOf.AsValueTupleOfNullableInt_String_NullableChar().Item1);
            Assert.ThrowsException<InvalidOperationException>(() => oneOf.AsString());
            var result = oneOf.MapValueTupleOfNullableInt_String_NullableChar(x => x);
            Assert.AreEqual(1, result.AsValueTupleOfNullableInt_String_NullableChar().Item1);
            oneOf.TryPickValueTupleOfNullableInt_String_NullableChar(out var value, out string remainder);
            Assert.AreEqual(1, value.Item1);

            OneOf<string, Tuple<int?, string, char?>> oneOf2 = new Tuple<int?, string, char?>(1, "test", 'c');
            Assert.IsTrue(oneOf2.IsTupleOfNullableInt_String_NullableChar());
            Assert.IsFalse(oneOf2.IsString());
            Assert.AreEqual(1, oneOf2.AsTupleOfNullableInt_String_NullableChar().Item1);
            Assert.ThrowsException<InvalidOperationException>(() => oneOf2.AsString());
            var result2 = oneOf2.MapTupleOfNullableInt_String_NullableChar(x => x);
            Assert.AreEqual(1, result2.AsTupleOfNullableInt_String_NullableChar().Item1);
            oneOf2.TryPickTupleOfNullableInt_String_NullableChar(out var value2, out string remainder2);
            Assert.AreEqual(1, value2.Item1);
        }

        [TestMethod]
        public void NamedTuples()
        {
            OneOf<string, (int? Id, string Name)> oneOf = (1, "test");
            var tuple = oneOf.AsValueTupleOfNullableInt_String();
            Assert.AreEqual(1, tuple.Id);
            Assert.AreEqual("test", tuple.Name);
        }
    }
}

namespace SomeNamespace
{
    public class CustomType
    {
        public class NestedCustomType
        {

        }
    }
}