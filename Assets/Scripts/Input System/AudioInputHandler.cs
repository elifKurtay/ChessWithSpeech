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

    public ChessGameController chessGameController;

    private DictationRecognizer dictationRecognizer;
    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, Action> actions = new Dictionary<string, Action>();
    private List<string> pieces = new List<string>() { "king", "queen", "bishop", "knight", "rook", "pawn" };
    private List<string> letters = new List<string>() { "a", "b", "c", "d", "e", "f", "g", "h" };

    
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
            if(text.Contains(" to ") && text.Split(' ').Length > 2)
            {

                string[] words = text.ToLower().Split(' ');
                int pieceIndex = 0;
                if (words[0] == "move")
                    pieceIndex = 1;
                //a 3 instead of a3
                if (words.Length == pieceIndex + 4 && words[words.Length - 1].All(char.IsDigit)
                    && words[words.Length - 2].Length == 1 && letters.Contains(words[words.Length - 2]) )
                {
                    string coords = words[2 + pieceIndex] + words[3 + pieceIndex];
                    Debug.LogFormat("Piece {0} will be moved to {1}", words[pieceIndex], coords);
                    chessGameController.FindAndMovePiece(words[pieceIndex], coords);
                    StartKeywordRecogniser();
                }
                else if (pieces.Contains(words[pieceIndex]) && words[pieceIndex + 2].Length == 2
                    && words[pieceIndex + 2][1] < 58 && letters.Contains(words[pieceIndex + 2][0] + "") )
                {
                    Debug.LogFormat("Piece {0} will be moved to {1}", words[pieceIndex], words[2 + pieceIndex]);
                    chessGameController.FindAndMovePiece(words[pieceIndex], words[2 + pieceIndex]);
                    StartKeywordRecogniser();
                }
                else
                    Debug.Log("BAD VOICE COMMAND. TRY AGAIN.");
            }
            
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
