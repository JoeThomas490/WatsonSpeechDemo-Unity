using UnityEngine;
using System.Collections;

using IBM.Watson.DeveloperCloud.Logging;
using IBM.Watson.DeveloperCloud.Services.SpeechToText.v1;
using IBM.Watson.DeveloperCloud.Utilities;
using IBM.Watson.DeveloperCloud.DataTypes;

public class SpeechToTextExample : MonoBehaviour
{
    public GridMaker gridMaker;


    private string _username = "d51b83c2-ee63-45a5-85aa-a517c07f9585";
    private string _password = "3EqfyKlzDy6a";
    private string _url = "https://stream.watsonplatform.net/speech-to-text/api";

    private int m_RecordingRoutine = 0;


    private AudioClip m_Recording = null;
    private int m_RecordingBufferSize = 2;
    private int m_RecordingHZ = 22050;


    private SpeechToText m_SpeechToText;

    void Start()
    {
        //Create the credentials to pass to the speech to text service
        Credentials credentials = new Credentials(_username, _password, _url);

        //Instantiate a new SpeechToText object
        m_SpeechToText = new SpeechToText(credentials);

        //Needs research
        LogSystem.InstallDefaultReactors();
        Log.Debug("ExampleStreaming", "Start();");

        Active = true;

        StartRecording();
    }

    public bool Active
    {
        get { return m_SpeechToText.IsListening; }
        set
        {
            //If set to true and we're not listening
            if (value && !m_SpeechToText.IsListening)
            {
                m_SpeechToText.DetectSilence = true;
                m_SpeechToText.EnableWordConfidence = false;
                m_SpeechToText.EnableTimestamps = false;
                m_SpeechToText.SilenceThreshold = 0.03f;
                m_SpeechToText.MaxAlternatives = 5;
                //Continous recognition off to only record single words
                m_SpeechToText.EnableContinousRecognition = false;
                m_SpeechToText.EnableInterimResults = false;
                m_SpeechToText.OnError = OnError;
                m_SpeechToText.StartListening(OnRecognize);
            }
            else if (!value && m_SpeechToText.IsListening)
            {
                m_SpeechToText.StopListening();
            }
        }
    }

    private void StartRecording()
    {
        //If there isn't a recording co-routine running
        if (m_RecordingRoutine == 0)
        {
            UnityObjectUtil.StartDestroyQueue();
            m_RecordingRoutine = Runnable.Run(RecordingHandler());
        }
    }

    private void StopRecording()
    {
        if (m_RecordingRoutine != 0)
        {
            Microphone.End(null);
            Runnable.Stop(m_RecordingRoutine);
            m_RecordingRoutine = 0;
        }
    }

    private void OnError(string error)
    {
        Active = false;

        Log.Debug("ExampleStreaming", "Error! {0}", error);
    }

    private IEnumerator RecordingHandler()
    {
        m_Recording = Microphone.Start(null, true, m_RecordingBufferSize, m_RecordingHZ);
        yield return null;      // let m_RecordingRoutine get set..


        //If the recording doesn't initialise properly
        if (m_Recording == null)
        {
            //Stop recording
            StopRecording();
            //Break out of function
            yield break;
        }

        bool bFirstBlock = true;
        int midPoint = m_Recording.samples / 2;
        float[] samples = null;

        //While our recording routine is still running and the recording isn't null
        while (m_RecordingRoutine != 0 && m_Recording != null)
        {
            //Get the position to write to
            int writePos = Microphone.GetPosition(null);
            //If we are going to overload the samples array or the mic isn't recording anymore
            if (writePos > m_Recording.samples || !Microphone.IsRecording(null))
            {
                Log.Error("MicrophoneWidget", "Microphone disconnected.");

                //Stop recording
                StopRecording();
                yield break;
            }

            //Recording is done in two halves for some reason
            if ((bFirstBlock && writePos >= midPoint)
                || (!bFirstBlock && writePos < midPoint))
            {
                // front block is recorded, make a RecordClip and pass it onto our callback.
                samples = new float[midPoint];
                m_Recording.GetData(samples, bFirstBlock ? 0 : midPoint);

                AudioData record = new AudioData();
                record.MaxLevel = Mathf.Max(samples);
                record.Clip = AudioClip.Create("Recording", midPoint, m_Recording.channels, m_RecordingHZ, false);
                record.Clip.SetData(samples, 0);

                m_SpeechToText.OnListen(record);

                bFirstBlock = !bFirstBlock;
            }
            else
            {
                // calculate the number of samples remaining until we ready for a block of audio, 
                // and wait that amount of time it will take to record.
                int remaining = bFirstBlock ? (midPoint - writePos) : (m_Recording.samples - writePos);
                float timeRemaining = (float)remaining / (float)m_RecordingHZ;

                yield return new WaitForSeconds(timeRemaining);
            }

        }

        yield break;
    }


    private void OnRecognize(SpeechRecognitionEvent result)
    {
        if (result != null && result.results.Length > 0)
        {
            foreach (var res in result.results)
            {
                foreach (var alt in res.alternatives)
                {
                    string text = alt.transcript;
                    Log.Debug("ExampleStreaming", string.Format("{0} ({1}, {2:0.00})\n", text, res.final ? "Final" : "Interim", alt.confidence));

                    //we want to disect text here
                    TestString(text);

                }
            }
        }
    }

    private void TestString(string testString)
    {
        string[] cutUpText = testString.Split(' ');


        int num = SearchForNumber(cutUpText);
        char c = SearchForLetter(cutUpText);

        string tag = c + num.ToString();

        GameObject cell = gridMaker.GetGridCellByTag(tag);
        if (cell != null)
        {
            cell.GetComponent<SpriteRenderer>().color = Color.green;
            Debug.Log("STRING : " + testString + " .. TRIGGERED : " + tag);
        }
    }

    int SearchForNumber(string[] strings)
    {
        foreach (string s in strings)
        {
            switch (s)
            {
                case "one":
                    return 1;
                case "two":
                case "to":
                case "too":
                    return 2;
                case "three":
                    return 3;
                case "four":
                case "for":
                    return 4;
                case "five":
                    return 5;
                case "six":
                    return 6;
                case "seven":
                    return 7;
                case "eight":
                case "ate":
                    return 8;
                case "nine":
                    return 9;
                case "ten":
                    return 10;
            }
        }

        return -1;
    }

    char SearchForLetter(string[] strings)
    {
        foreach (string s in strings)
        {
            switch(s)
            {
                case "a":
                    return 'A';
                case "be":
                    return 'B';
                case "see":
                    return 'C';
                case "he":
                    return 'E';
                case "eye":
                    return 'I';
                case "oh":
                    return 'O';
                case "cue":
                case "queue":
                    return 'Q';
                case "are":
                    return 'R';
                case "tea":
                    return 'T';
                case "you":
                    return 'U';
                case "why":
                    return 'Y';
            }
            if (s.Length == 2)
            {
                if (s[1] == '.')
                    return s[0];
            }
        }

        return ' ';
    }
}
