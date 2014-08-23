using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Put the mic position at the end of the circular array,
/// use material offset
/// </summary>
public class Example3 : Example
{
    /// <summary>
    /// Update event
    /// </summary>
    protected override void ExampleUpdate()
    {
        try
        {
            base.ExampleUpdate();

            if (UsePlotter)
            {
                if (null != m_plotData &&
                    m_plotData.Length > 0)
                {
                    Vector2 pos = MaterialWave.mainTextureOffset;
                    pos.x = Mic.GetPosition();
                    RendererWave.material.mainTextureOffset = pos/(float) m_plotData.Length;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(string.Format("Update exception={0}", ex));
        }
    }
}