/// as seen on 
/// http://answers.unity3d.com/questions/157940/getoutputdata-and-getspectrumdata-they-represent-t.html#

var qSamples: int = 1024;  // array size
var refValue: float = 0.1; // RMS value for 0 dB
var threshold = 0.02;      // minimum amplitude to extract pitch
var rmsValue: float;   // sound level - RMS
var dbValue: float;    // sound level - dB
var pitchValue: float; // sound pitch - Hz
 
private var samples: float[]; // audio samples
private var spectrum: float[]; // audio spectrum
private var fSample: float;
 
function Start () {
    samples = new float[qSamples];
    spectrum = new float[qSamples];
    fSample = AudioSettings.outputSampleRate;
}
 
function AnalyzeSound(){
    audio.GetOutputData(samples, 0); // fill array with samples
    var i: int;
    var sum: float = 0;
    for (i=0; i < qSamples; i++){
        sum += samples[i]*samples[i]; // sum squared samples
    }
    rmsValue = Mathf.Sqrt(sum/qSamples); // rms = square root of average
    dbValue = 20*Mathf.Log10(rmsValue/refValue); // calculate dB
    if (dbValue < -160) dbValue = -160; // clamp it to -160dB min
    // get sound spectrum
    audio.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);
    var maxV: float = 0;
    var maxN: int = 0;
    for (i=0; i < qSamples; i++){ // find max 
        if (spectrum[i] > maxV && spectrum[i] > threshold){
            maxV = spectrum[i];
            maxN = i; // maxN is the index of max
        }
    }
    var freqN: float = maxN; // pass the index to a float variable
    if (maxN > 0 && maxN < qSamples-1){ // interpolate index using neighbours
        var dL = spectrum[maxN-1]/spectrum[maxN];
        var dR = spectrum[maxN+1]/spectrum[maxN];
        freqN += 0.5*(dR*dR - dL*dL);
    }
    pitchValue = freqN*(fSample/2)/qSamples; // convert index to frequency
}
 
var display: GUIText; // drag a GUIText here to show results
 
function Update () {
    if (Input.GetKeyDown("p")){
        audio.Play();
    }
    AnalyzeSound();
    if (display){ 
        display.text = "RMS: "+rmsValue.ToString("F2")+
        " ("+dbValue.ToString("F1")+" dB)\n"+
        "Pitch: "+pitchValue.ToString("F0")+" Hz";
    }
}