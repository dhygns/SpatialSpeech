using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class WWWPlayMode : IWWW
    {
        private WWW _mWWW = null;

        public WWWPlayMode(string url)
        {
            //Debug.LogFormat("WWWPlayMode: url={0}", url);
            _mWWW = new WWW(url);
        }

        public bool IsDone()
        {
            return _mWWW.isDone;
        }

        public string GetError()
        {
            return _mWWW.error;
        }

        public string GetText()
        {
            return _mWWW.text;
        }

        public void Dispose()
        {
            _mWWW.Dispose();
        }
    }
}
