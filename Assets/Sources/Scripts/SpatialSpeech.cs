﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;


#if UNITY_WEBGL && !UNITY_EDITOR
using UnityWebGLSpeechDetection;
#else
using UnityEngine.Windows.Speech;
#endif

public class SpatialSpeech : MonoBehaviour {
    
    // Reference to the text that displays detected words
    public Text _mTextDictation = null;
    
    // Reference to the text that displays detected words
    public Text[] _mTextKeyword = null;
    public RawImage[] _mImageKeyword = null;

    // Dropdown selector for languages
    public Dropdown _mDropDownLanguages = null;
    
    // Dropdown selector for dialects
    public Dropdown _mDropDownDialects = null;
    
    // Reference to the supported languages and dialects
    
    // List of detected words
    private List<string> _mWords = new List<string>();
    private List<string> _mKeywords = new List<string>();
    private Dictionary<string, Texture> _mKeywordsTex = new Dictionary<string, Texture>();

    // String builder to format the dictation text
    private StringBuilder _mStringBuilder = new StringBuilder();



#if UNITY_WEBGL && !UNITY_EDITOR
    private LanguageResult _mLanguageResult = null;

    private ISpeechDetectionPlugin _mSpeechDetectionPlugin = null;

    // Use this for initialization
    IEnumerator Start () {
        //get Instance of WebGLSpeech
        _mSpeechDetectionPlugin = WebGLSpeechDetectionPlugin.GetInstance();

        //null check
        if(_mSpeechDetectionPlugin == null)
        {
            Debug.LogError("[SpatialSpeech] _mSpeechDetectionPlugin failed.");
            yield break;
        }

        // wait for plugin to become available
        while (!_mSpeechDetectionPlugin.IsAvailable())
        {
            yield return null;
        }

        // subscribe to events
        _mSpeechDetectionPlugin.AddListenerOnDetectionResult(HandleDetectionResult);

        // Get languages from plugin,
        _mSpeechDetectionPlugin.GetLanguages(HandleGetLanguages);
    }

   

    void HandleGetLanguages(LanguageResult languageResult)
    {
        //Debug.Log(languageResult.languages.);
        _mLanguageResult = languageResult;

        // prepare the language drop down items
        SpeechDetectionUtils.PopulateLanguagesDropdown(_mDropDownLanguages, _mLanguageResult);

        // subscribe to language change events
        if (_mDropDownLanguages)
        {
            _mDropDownLanguages.onValueChanged.AddListener(delegate
            {
                SpeechDetectionUtils.HandleLanguageChanged(_mDropDownLanguages,
                    _mDropDownDialects,
                    _mLanguageResult,
                    _mSpeechDetectionPlugin);
            });
        }

        // subscribe to dialect change events
        if (_mDropDownDialects)
        {
            _mDropDownDialects.onValueChanged.AddListener(delegate
            {
                SpeechDetectionUtils.HandleDialectChanged(_mDropDownDialects,
                    _mLanguageResult,
                    _mSpeechDetectionPlugin);
            });
        }

        // Disabled until a language is selected
        SpeechDetectionUtils.DisableDialects(_mDropDownDialects);

        // set the default language
        SpeechDetectionUtils.RestoreLanguage(_mDropDownLanguages);

        // set the default dialect
        SpeechDetectionUtils.RestoreDialect(_mDropDownDialects);
    }

    bool HandleDetectionResult(DetectionResult detectionResult)
    {
        if (null == detectionResult)
        {
            return false;
        }

        SpeechRecognitionResult[] results = detectionResult.results;
        if (null == results)
        {
            return false;
        }

        foreach (SpeechRecognitionResult result in results)
        {
            SpeechRecognitionAlternative[] alternatives = result.alternatives;
            if (null == alternatives)
            {
                continue;
            }
            foreach (SpeechRecognitionAlternative alternative in alternatives)
            {
                if (string.IsNullOrEmpty(alternative.transcript))
                {
                    continue;
                }
                if (result.isFinal)
                {
                    _mWords.Add(string.Format("\"{0}\"", alternative.transcript));

                    //_mTextKeyword.text = "Processing...";
                    SpatialMorphologicalAnalyzer.RequestMorpho(alternative.transcript, HandleGetLexeme);
                }
                else
                {
                    string finalSentence = string.Format("\"{0}\"", alternative.transcript);
                    _mWords.Add(finalSentence);
                }
            }
        }
        reorderingWords();
        return false;
    }
#else
    private DictationRecognizer m_DictationRecognizer;

    void Start()
    {
        m_DictationRecognizer = new DictationRecognizer();

        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {

            Debug.Log("FINAL : " + text);
            _mWords.Add(string.Format("\"{0}\"", text));

            SpatialMorphologicalAnalyzer.RequestMorpho(text, HandleGetLexeme);
            reorderingWords();
        };


        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            string finalSentence = string.Format("\"{0}\"", text);
            _mWords.Add(finalSentence);
            reorderingWords();
        };

        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            if (completionCause != DictationCompletionCause.Complete)
            {
                Debug.LogErrorFormat("Dictation completed unsuccessfully: {0}.", completionCause);
                m_DictationRecognizer.Start();
            }
        };

        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            Debug.LogErrorFormat("Dictation error: {0}; HResult = {1}.", error, hresult);
        };

        m_DictationRecognizer.Start();
    }

#endif

    // Update is called once per frame
    void Update()
    {
        string sentence = "";
        if (Input.anyKeyDown)
        {
            switch (Input.inputString)
            {
                case "1": sentence = "I think Disney will require McDonald's in 2018."; break;
                case "2": sentence = "Create an object from its JSON representation."; break;
                case "3": sentence = "therefore the type you are creating must be supported by the serializer."; break;
                case "4": sentence = "how to reply when someone mentions the president at Thanksgiving."; break;
                case "5": sentence = "Roman empire has been thriving for long time."; break;
                case "6": sentence = "Disney"; break;
                default: return;
            }
            //_mTextKeyword.text = "Processing...";
            _mTextDictation.text = sentence;
            SpatialMorphologicalAnalyzer.RequestMorpho(sentence, HandleGetLexeme);
        }

        for (int i = 0; i < _mKeywords.Count; i++)
        {
            string word = _mKeywords.ToArray()[i];
            Texture tex = _mKeywordsTex.ContainsKey(word) ? _mKeywordsTex[word] : null;

            _mTextKeyword[i].text = word;
            _mImageKeyword[i].texture = tex;
            _mImageKeyword[i].rectTransform.sizeDelta = _mKeywordsTex.ContainsKey(word) ? new Vector2(65.0f * tex.width / tex.height, 65.0f) : Vector2.one * 65.0f;
        }
    }

    void HandleGetLexeme(Morpho.LexemeList list)
    {
        //_mTextKeyword.text = "";
        foreach (Morpho.Lexeme lexeme in list.lexeme)
        {
            string word = lexeme.senselist.sense.baseform;
            string type = lexeme.senselist.sense.partofspeech.text;

            if (type == "+PROP" || type == "+ADJ")
            {
                _mKeywords.Add(word);// + "\n";
                SpatialImageSearch.RequestSearch(word, (Texture tex) =>
                {
                    Debug.Log(tex);
                    _mKeywordsTex.Add(word, tex);
                });
            }
        }

        while (_mKeywords.Count > 5)
        {
            string word = _mKeywords.ToArray()[0];
            _mKeywords.RemoveAt(0);
            _mKeywordsTex.Remove(word);
        }

    }

    void reorderingWords()
    {
        while (_mWords.Count > 3)
        {
            _mWords.RemoveAt(0);
        }

        if (_mTextDictation)
        {
            if (_mStringBuilder.Length > 0)
            {
                _mStringBuilder.Remove(0, _mStringBuilder.Length);
            }
            foreach (string text in _mWords)
            {
                _mStringBuilder.AppendLine(text);
            }
            _mTextDictation.text = _mStringBuilder.ToString();
        }
    }
}
