using FluentAssertions;
using NUnit.Framework;
using UnityTutorialSystem.UI.Trees;

namespace Tests
{
    public class TreePathTest
    {
        // A Test behaves as an ordinary method
        [Test]
        public void Append_On_Empty_Creates_Valid_Path()
        {
            var root = TreePath.Empty<string>();
            var result= root.Append("One");
            result.Count.Should().Be(1);
            result[0].Should().Be("One");
        }

        // A Test behaves as an ordinary method
        [Test]
        public void Append_On_Path_Creates_Valid_Path()
        {
            var root = TreePath.From("One", "Two");
            var result= root.Append("Three");
            result.Count.Should().Be(3);
            result[2].Should().Be("Three");
        }
    }
}
