using NUnit.Framework;
using FluentAssertions;

namespace ParserObjects.Tests.Examples.Classes
{
    public class ClassParserTests
    {
        [Test]
        public void SingleClass_Success()
        {
            var target = new ClassParser();
            var result = target.Parse("public class MyClass { }");
            result.Should().NotBeNull();
            result.AccessModifier.Should().Be("public");
            result.StructureType.Should().Be("class");
            result.Name.Should().Be("MyClass");
            result.Children.Count.Should().Be(0);
        }

        [Test]
        public void SingleClass_Fail_AccessModifierSecond()
        {
            var target = new ClassParser();
            var result = target.Parse("class public MyClass { }");
            result.Should().BeNull();
        }

        [Test]
        public void SingleClass_Fail_NoAccessModifier()
        {
            var target = new ClassParser();
            var result = target.Parse("class MyClass { }");
            result.Should().BeNull();
        }

        [Test]
        public void SingleClass_Fail_NoStructureType()
        {
            var target = new ClassParser();
            var result = target.Parse("public MyClass { }");
            result.Should().BeNull();
        }

        [Test]
        public void NestedClasses_Success()
        {
            var target = new ClassParser();
            var c1 = target.Parse(@"
public class MyClass1 { 
    private struct MyClass2 {
        internal interface MyClass3 {
        }
    }
}");
            c1.Should().NotBeNull();
            c1.AccessModifier.Should().Be("public");
            c1.StructureType.Should().Be("class");
            c1.Name.Should().Be("MyClass1");
            c1.Children.Count.Should().Be(1);

            var c2 = c1.Children[0];
            c2.Should().NotBeNull();
            c2.AccessModifier.Should().Be("private");
            c2.StructureType.Should().Be("struct");
            c2.Name.Should().Be("MyClass2");
            c2.Children.Count.Should().Be(1);

            var c3 = c2.Children[0];
            c3.Should().NotBeNull();
            c3.AccessModifier.Should().Be("internal");
            c3.StructureType.Should().Be("interface");
            c3.Name.Should().Be("MyClass3");
            c3.Children.Count.Should().Be(0);
        }

        [Test]
        public void InterfacesMayNotContainChildren()
        {
            var target = new ClassParser();
            var c1 = target.Parse(@"
public interface Interface1 { 
    private class MyClass2 {
    }
}");
            c1.Should().BeNull();
        }
    }
}
