using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Read the mic wave and run spectrum analysis
/// </summary>
public class Example : MonoBehaviour
{
    public SpectrumMicrophone Mic = null;

    public bool ShowUsePlotter = true;

    Texture2D m_textureSpectrumLeft = null;
    Texture2D m_textureSpectrumRight = null;
    Texture2D m_textureWave = null;

    public Material MaterialSpectrumLeft = null;
    public Material MaterialSpectrumRight = null;
    public Material MaterialWave = null;

    public MeshRenderer RendererSpectrumLeft = null;
    public MeshRenderer RendererSpectrumRight = null;
    public MeshRenderer RendererWave = null;

    public int TextureSize = 16;

    protected Color32[] m_colorsSpectrumLeft = null;
    protected Color32[] m_colorsSpectrumRight = null;
    protected Color32[] m_colorsWave = null;

    protected GraphPlotter m_plotter = new GraphPlotter();

    public FFTWindow Window = FFTWindow.Rectangular;

    public bool NormalizeGraph = true;

    /// <summary>
    /// Imaginary spectrum
    /// </summary>
    public bool OverrideSpectrumImag = false;
    public float[] SpectrumImag = null;

    /// <summary>
    /// Toggle using plotter
    /// </summary>
    public bool UsePlotter = true;

    private int m_frames = 0;

    private float m_framesPerSecond = 0f;

    private DateTime m_timerFrames = DateTime.Now;

    void OnGUI()
    {
        try
        {
            ++m_frames;

            if (m_timerFrames < DateTime.Now)
            {
                if (m_frames == 0)
                {
                    m_framesPerSecond = 0f;
                }
                else
                {
                    m_framesPerSecond = m_frames;
                }
                m_timerFrames = DateTime.Now + TimeSpan.FromSeconds(1);
                m_frames = 0;
            }

            ExampleUpdate();

            GUILayout.Label(string.Format("FPS: {0:F2}", m_framesPerSecond));

            if (ShowUsePlotter &&
                GUILayout.Button("Use Plotter", GUILayout.MinHeight(40)))
            {
                UsePlotter = !UsePlotter;
            }

            if (RendererSpectrumLeft)
            {
                RendererSpectrumLeft.enabled = UsePlotter;
            }

            if (RendererSpectrumRight)
            {
                RendererSpectrumRight.enabled = UsePlotter;
            }

            if (RendererWave)
            {
                RendererWave.enabled = UsePlotter;
            }

            if (string.IsNullOrEmpty(Mic.DeviceName))
            {
                GUILayout.Space(150);

                GUILayout.Label(string.Format("Select a microphone: count={0}", Microphone.devices.Length));

                foreach (string device in Microphone.devices)
                {
                    if (string.IsNullOrEmpty(device))
                    {
                        continue;
                    }

                    if (GUILayout.Button(device, GUILayout.Height(60)))
                    {
                        Mic.DeviceName = device;
                    }
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("OnGUI exception={0}", ex));
        }
    }

    public void Init()
    {
        if (MaterialSpectrumLeft &&
            RendererSpectrumLeft)
        {
            RendererSpectrumLeft.material = (Material)UnityEngine.Object.Instantiate(MaterialSpectrumLeft);
        }

        if (MaterialSpectrumRight &&
            RendererSpectrumRight)
        {
            RendererSpectrumRight.material = (Material)UnityEngine.Object.Instantiate(MaterialSpectrumRight);
        }

        if (MaterialWave &&
            RendererWave)
        {
            RendererWave.material = (Material)UnityEngine.Object.Instantiate(MaterialWave);
        }

        if (null == m_textureSpectrumLeft &&
            RendererSpectrumLeft)
        {
            m_textureSpectrumLeft = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
            m_textureSpectrumLeft.wrapMode = TextureWrapMode.Clamp;
            m_textureSpectrumLeft.filterMode = FilterMode.Point;
            m_textureSpectrumLeft.anisoLevel = 0;
            RendererSpectrumLeft.material.mainTexture = m_textureSpectrumLeft;
            m_colorsSpectrumLeft = m_textureSpectrumLeft.GetPixels32();
        }

        if (null == m_textureSpectrumRight &&
            RendererSpectrumRight)
        {
            m_textureSpectrumRight = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
            m_textureSpectrumRight.wrapMode = TextureWrapMode.Clamp;
            m_textureSpectrumRight.filterMode = FilterMode.Point;
            m_textureSpectrumRight.anisoLevel = 0;
            RendererSpectrumRight.material.mainTexture = m_textureSpectrumRight;
            m_colorsSpectrumRight = m_textureSpectrumRight.GetPixels32();
        }

        if (null == m_textureWave &&
            RendererWave)
        {
            m_textureWave = new Texture2D(TextureSize, TextureSize, TextureFormat.ARGB32, false);
            m_textureWave.wrapMode = TextureWrapMode.Repeat;
            m_textureWave.filterMode = FilterMode.Point;
            m_textureWave.anisoLevel = 0;
            RendererWave.material.mainTexture = m_textureWave;
            m_colorsWave = m_textureWave.GetPixels32();
        }

        m_plotter.TextureSize = TextureSize;
    }

    public void CleanUp()
    {
        if (RendererSpectrumLeft &&
            null != RendererSpectrumLeft.material)
        {
            UnityEngine.Object.DestroyImmediate(RendererSpectrumLeft.material, true);
        }

        if (RendererSpectrumRight &&
            null != RendererSpectrumRight.material)
        {
            UnityEngine.Object.DestroyImmediate(RendererSpectrumRight.material, true);
        }

        if (RendererWave &&
            null != RendererWave.material)
        {
            UnityEngine.Object.DestroyImmediate(RendererWave.material, true);
        }

        if (RendererSpectrumLeft)
        {
            RendererSpectrumLeft.material = MaterialSpectrumLeft;
        }

        if (RendererSpectrumRight)
        {
            RendererSpectrumRight.material = MaterialSpectrumRight;
        }

        if (RendererWave)
        {
            RendererWave.material = MaterialWave;
        }

        if (m_textureSpectrumLeft)
        {
            UnityEngine.Object.DestroyImmediate(m_textureSpectrumLeft, true);
            m_textureSpectrumLeft = null;
            MaterialSpectrumLeft.mainTexture = null;
        }

        if (m_textureSpectrumRight)
        {
            UnityEngine.Object.DestroyImmediate(m_textureSpectrumRight, true);
            m_textureSpectrumRight = null;
            MaterialSpectrumRight.mainTexture = null;
        }

        if (m_textureWave)
        {
            UnityEngine.Object.DestroyImmediate(m_textureWave, true);
            m_textureWave = null;
            MaterialWave.mainTexture = null;
        }
    }

    void OnEnable()
    {
        Init();
    }

    void OnApplicationQuit()
    {
        CleanUp();
    }

    protected float[] m_micData = null;
    protected float[] m_plotData = null;

    protected virtual void GetMicData()
    {
        m_micData = Mic.GetData(0);
    }

    protected virtual void PlotWave()
    {
        if (UsePlotter)
        {
            float min, max;
            m_plotter.PlotGraph(m_micData, m_plotData, m_micData.Length, NormalizeGraph, out min, out max, true,
                                m_colorsWave);
            if (NormalizeGraph)
            {
                m_plotter.Min = Mathf.Lerp(m_plotter.Min, min, 0.1f);
                m_plotter.Max = Mathf.Lerp(m_plotter.Max, max, 0.1f);
            }
        }
    }

    protected virtual void PlotSpectrum()
    {
        float[] spectrumReal;
        float[] spectrumImag;
        Mic.GetSpectrumData(Window, out spectrumReal, out spectrumImag);
        if (UsePlotter)
        {
            m_plotter.PlotGraph2(spectrumReal, m_plotData, 0, spectrumReal.Length, NormalizeGraph, m_colorsSpectrumLeft);

            if (OverrideSpectrumImag &&
                null != SpectrumImag)
            {
                m_plotter.PlotGraph2(SpectrumImag, m_plotData, 0, SpectrumImag.Length, NormalizeGraph,
                                     m_colorsSpectrumRight);
            }
            else
            {
                m_plotter.PlotGraph2(spectrumImag, m_plotData, 0, spectrumImag.Length, NormalizeGraph,
                                     m_colorsSpectrumRight);
            }

            //float min, max;
            //m_plotter.PlotGraph(spectrumData, m_plotData, spectrumData.Length, NormalizeGraph, out min, out max, false, m_colorsSpectrumRight);

            if (m_textureSpectrumLeft &&
                null != m_colorsSpectrumLeft)
            {
                m_textureSpectrumLeft.SetPixels32(m_colorsSpectrumLeft);
                m_textureSpectrumLeft.Apply();
            }

            if (m_textureSpectrumRight &&
                null != m_colorsSpectrumRight)
            {
                m_textureSpectrumRight.SetPixels32(m_colorsSpectrumRight);
                m_textureSpectrumRight.Apply();
            }

            if (m_textureWave &&
                null != m_colorsWave)
            {
                m_textureWave.SetPixels32(m_colorsWave);
                m_textureWave.Apply();
            }
        }
    }

    protected virtual void ExampleUpdate()
    {
        try
        {
            GetMicData();
            if (null == m_micData)
            {
                return;
            }
            if (null == m_plotData ||
                m_plotData.Length != m_micData.Length)
            {
                m_plotData = new float[m_micData.Length];
            }
            
            PlotWave();

            PlotSpectrum();
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("Update exception={0}", ex));
        }
    }
}