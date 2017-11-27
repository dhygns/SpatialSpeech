using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpatialImageSearch : MonoBehaviour {

    static private SpatialImageSearch _instance = null;

    public delegate void Listener(Texture texture);

    Dictionary<string, string> headers = new Dictionary<string, string>();


    private void Awake()
    {
        if(_instance == null) _instance = this;

#if UNITY_WEBGL
        headers.Add("Access-Control-Allow-Credentials", "true");
        headers.Add("Access-Control-Allow-Headers", "Accept, X-Access-Token, X-Application-Name, X-Request-Sent-Time");
        headers.Add("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
        headers.Add("Access-Control-Allow-Origin", "*");
#endif
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    IEnumerator _requestSearch(string word, Listener cb = null)
    {
        string url = "http://images.google.com/images?q=" + word + "&hl=en&imgsz=small";
        WWW www = new WWW(url);

        yield return www;

        if (www.error != null)
        {
            Debug.LogWarning(www.error);
        }
        else
        {
            Debug.Log(www.text);
            string text = www.text;
            int left = www.text.IndexOf("Image result for");
            int right = left;
            for (; text[left] != '<'; left--) ;
            for (; text[right] != '<'; right++) ;

            string ret = text.Substring(left, right - left);

            int idx = ret.IndexOf("src=\"") + 5;
            string src = ret.Remove(0, idx);

            idx = src.IndexOf("\"");
            src = src.Substring(0, idx);

            WWW img = new WWW(src, null, headers);

            yield return img;

            if(cb != null) cb(img.texture);
        }
    }

    private void requestSearch(string word, Listener cb)
    {
        StartCoroutine(_requestSearch(word, cb));
    }

    static public void RequestSearch(string word, Listener cb)
    {
        _instance.requestSearch(word, cb);
    }
}
