using System;
using System.Text;
using NUnit.Framework;
using Prg.Scripts.Common.Util;

namespace Tests.Editor.Util
{
    [TestFixture]
    public class StringSerializerTest
    {
        [Test]
        public void ByteTest()
        {
            // https://github.com/JDanielSmith/Base16k/blob/master/Base16k.Test/Base16kTest.cs
            const int length = 64;
            const int seed = 64;

            var expected = new byte[length];
            var random = new Random(seed);
            random.NextBytes(expected);

            var encoded = StringSerializer.ToBase16KString(expected);
            var actual = StringSerializer.FromBase16KString(encoded);

            Assert.That(expected, Is.EqualTo(actual));
        }

        [Test]
        public void SimpleTest()
        {
            var string1 = "a_b_c_d...xyz..åäö";
            var compressed1 = StringSerializer.Encode(string1);
            Debug.Log($"SimpleTest '{string1}' -> '{compressed1}'");
            var string2 = StringSerializer.Decode(compressed1);
            Assert.That(string1, Is.EqualTo(string2));
        }

        [Test]
        public void LargeTest()
        {
            var string1 = LoremIpsum;
            var compressed1 = StringSerializer.Encode(string1);
            Debug.Log($"LargeTest '{string1.Length}' -> '{compressed1.Length}'");
            var string2 = StringSerializer.Decode(compressed1);
            Assert.That(string1, Is.EqualTo(string2));
        }

        [Test]
        public void SuperLargeTest()
        {
            var builder = new StringBuilder();
            for (int i = 10 - 1; i >= 0; i--)
            {
                builder.Append(LoremIpsum).AppendLine();
            }
            var string1 = builder.ToString();
            var compressed1 = StringSerializer.Encode(string1);
            Debug.Log($"SuperLargeTest '{string1.Length}' -> '{compressed1.Length}'");
            var string2 = StringSerializer.Decode(compressed1);
            Assert.That(string1, Is.EqualTo(string2));
        }

        private static string LoremIpsum = @"Lorem ipsum dolor sit amet, consectetuer adipiscing elit. Sed posuere interdum sem. Quisque ligula eros ullamcorper quis, lacinia quis facilisis sed sapien. Mauris varius diam vitae arcu. Sed arcu lectus auctor vitae, consectetuer et venenatis eget velit. Sed augue orci, lacinia eu tincidunt et eleifend nec lacus. Donec ultricies nisl ut felis, suspendisse potenti. Lorem ipsum ligula ut hendrerit mollis, ipsum erat vehicula risus, eu suscipit sem libero nec erat. Aliquam erat volutpat. Sed congue augue vitae neque. Nulla consectetuer porttitor pede. Fusce purus morbi tortor magna condimentum vel, placerat id blandit sit amet tortor.";
    }
}