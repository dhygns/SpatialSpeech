﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

using UnityWebGLSpeechDetection;

public class SpatialSpeech : MonoBehaviour {
    
    // Reference to the text that displays detected words
    public Text _mTextDictation = null;
    
    // Reference to the text that displays detected words
    public Text _mTextKeyword = null;
    
    // Dropdown selector for languages
    public Dropdown _mDropDownLanguages = null;
    
    // Dropdown selector for dialects
    public Dropdown _mDropDownDialects = null;
    
    // Reference to the supported languages and dialects
    private LanguageResult _mLanguageResult = null;
    
    // List of detected words
    private List<string> _mWords = new List<string>();
    
    // String builder to format the dictation text
    private StringBuilder _mStringBuilder = new StringBuilder();


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

    // Update is called once per frame
    void Update () {
		
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

                    _mTextKeyword.text = "";
                    SpatialMorphologicalAnalyzer.RequestMorpho(alternative.transcript, (Morpho.LexemeList list) =>
                    {
                        foreach (Morpho.Lexeme lexeme in list.lexeme)
                        {
                            _mTextKeyword.text += lexeme.senselist.sense.baseform + " : " + lexeme.senselist.sense.partofspeech.text + "\n";
                        }
                    });
                }
                else
                {
                    string finalSentence = string.Format("\"{0}\"", alternative.transcript);
                    _mWords.Add(finalSentence);
                    _mTextKeyword.text = DetectKeyword(finalSentence);
                }
            }
        }
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
        return false;
    }

    string DetectKeyword(string sentence)
    {
        string[] words = sentence.Split(' ');
        
        return words[Random.Range(0, words.Length)];
    }
}
