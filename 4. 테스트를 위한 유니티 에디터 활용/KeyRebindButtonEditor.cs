// Editor/KeyRebindButtonEditor.cs
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(KeyRebindButton))]
public class KeyRebindButtonEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        KeyRebindButton button = (KeyRebindButton)target;

        if (GUILayout.Button("자동 인덱스 매핑"))
        {
            string name = button.gameObject.name.ToLower();

            if (name.Contains("up")) button.bindingIndex = 1;
            else if (name.Contains("down")) button.bindingIndex = 2;
            else if (name.Contains("left")) button.bindingIndex = 3;
            else if (name.Contains("right")) button.bindingIndex = 4;
            else Debug.LogWarning($"[{button.name}] 자동 인덱스 실패: 이름에 방향이 포함되지 않음");

            Debug.Log($"[{button.name}] 자동 설정된 bindingIndex = {button.bindingIndex}");
            EditorUtility.SetDirty(button);
        }
    }
}
