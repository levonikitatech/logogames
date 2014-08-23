using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WordDetection))]
public class WordDetectionInspector : Editor
{
    [MenuItem("GameObject/Create Other/Audio/Add Word Detection")]
    public static void MenuCreateMic()
    {
        GameObject go = new GameObject("WordDetection");
        go.AddComponent<WordDetection>();
    }

    public override void OnInspectorGUI()
    {
        WordDetection item = target as WordDetection;

        item.Mic = (SpectrumMicrophone)EditorGUILayout.ObjectField("Spectrum Microphone:", item.Mic, typeof(SpectrumMicrophone), true);

        if (null != item.Mic)
        {
            int size = item.Mic.CaptureTime * item.Mic.SampleRate;

            GUILayout.BeginHorizontal();
            GUILayout.Label("Threshold:");
            item.Threshold = (int)GUILayout.HorizontalSlider(item.Threshold, 1, size);
            item.Threshold = EditorGUILayout.IntField(item.Threshold);
            item.Threshold = Mathf.Min(item.Threshold, size);
            item.Threshold = Mathf.Max(item.Threshold, 1);
            GUILayout.EndHorizontal();
            
            item.UsePushToTalk = GUILayout.Toggle(item.UsePushToTalk, "Use Push To Talk");
        }
    }
}