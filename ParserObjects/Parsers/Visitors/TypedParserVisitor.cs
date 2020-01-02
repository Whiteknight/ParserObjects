using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ParserObjects.Parsers.Visitors
{
    public sealed class TypedParserVisitor : IParserVisitor
    {
        private const string DefaultVisitHandlerName = "Visit";
        private readonly Dictionary<Type, Func<IParser, IParser>> _handlers;
        private readonly Func<IParser, IParser> _defaultHandler;

        public TypedParserVisitor()
        {
            _handlers = new Dictionary<Type, Func<IParser, IParser>>();
            _defaultHandler = p => p;
        }

        public TypedParserVisitor(Func<IParser, IParser> defaultHandler)
        {
            _handlers = new Dictionary<Type, Func<IParser, IParser>>();
            _defaultHandler = defaultHandler ?? (p => p);
        }

        public IParser Visit(IParser parser) => VisitInternal(parser, new Dictionary<IParser, IParser>());

        private IParser VisitInternal(IParser parser, IDictionary<IParser, IParser> seen)
        {
            if (seen.ContainsKey(parser))
                return seen[parser];

            var newParser = DispatchVisitHandler(parser);
            seen.Add(parser, newParser);

            foreach (var child in newParser.GetChildren())
            {
                var newChild = VisitInternal(child, seen);
                if (newChild != child)
                {
                    Debug.WriteLine("Replacing " + child);
                    newParser = newParser.ReplaceChild(child, newChild);
                    newParser.Name = parser.Name;
                }
            }

            return newParser;
        }

        public IParser DispatchVisitHandler(IParser parser)
        {
            var type = parser.GetType();

            // Check base types first
            while (true)
            {
                if (_handlers.ContainsKey(type))
                    return _handlers[type](parser);
                type = type.BaseType;
                if (type == null || type == typeof(object))
                    break;
            }

            // Check interface types second
            var interfaceTypes = type.GetInterfaces().Where(i => typeof(IParser).IsAssignableFrom(i)).ToList();
            foreach (var interfaceType in interfaceTypes)
            {
                if (_handlers.ContainsKey(interfaceType))
                    return _handlers[interfaceType](parser);
            }

            return _defaultHandler(parser);
        }

        public void AddHandler<TParser>(Func<TParser, IParser> handler)
        {
            _handlers.Add(typeof(TParser), p => handler((TParser) p));
        }

        public void FindVisitHandlers(object instance, string methodName = DefaultVisitHandlerName)
        {
            var methods = instance.GetType()
                .GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.Name.Equals(methodName, StringComparison.OrdinalIgnoreCase))
                .Where(IsVisitHandlerMethod)
                .ToList();

            foreach (var method in methods)
            {
                var firstParameterType = method.GetParameters()[0].ParameterType;
                var parameter = Expression.Parameter(typeof(IParser), "p");
                var handlerExpression = Expression.Lambda<Func<IParser, IParser>>(
                    Expression.Convert(
                        Expression.Call(
                            Expression.Constant(instance),
                            method,
                            Expression.Convert(parameter, firstParameterType)
                        ),
                        typeof(IParser)
                    ),
                    parameter
                );
                var handler = handlerExpression.Compile();
                _handlers.Add(firstParameterType, handler);
            }
        }

        private static bool IsVisitHandlerMethod(MethodInfo x)
        {
            var parameters = x.GetParameters();
            if (x.ReturnType != typeof(IParser))
                return false;
            if (parameters.Length != 1 || parameters[0].ParameterType != typeof(IParser) || typeof(IParser).IsAssignableFrom(parameters[0].ParameterType))
                return false;
            return true;
        }
    }
}
