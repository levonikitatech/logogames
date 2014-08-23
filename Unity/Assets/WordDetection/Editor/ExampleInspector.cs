using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Example))]
public class ExampleInspector : Editor
{
    public override void OnInspectorGUI()
    {
        Example item = target as Example;

        item.UsePlotter = GUILayout.Toggle(item.UsePlotter, "Use Plotter");

        item.ShowUsePlotter = GUILayout.Toggle(item.ShowUsePlotter, "Show Use Plotter");

        int textureSize = item.TextureSize;

        GUILayout.BeginHorizontal();
        GUILayout.Label("Texture Size:");
        item.TextureSize = (int)GUILayout.HorizontalSlider(item.TextureSize, 4, 512);
        item.TextureSize = EditorGUILayout.IntField(item.TextureSize);
        int log = (int)Mathf.Log(item.TextureSize, 2);
        item.TextureSize = (int)Mathf.Pow(2, log);
        item.TextureSize = Mathf.Min(item.TextureSize, 512);
        item.TextureSize = Mathf.Max(item.TextureSize, 4);
        GUILayout.EndHorizontal();

        item.Mic = (SpectrumMicrophone)EditorGUILayout.ObjectField("Spectrum Microphone:", item.Mic, typeof(SpectrumMicrophone), true);

        item.MaterialSpectrumLeft = (Material)EditorGUILayout.ObjectField("Material Spectrum Left:", item.MaterialSpectrumLeft, typeof(Material), false);
        item.MaterialSpectrumRight = (Material)EditorGUILayout.ObjectField("Material Spectrum Right:", item.MaterialSpectrumRight, typeof(Material), false);
        item.MaterialWave = (Material)EditorGUILayout.ObjectField("Material Wave:", item.MaterialWave, typeof(Material), false);

        item.RendererSpectrumLeft = (MeshRenderer)EditorGUILayout.ObjectField("Renderer Spectrum Left:", item.RendererSpectrumLeft, typeof(MeshRenderer), true);
        item.RendererSpectrumRight = (MeshRenderer)EditorGUILayout.ObjectField("Renderer Spectrum Right:", item.RendererSpectrumRight, typeof(MeshRenderer), true);
        item.RendererWave = (MeshRenderer)EditorGUILayout.ObjectField("Renderer Wave:", item.RendererWave, typeof(MeshRenderer), true);

        GUILayout.BeginHorizontal();
        GUILayout.Label("FFT Window:");
        item.Window = (FFTWindow)EditorGUILayout.EnumPopup(item.Window);
        GUILayout.EndHorizontal();

        item.NormalizeGraph = GUILayout.Toggle(item.NormalizeGraph, "Normalize Graph");

        if (textureSize != item.TextureSize)
        {
            if (EditorApplication.isPlaying)
            {
                item.CleanUp();
                item.Init();
            }
        }
    }
}