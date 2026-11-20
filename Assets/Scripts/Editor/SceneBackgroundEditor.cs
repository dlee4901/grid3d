using UnityEditor;

[CustomEditor(typeof(SceneBackground))]
public class SceneBackgroundEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var skyboxes = serializedObject.FindProperty("_skyboxes");
        var selected = serializedObject.FindProperty("_selected");
        if (skyboxes.arraySize > 0)
        {
            var names = new string[skyboxes.arraySize];
            for (var i = 0; i < names.Length; i++)
            {
                var skybox = skyboxes.GetArrayElementAtIndex(i).objectReferenceValue;
                names[i] = skybox != null ? skybox.name : "(empty)";
            }
            selected.intValue = EditorGUILayout.Popup("Background", selected.intValue, names);
        }
        EditorGUILayout.PropertyField(skyboxes, true);
        serializedObject.ApplyModifiedProperties();
    }
}
