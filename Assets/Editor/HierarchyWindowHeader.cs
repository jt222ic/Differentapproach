using UnityEngine;
using UnityEditor;

//Simply re-styles a gameObject name in the Hiearchy window to be black and all caps.
//Allows us to seperate our gameObjects and not lose our minds.

[InitializeOnLoad]
public static class HierarchySectionHeader
{
    static HierarchySectionHeader()
    {
        EditorApplication.hierarchyWindowItemOnGUI += HierarchyWindowItemOnGUI;
       
    }

    static void HierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
    {
        var gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

        if (gameObject != null && gameObject.name.StartsWith("//", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.black);
            EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("/", "").ToUpperInvariant());
        }

        if (gameObject != null && gameObject.name.StartsWith("*", System.StringComparison.Ordinal))
        {
            EditorGUI.DrawRect(selectionRect, Color.red);
            EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("*", "").ToUpper());
            
        }

        //if (gameObject != null && gameObject.name.EndsWith("**", System.StringComparison.Ordinal))
        //{
        //    EditorGUI.DrawRect(selectionRect, Color.red);
        //    EditorGUI.DropShadowLabel(selectionRect, gameObject.name.Replace("**", "").ToUpperInvariant());
        //    //EditorGUI.ColorField(selectionRect, Color.blue);
        //    //EditorGUI.HelpBox(selectionRect, gameObject.name, MessageType.Info);
        //}
    }
}
