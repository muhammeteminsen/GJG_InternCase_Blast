using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var methods = target.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttributes(typeof(ButtonAttribute), true).Length > 0);

        foreach (var method in methods)
        {
            var buttonAttribute = (ButtonAttribute)method.GetCustomAttributes(typeof(ButtonAttribute), true)[0];
            string buttonName = string.IsNullOrEmpty(buttonAttribute.ButtonName)
                ? method.Name
                : buttonAttribute.ButtonName;

            if (GUILayout.Button(buttonName))
            {
                method.Invoke(target, null);
            }
        }
    }
}