#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SetGameButton))]
[CanEditMultipleObjects]
[System.Serializable]
public class SetGameButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        SetGameButton myScript = target as SetGameButton;

        switch (myScript.ButtonType)
        {
            case SetGameButton.EButtonType.DifficultyBtn:
                myScript.PairNum = (GameSettings.EPairNumber) EditorGUILayout.EnumPopup("Difficulty", myScript.PairNum);
                break;
            case SetGameButton.EButtonType.TopicsBtn:
                myScript.Topics = (GameSettings.EPuzzleTopics)EditorGUILayout.EnumPopup("Puzzle Topics", myScript.Topics);
                break;
        }

        if(GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }

   
}
#endif
