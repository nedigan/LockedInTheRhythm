namespace EditorCools.Editor
{
    using System.Reflection;
    using System.Collections.Generic;
    using UnityEditor;
    using UnityEngine;

    public class Button
    {
        public readonly string DisplayName;
        public readonly MethodInfo Method;
        public readonly ButtonAttribute ButtonAttribute;
        public readonly string Tooltip;

        public Button(MethodInfo method, ButtonAttribute buttonAttribute)
        {
            ButtonAttribute = buttonAttribute;
            DisplayName = string.IsNullOrEmpty(buttonAttribute.Name)
                ? ObjectNames.NicifyVariableName(method.Name)
                : buttonAttribute.Name;
            Method = method;
            Tooltip = method.GetCustomAttribute<TooltipAttribute>()?.tooltip;
        }

        internal void Draw(IEnumerable<object> targets)
        {
            var isDisabled = false;
            if (!string.IsNullOrEmpty(ButtonAttribute.IsDisabledMethod))
            {
                var isDisabledMethod = Method.DeclaringType.GetMethod(ButtonAttribute.IsDisabledMethod, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (isDisabledMethod != null)
                {
                    foreach (var target in targets)
                    {
                        var isMethod = isDisabledMethod.Invoke(target, null);
                        if (isMethod is bool isMethodBool)
                        {
                            isDisabled = isMethodBool;
                            break;
                        }
                    }
                }
            }
            using (new EditorGUI.DisabledScope(isDisabled))
            {
                if (GUILayout.Button(DisplayName))
                {
                    foreach (object target in targets)
                    {
                        Method.Invoke(target, null);
                    }
                }
            }
        }
    }
}