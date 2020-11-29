using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ParserObjects.Tests.Parsers
{
    public class ParserTests
    {
        [Test]
        public void AllParsersHaveToStringOverride()
        {
            var parserTypes = typeof(IParser).Assembly
                .GetTypes()
                .Where(t => !t.IsInterface && !t.IsAbstract && typeof(IParser).IsAssignableFrom(t))
                .ToList();
            foreach (var parserType in parserTypes)
            {
                var toStringMethod = parserType.GetMethod(nameof(ToString), BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
                toStringMethod.Should().NotBeNull($"Type {parserType.FullName} does not have a .ToString method override");
                    
            }
        }
    }
}
