using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Morpho
{
    /// <summary>
    /// Part Of Speech
    /// </summary>
    [System.Serializable]
    public class POS
    {
        public string id;
        public string text;
    }


    /// <summary>
    /// Sense
    /// </summary>
    [System.Serializable]
    public class Sense
    {
        public string id;
        public string numtags;
        public string baseform;
        public POS partofspeech;
    }

    /// <summary>
    /// Sense-List
    /// </summary>
    [System.Serializable]
    public class SenseList
    {
        public string disambiguated;
        public Sense sense;
    }

    /// <summary>
    /// Lexeme
    /// </summary>
    [System.Serializable]
    public class Lexeme
    {
        public string id;
        public string surfaceform;
        public SenseList senselist;
    }

    /// <summary>
    /// Lexeme-List
    /// </summary>
    [System.Serializable]
    public class LexemeList
    {
        public Lexeme[] lexeme;
    }
}

public class SpatialMorphologicalAnalyzer : MonoBehaviour
{
    static private SpatialMorphologicalAnalyzer _instance = null;

    public delegate void Listener(Morpho.LexemeList lexemes);
    private Listener _callback = null;

    private string _language = "English";

    //singleton instance
    private void Awake()
    {
        if(_instance == null) _instance = this;
    }

    // Use this for initialization
    void Start()
    {
         //   SpatialMorphologicalAnalyzer.RequestMorpho("hello, nice to meet you", (Morpho.LexemeList list) =>
         //{
         //    foreach (Morpho.Lexeme lexeme in list.lexeme)
         //    {
         //        Debug.Log(lexeme.senselist.sense.baseform + " : " + lexeme.senselist.sense.partofspeech.text);
         //    }
         //});
         //   //StartCoroutine(_requestToServer("I think Disney will require McDonald's in 2018."));
    }

    // Update is called once per frame
    void Update()
    {
    }

    Morpho.LexemeList _convertJsonToData(string jtxt)
    {
        jtxt = jtxt.Replace("@", "");
        jtxt = jtxt.Replace("-", "");
        jtxt = jtxt.Replace("#", "");

        int idx = jtxt.IndexOf("[{");
        jtxt = jtxt.Remove(0, idx);

        idx = jtxt.IndexOf("}]");
        jtxt = jtxt.Remove(idx + 2);// + "}";
        jtxt = @"{ ""lexeme"" : " + jtxt + @"}";

        return JsonUtility.FromJson<Morpho.LexemeList>(jtxt);
    }

    IEnumerator _requestToServer(string sentence, string language)
    {

        string url = "https://services.open.xerox.com/bus/op/fst-nlp-tools/PartOfSpeechTagging";//?inputtext=" + sentence + "&language=" + languge;
        Debug.Log(url);

        WWWForm form = new WWWForm();
        form.AddField("inputtext", sentence, System.Text.Encoding.UTF8);
        form.AddField("language", language, System.Text.Encoding.UTF8);

        WWW www = new WWW(url, form);

        yield return www;

        if (www.error != null)
        {
            Debug.LogError(www.error);
        }
        else
        {
            _callback(_convertJsonToData(www.text));
        }

    }

    void setLanguage(string language)
    {
        _language = language;
    }

    void setListener(Listener cb)
    {
        _callback = cb;
    }

    void requestToServer(string sentence)
    {
        Debug.Log(sentence + " // " + _language + " // " + this);
        StartCoroutine(_requestToServer(sentence, _language));
    }


    /* Interfaces */

    /// <summary>
    /// Setup Language for Morphological Analyzer
    /// </summary>
    /// <param name="language"> language </param>
    static public void SetLanguage(string language)
    {
        _instance.setLanguage(language);
    }

    /// <summary>
    /// Request Morphological Analyzing
    /// </summary>
    /// <param name="sentence"> target sentence </param>
    /// <param name="cb"> callback function </param>
    static public void RequestMorpho(string sentence, Listener cb)
    {
        _instance.setListener(cb);
        _instance.requestToServer(sentence);
    }

}
