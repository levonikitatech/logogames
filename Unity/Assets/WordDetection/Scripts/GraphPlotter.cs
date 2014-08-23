using System;
using UnityEngine;

/// <summary>
/// Handle displaying plot data in a texture
/// </summary>
public class GraphPlotter
{
    public Color BackgroundColor = new Color(1, 0, 0, 0.1f);

    public float Max = 0f;
    public float Min = 0f;

    public int TextureSize = 16;

    public void PlotGraph(float[] data, float[] plotData, int size, bool normalizeGraph, out float min, out float max, bool useMinMax, Color32[] colors)
    {
        min = 0;
        max = 0;

        float index;
        int i;

        //normalize the graph
        float offset = Mathf.Max(1, size / (float)TextureSize);
        //Debug.Log(string.Format("Offset: {0}", offset));

        for (index = 0, i = 0; i < size; index += offset, i = (int)index)
        {
            plotData[i] = data[i];
        }

        if (normalizeGraph)
        {
            for (index = 0, i = 0; i < size; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (val > 0f)
                {
                    if (val > max)
                    {
                        max = val;
                    }
                }
                else
                {
                    val = -val;
                    if (val > min)
                    {
                        min = val;
                    }
                }
            }

            //Debug.Log(string.Format("Min: {0}", min));
            //Debug.Log(string.Format("Max: {0}", max));

            if (useMinMax)
            {
                for (index = 0, i = 0; i < size; index += offset, i = (int)index)
                {
                    float val = plotData[i];
                    if (val > 0f)
                    {
                        if (Max != 0f)
                        {
                            val /= Max;
                        }
                    }
                    else
                    {
                        if (Min != 0f)
                        {
                            val /= Min;
                        }
                    }

                    plotData[i] = val;
                }
            }
            else
            {
                for (index = 0, i = 0; i < size; index += offset, i = (int)index)
                {
                    float val = plotData[i];
                    if (val > 0f)
                    {
                        if (max != 0f)
                        {
                            val /= max;
                        }
                    }
                    else
                    {
                        if (min != 0f)
                        {
                            val /= min;
                        }
                    }

                    plotData[i] = val;
                }
            }
        }

        int textureSizeHalf = TextureSize / 2;
        int textureSizeHalfMinusOne = textureSizeHalf - 1;

        int x;
        for (x = 0, index = 0, i = 0; i < size && x < TextureSize; index += offset, i = (int)index, ++x)
        {
            float val = plotData[i];
            int v = (int)(val * textureSizeHalf);
            int y = textureSizeHalf;
            while (y >= 0)
            {
                float t = y / (float)textureSizeHalf;
                if (v > 0 &&
                    y <= v)
                {
                    SetColors(colors, x, textureSizeHalf + y, Color.Lerp(Color.red, Color.white, t));
                    SetColors(colors, x, textureSizeHalfMinusOne - y, BackgroundColor);
                }
                else if (v < 0 &&
                    y <= -v)
                {
                    SetColors(colors, x, textureSizeHalf + y, BackgroundColor);
                    SetColors(colors, x, textureSizeHalfMinusOne - y, Color.Lerp(Color.magenta, Color.yellow, t));
                }
                else
                {
                    SetColors(colors, x, textureSizeHalfMinusOne + y, BackgroundColor);
                    SetColors(colors, x, textureSizeHalf - y, BackgroundColor);
                }
                --y;
            }
        }
    }

    public void PlotGraph2(float[] data, float[] plotData, int beginIndex, int endIndex, bool normalizeGraph, Color32[] colors)
    {
        float index;
        int i;
        int size = endIndex - beginIndex + 1;

        //normalize the graph
        float offset = size / (float)TextureSize;
        //Debug.Log(string.Format("Offset: {0}", offset));

        for (index = beginIndex, i = beginIndex; i < endIndex; index += offset, i = (int)index)
        {
            plotData[i] = Mathf.Abs(data[i]);
        }

        if (normalizeGraph)
        {
            float max = 0;
            for (index = beginIndex, i = beginIndex; i < endIndex; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (val > max)
                {
                    max = val;
                }
            }

            //Debug.Log(string.Format("Min: {0}", min));
            //Debug.Log(string.Format("Max: {0}", max));

            for (index = beginIndex, i = beginIndex; i < endIndex; index += offset, i = (int)index)
            {
                float val = plotData[i];
                if (max != 0f)
                {
                    val /= max;
                }

                plotData[i] = val;
            }
        }

        int x;
        for (x = 0, index = beginIndex, i = beginIndex; i < endIndex && x < TextureSize; index += offset, i = (int)index, ++x)
        {
            float val = plotData[i];
            int v = (int)(val * TextureSize);
            int y = TextureSize - 1;
            while (y >= 0)
            {
                float t = y / (float)TextureSize;
                if (v > 0 &&
                    y <= v)
                {
                    SetColors(colors, x, y, Color.Lerp(Color.red, Color.white, t));
                }
                else
                {
                    SetColors(colors, x, y, BackgroundColor);
                }
                --y;
            }
        }
    }

    void SetColors(Color32[] colors, int x, int y, Color color)
    {
        if (null == colors)
        {
            return;
        }

        int index = x + y * TextureSize;
        if (index >= 0 &&
            (index < TextureSize * TextureSize))
        {
            colors[index] = color;
        }
    }    
}