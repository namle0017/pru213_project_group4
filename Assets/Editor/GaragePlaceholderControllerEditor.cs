using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GaragePlaceholderController))]
public class GaragePlaceholderControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Garage Test Buttons", EditorStyles.boldLabel);

        GaragePlaceholderController controller = (GaragePlaceholderController)target;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock F1"))
        {
            controller.UnlockF1ForTest();
            MarkDirty(controller);
        }

        if (GUILayout.Button("Lock F1"))
        {
            controller.LockF1ForTest();
            MarkDirty(controller);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Unlock Motor"))
        {
            controller.UnlockMotorForTest();
            MarkDirty(controller);
        }

        if (GUILayout.Button("Lock Motor"))
        {
            controller.LockMotorForTest();
            MarkDirty(controller);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Refresh Garage"))
        {
            controller.RefreshGarageForTest();
            MarkDirty(controller);
        }
    }

    private static void MarkDirty(Object targetObject)
    {
        EditorUtility.SetDirty(targetObject);

        if (!Application.isPlaying)
        {
            AssetDatabase.SaveAssets();
        }
    }
}
