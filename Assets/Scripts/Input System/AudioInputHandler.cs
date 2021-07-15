using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;

public class AudioInputHandler : MonoBehaviour
{
    //variables required for fussion and fission
    public bool stopInput = false;
    public bool startInput = false;
    public bool castling = false;
    public bool promotion = false;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();

    private DictationRecognizer dictationRecognizer;

    
    // Start is called before the first frame update
    void Start()
    {
        actions.Add("move", Move);
        actions.Add("castling", Castling);
        actions.Add("promotion", Promotion);
        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognisedSpeech;
        keywordRecognizer.Start();
        Debug.Log("AudioInputHandler init ");

    }

    // Update is called once per frame
    void Update()
    {
        if (stopInput)
        {
            keywordRecognizer.Stop();
            stopInput = false;
        }
        if (startInput)
        {
            keywordRecognizer.Start();
            startInput = false;
        }
    }

    public void StartRecording()
    {
        // Shutdown the PhraseRecognitionSystem. This controls the KeywordRecognizers.
        if (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            PhraseRecognitionSystem.Shutdown();
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
            Debug.Log("keywordRecognizer has been disposed.");
        }

        StartCoroutine(StartRecordingWhenPossible());
    }

    public void StartKeywordRecogniser()
    {
        if (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
            Debug.Log("dictationRecognizer has been disposed.");
        }
        StartCoroutine(StartKeywordRecogniserWhenPossible());
    }

    private IEnumerator StartKeywordRecogniserWhenPossible()
    {
        while (dictationRecognizer.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }

        keywordRecognizer = new KeywordRecognizer(actions.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += RecognisedSpeech;
        keywordRecognizer.Start();
        // Start recording from the microphone for 10 seconds.
        //Microphone.Start(deviceName, false, messageLength, samplingRate);
        Debug.Log("keywordRecognizer has started.");
    }

    private IEnumerator StartRecordingWhenPossible()
    {
        while (PhraseRecognitionSystem.Status == SpeechSystemStatus.Running)
        {
            yield return null;
        }

        dictationRecognizer = new DictationRecognizer();

        dictationRecognizer.DictationResult += (text, confidence) =>
        {
            Debug.LogFormat("Dictation result: {0}", text);
            StartKeywordRecogniser();
        };

        dictationRecognizer.DictationHypothesis += (text) =>
        {
            Debug.LogFormat("Dictation hypothesis: {0}", text);
        };

        dictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
        };

        dictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };
        dictationRecognizer.Start();
        // Start recording from the microphone for 10 seconds.
        //Microphone.Start(deviceName, false, messageLength, samplingRate);
        Debug.Log("dictationRecognizer has started.");
    }

    private void RecognisedSpeech( PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        if (promotion) 
            promotion = false;
        actions[speech.text].Invoke();
    }

    private void Move()
    {
        Debug.Log("Move keyword captured");
        StartRecording();
    }

    private void Castling()
    {
        Debug.Log("Castling");
    }

    private void Promotion()
    {
        promotion = true;
        Debug.Log("Promotion");
    }

    public bool promote()
    {
        return promotion;
    }
}
