using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;

using UnityWebGLSpeechDetection;

public class SpatialSpeech : MonoBehaviour {

    /// <summary>
    /// Reference to the text that displays detected words
    /// </summary>
    public Text _mTextDictation = null;

    /// <summary>
    /// Dropdown selector for languages
    /// </summary>
    public Dropdown _mDropDownLanguages = null;

    /// <summary>
    /// Dropdown selector for dialects
    /// </summary>
    public Dropdown _mDropDownDialects = null;

    /// <summary>
    /// Reference to the supported languages and dialects
    /// </summary>
    private LanguageResult _mLanguageResult = null;


    /// <summary>
    /// List of detected words
    /// </summary>
    private List<string> _mWords = new List<string>();

    /// <summary>
    /// String builder to format the dictation text
    /// </summary>
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
                    _mWords.Add(string.Format("[FINAL] \"{0}\" Confidence={1}",
                        alternative.transcript,
                        alternative.confidence));
                }
                else
                {
                    _mWords.Add(string.Format("\"{0}\" Confidence={1}",
                        alternative.transcript,
                        alternative.confidence));
                }
            }
        }
        while (_mWords.Count > 15)
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
}
