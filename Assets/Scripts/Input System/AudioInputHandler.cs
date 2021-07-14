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


    // Start is called before the first frame update
    void Start()
    {
        actions.Add("queen", SelectQueen);
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

    private void RecognisedSpeech( PhraseRecognizedEventArgs speech)
    {
        Debug.Log(speech.text);
        if (promotion) 
            promotion = false;
        actions[speech.text].Invoke();
    }

    private void SelectQueen()
    {
        Debug.Log("Queen selected.");
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
