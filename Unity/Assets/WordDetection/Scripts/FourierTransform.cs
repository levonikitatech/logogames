using UnityEngine;

/// <summary>
/// Calculate fourier transforms
/// </summary>
public class FourierTransform
{
    /// <summary>
    /// Transform instance
    /// </summary>
    FFTParam m_fft = null;

    /// <summary>
    /// Calculate the fourier transform on the sample length
    /// </summary>
    /// <param name="samples"></param>
    /// <param name="complex"></param>
    /// <param name="spectrumReal"></param>
    /// <param name="spectrumImag"></param>
    public void FFT(float[] samples, float[] complex, float[] spectrumReal, float[] spectrumImag)
    {
        if (null == samples ||
            null == complex ||
            null == spectrumReal ||
            null == spectrumImag)
        {
            return;
        }

        int size = samples.Length;
        int halfSize = size / 2;

        if (complex.Length != size)
        {
            return;
        }

        if (spectrumReal.Length != halfSize)
        {
            return;
        }

        if (spectrumImag.Length != halfSize)
        {
            return;
        }

        if (null == m_fft)
        {
            m_fft = InitializeFFT(size);
        }
        else
        {
            InitializeFFT(m_fft, size);
        }

        for (int index = 0; index < size; ++index)
        {
            complex[index] = samples[index];
        }
        RealFFTf(complex, m_fft);

        for (int index = 1; index < halfSize; ++index)
        {
            spectrumReal[index] = complex[m_fft.BitReversed[index]];
            spectrumImag[index] = complex[m_fft.BitReversed[index] + 1];
        }
    }

    const float M_PI = 3.14159265358979323846f;  // pi

    class FFTParam
    {
        public int[] BitReversed;
        public float[] SinTable;
        public int Points;
    }

    /// <summary>
    /// Instantiate a fourier transform instance
    /// </summary>
    /// <param name="fftlen"></param>
    /// <returns></returns>
    FFTParam InitializeFFT(int fftlen)
    {
        FFTParam h = new FFTParam();
        InitializeFFT(h, fftlen);
        return h;
    }

    /// <summary>
    /// Initialize an existing fourier transform
    /// </summary>
    /// <param name="h"></param>
    /// <param name="fftlen"></param>
    void InitializeFFT(FFTParam h, int fftlen)
    {
        int i;
        int temp;
        int mask;

        // FFT size is only half the number of data points
        // The full FFT output can be reconstructed from this FFT's output.
        // (This optimization can be made since the data is real.)
        h.Points = fftlen / 2;
        if (null == h.SinTable ||
            h.SinTable.Length != fftlen)
        {
            h.SinTable = new float[fftlen];
        }
        if (null == h.BitReversed ||
            h.BitReversed.Length != h.Points)
        {
            h.BitReversed = new int[h.Points];
        }

        for (i = 0; i < h.Points; ++i)
        {
            temp = 0;
            for (mask = h.Points / 2; mask > 0; mask >>= 1)
                temp = (temp >> 1) + (((i & mask) != 0) ? h.Points : 0);

            h.BitReversed[i] = temp;
        }

        for (i = 0; i < h.Points; ++i)
        {
            h.SinTable[h.BitReversed[i]] = -Mathf.Sin(2 * M_PI * i / (2 * h.Points));
            h.SinTable[h.BitReversed[i] + 1] = -Mathf.Cos(2 * M_PI * i / (2 * h.Points));
        }
    }

    /// <summary>
    /// Free up the memory allotted for Sin table and Twiddle Pointers
    /// </summary>
    /// <param name="h"></param>
    void EndFFT(FFTParam h)
    {
        if (h.Points > 0)
        {
            h.BitReversed = null;
            h.SinTable = null;
        }
        h.Points = 0;
    }

    const int MAX_HFFT = 10;
    static FFTParam[] hFFTArray = new FFTParam[MAX_HFFT];
    static int[] nFFTLockCount = new int[MAX_HFFT];

    /// <summary>
    /// Get a handle to the FFT tables of the desired length,
    /// This version keeps common tables rather than allocating a new table every time
    /// </summary>
    /// <param name="fftlen"></param>
    /// <returns></returns>
    FFTParam GetFFT(int fftlen)
    {
        int h, n = fftlen / 2;
        for (h = 0; (h < MAX_HFFT) && (hFFTArray[h] != null) && (n != hFFTArray[h].Points); ++h) ;
        if (h < MAX_HFFT)
        {
            if (hFFTArray[h] == null)
            {
                hFFTArray[h] = InitializeFFT(fftlen);
                nFFTLockCount[h] = 0;
            }
            ++nFFTLockCount[h];
            return hFFTArray[h];
        }
        else
        {
            // All buffers used, so fall back to allocating a new set of tables
            return InitializeFFT(fftlen); ;
        }
    }

    /// <summary>
    /// Release a previously requested handle to the FFT tables
    /// </summary>
    /// <param name="hFFT"></param>
    void ReleaseFFT(FFTParam hFFT)
    {
        int h;
        for (h = 0; (h < MAX_HFFT) && (hFFTArray[h] != hFFT); ++h) ;
        if (h < MAX_HFFT)
        {
            nFFTLockCount[h]--;
        }
        else
        {
            EndFFT(hFFT);
        }
    }

    /// <summary>
    /// Deallocate any unused FFT tables
    /// </summary>
    void CleanupFFT()
    {
        int h;
        for (h = 0; (h < MAX_HFFT); ++h)
        {
            if ((nFFTLockCount[h] <= 0) && (hFFTArray[h] != null))
            {
                EndFFT(hFFTArray[h]);
                hFFTArray[h] = null;
            }
        }
    }

    /// <summary>
    ///  Forward FFT routine.  Must call InitializeFFT(fftlen) first!
    ///
    ///  Note: Output is BIT-REVERSED! so you must use the BitReversed to
    ///        get legible output, (i.e. Real_i = buffer[ h.BitReversed[i] ]
    ///                                  Imag_i = buffer[ h.BitReversed[i]+1 ] )
    ///        Input is in normal order.
    ///
    /// Output buffer[0] is the DC bin, and output buffer[1] is the Fs/2 bin
    /// - this can be done because both values will always be real only
    /// - this allows us to not have to allocate an extra complex value for the Fs/2 bin
    ///
    ///  Note: The scaling on this is done according to the standard FFT definition,
    ///        so a unit amplitude DC signal will output an amplitude of (N)
    ///        (Older revisions would progressively scale the input, so the output
    ///        values would be similar in amplitude to the input values, which is
    ///        good when using fixed point arithmetic)
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="h"></param>
    void RealFFTf(float[] buffer, FFTParam h)
    {
        int A;
        int B;
        int endptr1;
        int endptr2;
        int br1;
        int br2;
        float HRplus, HRminus, HIplus, HIminus;
        float v1, v2, sin, cos;

        int ButterfliesPerGroup = h.Points / 2;

        //  Butterfly:
        //     Ain-----Aout
        //         \ /
        //         / \
        //     Bin-----Bout

        endptr1 = h.Points * 2;

        while (ButterfliesPerGroup > 0)
        {
            A = 0;
            B = ButterfliesPerGroup * 2;
            int sptr = 0;

            while (A < endptr1)
            {
                sin = h.SinTable[sptr];
                cos = h.SinTable[sptr + 1];
                endptr2 = B;
                while (A < endptr2)
                {
                    v1 = buffer[B] * cos + buffer[B + 1] * sin;
                    v2 = buffer[B] * sin - buffer[B + 1] * cos;
                    buffer[B] = buffer[A] + v1;
                    buffer[A] = buffer[B] - 2 * v1;
                    ++A;
                    ++B;
                    buffer[B] = buffer[A] - v2;
                    buffer[A] = buffer[B] + 2 * v2;
                    ++A;
                    ++B;
                }
                A = B;
                B += ButterfliesPerGroup * 2;
                sptr += 2;
            }
            ButterfliesPerGroup >>= 1;
        }
        
        // Massage output to get the output for a real input sequence.
        br1 = 1;
        br2 = h.Points - 1;

        while (br1 < br2)
        {
            sin = h.SinTable[h.BitReversed[br1]];
            cos = h.SinTable[h.BitReversed[br1] + 1];
            A = h.BitReversed[br1];
            B = h.BitReversed[br2];
            HRplus = (HRminus = buffer[A] - buffer[B]) + (buffer[B] * 2);
            HIplus = (HIminus = buffer[A + 1] - buffer[B + 1]) + (buffer[B + 1] * 2);
            v1 = (sin * HRminus - cos * HIplus);
            v2 = (cos * HRminus + sin * HIplus);
            buffer[A] = (HRplus + v1) * 0.5f;
            buffer[B] = buffer[A] - v1;
            buffer[(A + 1)] = (HIminus + v2) * 0.5f;
            buffer[B + 1] = buffer[A + 1] - HIminus;

            br1++;
            br2--;
        }
        
        // Handle the center bin (just need a conjugate)
        A = h.BitReversed[br1] + 1;
        buffer[A] = -buffer[A];
        
        // Handle DC bin separately - and ignore the Fs/2 bin
        //buffer[0]+=buffer[1];
        //buffer[1]=0f;
        
        // Handle DC and Fs/2 bins separately
        // Put the Fs/2 value into the imaginary part of the DC bin
        v1 = buffer[0] - buffer[1];
        buffer[0] += buffer[1];
        buffer[1] = v1;
    }


    /// <summary>
    ///  Description: This routine performs an inverse FFT to real data.
    ///               This code is for floating point data.
    /// 
    ///   Note: Output is BIT-REVERSED! so you must use the BitReversed to
    ///         get legible output, (i.e. wave[2*i]   = buffer[ BitReversed[i] ]
    ///                                   wave[2*i+1] = buffer[ BitReversed[i]+1 ] )
    ///         Input is in normal order, interleaved (real,imaginary) complex data
    ///         You must call InitializeFFT(fftlen) first to initialize some buffers!
    /// 
    ///  Input buffer[0] is the DC bin, and input buffer[1] is the Fs/2 bin
    ///  - this can be done because both values will always be real only
    ///  - this allows us to not have to allocate an extra complex value for the Fs/2 bin
    /// 
    ///   Note: The scaling on this is done according to the standard FFT definition,
    ///         so a unit amplitude DC signal will output an amplitude of (N)
    ///         (Older revisions would progressively scale the input, so the output
    ///         values would be similar in amplitude to the input values, which is
    ///         good when using fixed point arithmetic)
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="h"></param>
    void InverseRealFFTf(float[] buffer, FFTParam h)
    {
        int A;
        int B;
        int endptr1;
        int endptr2;
        int br1;
        float HRplus, HRminus, HIplus, HIminus;
        float v1, v2, sin, cos;

        int ButterfliesPerGroup = h.Points / 2;

        // Massage input to get the input for a real output sequence.
        A = 2;
        B = h.Points * 2 - 2;
        br1 = 1;
        while (A < B)
        {
            sin = h.SinTable[h.BitReversed[br1]];
            cos = h.SinTable[h.BitReversed[br1] + 1];
            HRplus = (HRminus = buffer[A] - buffer[B]) + (buffer[B] * 2);
            HIplus = (HIminus = buffer[A + 1] - buffer[B + 1]) + (buffer[B + 1] * 2);
            v1 = (sin * HRminus + cos * HIplus);
            v2 = (cos * HRminus - sin * HIplus);
            buffer[A] = (HRplus + v1) * 0.5f;
            buffer[B] = buffer[A] - v1;
            buffer[A + 1] = (HIminus - v2) * 0.5f;
            buffer[B + 1] = buffer[A + 1] - HIminus;

            A += 2;
            B -= 2;
            br1++;
        }
        // Handle center bin (just need conjugate)
        buffer[A + 1] = -buffer[A + 1];

        // Handle DC bin separately - this ignores any Fs/2 component
        // buffer[1]=buffer[0]=buffer[0]/2;
        // Handle DC and Fs/2 bins specially
        // The DC bin is passed in as the real part of the DC complex value
        // The Fs/2 bin is passed in as the imaginary part of the DC complex value
        // (v1+v2) = buffer[0] == the DC component
        // (v1-v2) = buffer[1] == the Fs/2 component
        v1 = 0.5f * (buffer[0] + buffer[1]);
        v2 = 0.5f * (buffer[0] - buffer[1]);
        buffer[0] = v1;
        buffer[1] = v2;

        //  Butterfly:
        //     Ain-----Aout
        //         \ /
        //         / \
        //     Bin-----Bout

        endptr1 = h.Points * 2;

        while (ButterfliesPerGroup > 0)
        {
            A = 0;
            B = ButterfliesPerGroup * 2;
            int sptr = 0;

            while (A < endptr1)
            {
                sin = h.SinTable[sptr];
                ++sptr;
                cos = h.SinTable[sptr];
                ++sptr;
                endptr2 = B;
                while (A < endptr2)
                {
                    v1 = buffer[B] * cos - buffer[B + 1] * sin;
                    v2 = buffer[B] * sin + buffer[B + 1] * cos;
                    buffer[B] = (buffer[A] + v1) * 0.5f;
                    buffer[A] = buffer[B] - v1;
                    ++A;
                    ++B;
                    buffer[B] = (buffer[A] + v2) * 0.5f;
                    buffer[A] = buffer[B] - v2;
                    ++A;
                    ++B;
                }
                A = B;
                B += ButterfliesPerGroup * 2;
            }
            ButterfliesPerGroup >>= 1;
        }
    }

    /// <summary>
    /// Copy the buffer to real and imaginary out
    /// </summary>
    /// <param name="hFFT"></param>
    /// <param name="buffer"></param>
    /// <param name="RealOut"></param>
    /// <param name="ImagOut"></param>
    void ReorderToFreq(FFTParam hFFT, float[] buffer, float[] RealOut, float[] ImagOut)
    {
        // Copy the data into the real and imaginary outputs
        for (int i = 1; i < hFFT.Points; ++i)
        {
            RealOut[i] = buffer[hFFT.BitReversed[i]];
            ImagOut[i] = buffer[hFFT.BitReversed[i] + 1];
        }
        RealOut[0] = buffer[0]; // DC component
        ImagOut[0] = 0;
        RealOut[hFFT.Points] = buffer[1]; // Fs/2 component
        ImagOut[hFFT.Points] = 0;
    }

    /// <summary>
    /// Reorder buffer to time
    /// </summary>
    /// <param name="hFFT"></param>
    /// <param name="buffer"></param>
    /// <param name="TimeOut"></param>
    void ReorderToTime(FFTParam hFFT, float[] buffer, float[] TimeOut)
    {
        // Copy the data into the real outputs
        for (int i = 0; i < hFFT.Points; ++i)
        {
            TimeOut[i * 2] = buffer[hFFT.BitReversed[i]];
            TimeOut[i * 2 + 1] = buffer[hFFT.BitReversed[i] + 1];
        }
    }
}