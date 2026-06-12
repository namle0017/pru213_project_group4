using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DailyRewardController))]
public class DailyRewardControllerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        DailyRewardController controller = (DailyRewardController)target;

        EditorGUILayout.Space(12f);
        EditorGUILayout.LabelField("Daily Reward Debug", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Current Day", controller.CurrentDayForDebug.ToString());
        EditorGUILayout.LabelField("Last Claim Date", string.IsNullOrEmpty(controller.LastClaimDateForDebug) ? "(none)" : controller.LastClaimDateForDebug);
        EditorGUILayout.LabelField("Claimed Today", controller.HasClaimedTodayForDebug ? "Yes" : "No");
        EditorGUILayout.LabelField("Claimed Mask", controller.ClaimedMaskForDebug.ToString());

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Fast Test", EditorStyles.boldLabel);

        if (GUILayout.Button("Claim Today Reward"))
        {
            controller.ClaimTodayReward();
            MarkDirty(controller);
        }

        if (GUILayout.Button("Simulate New Day"))
        {
            controller.DevSimulateNewDay();
            MarkDirty(controller);
        }

        if (GUILayout.Button("Reset Daily Reward"))
        {
            controller.DevResetDailyReward();
            MarkDirty(controller);
        }

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Set Available Day", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        DrawDayButton(controller, 1);
        DrawDayButton(controller, 2);
        DrawDayButton(controller, 3);
        DrawDayButton(controller, 4);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        DrawDayButton(controller, 5);
        DrawDayButton(controller, 6);
        DrawDayButton(controller, 7);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(8f);
        EditorGUILayout.LabelField("Motor Reward Test", EditorStyles.boldLabel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Lock Motor"))
        {
            controller.DevLockMotor();
            MarkDirty(controller);
        }

        if (GUILayout.Button("Unlock Motor"))
        {
            controller.DevUnlockMotor();
            MarkDirty(controller);
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Add 1000 Coins"))
        {
            controller.DevAddCoins(1000);
            MarkDirty(controller);
        }

        if (GUILayout.Button("Refresh Daily UI"))
        {
            controller.DevRefresh();
            MarkDirty(controller);
        }
    }

    private static void DrawDayButton(DailyRewardController controller, int day)
    {
        if (!GUILayout.Button("Day " + day))
        {
            return;
        }

        controller.DevSetDayAvailable(day);
        MarkDirty(controller);
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
