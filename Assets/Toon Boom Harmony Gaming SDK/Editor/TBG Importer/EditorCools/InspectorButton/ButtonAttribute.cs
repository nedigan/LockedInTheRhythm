namespace EditorCools
{
    using System;

    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public sealed class ButtonAttribute : Attribute
    {
        public readonly string Name;
        public readonly string Row;
        public readonly float Space;
        public readonly bool HasRow;
        public string IsDisabledMethod;
        public ButtonAttribute(string name = default, string row = default, float space = default, string isDisabledMethod = default)
        {
            Row = row;
            HasRow = !string.IsNullOrEmpty(Row);
            Name = name;
            Space = space;
            IsDisabledMethod = isDisabledMethod;
        }
    }
}

