using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using ParserObjects.Visitors;

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

        [Test]
        public void AllParsersHaveBnfVisitorMethods()
        {
            var parserTypes = typeof(IParser).Assembly
                .GetTypes()
                .Where(t => t.IsPublic && !t.IsInterface && !t.IsAbstract && typeof(IParser).IsAssignableFrom(t))
                .ToList();

            var visitMethods1 = typeof(BnfStringifyVisitor)
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .ToList();

            var visitMethods = visitMethods1
                .Where(m => m.Name == "VisitTyped")
                .Select(m => new
                {
                    Method = m,
                    Parameters = m.GetParameters()
                })
                .Where(x => x.Parameters.Length == 2 && x.Parameters[1].ParameterType == typeof(BnfStringifyVisitor.State))
                .Select(x => x.Parameters[0].ParameterType)
                .ToList();

            var matches = new List<(Type type, Type methodType)>();
            foreach (var parserType in parserTypes)
            {
                var methodType = visitMethods.FirstOrDefault(pt =>
                    pt.Namespace == parserType.Namespace
                    && pt.Name == parserType.Name
                    && pt.DeclaringType == parserType.DeclaringType);
                matches.Add((parserType, methodType));
            }

            var typesWithoutMatchingMethods = matches.Where(x => x.methodType == null).Select(x => x.type).ToList();
            var error = new StringBuilder();
            error.AppendLine(":");
            foreach (var t in typesWithoutMatchingMethods)
                error.AppendLine(t.FullName);
            typesWithoutMatchingMethods.Should().BeEmpty(error.ToString());
        }
    }
}
