using System.Collections.Generic;

namespace ParserObjects.Tests.Examples.Classes
{
    public class Definition
    {
        public Definition(string name)
        {
            Name = name;
        }

        public string AccessModifier { get; set;  }
        public string StructureType { get; set; }
        public string Name { get; }

        public List<Definition> Children { get; set; }

        public bool Validate()
        {
            var hasRequiredFields = !string.IsNullOrEmpty(AccessModifier)
                && !string.IsNullOrEmpty(StructureType)
                && !string.IsNullOrEmpty(Name);
            if (!hasRequiredFields)
                return false;

            return Children != null;
        }
    }
}
