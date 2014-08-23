using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Put the mic position at the end of the circular array,
/// manually shift the array
/// </summary>
public class Example2 : Example
{
    float[] m_copyData = null;

    protected override void GetMicData()
    {
        int position = Mic.GetPosition();
        int size = Mic.CaptureTime * Mic.SampleRate;
        m_micData = Mic.GetData(0);

        if (null == m_copyData ||
            m_copyData.Length != size)
        {
            m_copyData = new float[size];
        }

        //shift array
        for (int index = 0, i = position; index < size; ++index, i = (i + 1) % size)
        {
            m_copyData[index] = m_micData[i];
        }

        //replace reference with copy
        m_micData = m_copyData;
    }
}