using System;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif
using UnityEngine;

namespace UnityWebGLSpeechDetection
{
    public class WebGLSpeechDetectionPlugin : BaseSpeechDetectionPlugin, ISpeechDetectionPlugin
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static WebGLSpeechDetectionPlugin _sInstance = null;

        /// <summary>
        /// Get singleton instance
        /// </summary>
        public static WebGLSpeechDetectionPlugin GetInstance()
        {
            return _sInstance;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        private class OnlyWebGL
        {
            [DllImport("__Internal")]
            public static extern bool IsAvailable();

            [DllImport("__Internal")]
            public static extern void Init();

            [DllImport("__Internal")]
            public static extern void Abort();

            [DllImport("__Internal")]
            public static extern string GetLanguages();

            [DllImport("__Internal")]
            public static extern int GetNumberOfResults();

            [DllImport("__Internal")]
            public static extern string GetResult();

            [DllImport("__Internal")]
            public static extern void SetLanguage(string dialect);

            [DllImport("__Internal")]
            public static extern void Start();

            [DllImport("__Internal")]
            public static extern void Stop();
        }
#endif

        public bool IsAvailable()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            return OnlyWebGL.IsAvailable();
#else
            return false;
#endif
        }

        public void Abort()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.Abort();
#endif
        }

        public void StartRecognition()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.Start();
#endif
        }

        public void StopRecognition()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.Stop();
#endif
        }

        public void GetLanguages(Action<LanguageResult> callback)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            string jsonData = OnlyWebGL.GetLanguages();
            if (string.IsNullOrEmpty(jsonData))
            {
                Debug.LogError("GetLanguages: Languages are empty!");
                callback.Invoke(null);
                return;
            }
            //Debug.Log("GetLanguages=" + jsonData);
            LanguageResult languageResult = JsonUtility.FromJson<LanguageResult>(jsonData);
            callback.Invoke(languageResult);
            return;
#else
            callback.Invoke(null);
#endif
        }

        public void SetLanguage(string dialect)
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            OnlyWebGL.SetLanguage(dialect);
#endif
        }

        /// <summary>
        /// Set the singleton instance
        /// </summary>
        private void Awake()
        {
            _sInstance = this;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        void Start()
        {

            if (OnlyWebGL.IsAvailable())
            {
                OnlyWebGL.Init();
            }
        }

        void FixedUpdate()
        {
            if (OnlyWebGL.GetNumberOfResults() > 0)
            {
                string jsonData = OnlyWebGL.GetResult();
                if (string.IsNullOrEmpty(jsonData))
                {
                    return;
                }
                DetectionResult detectionResult = JsonUtility.FromJson<DetectionResult>(jsonData);
                if (null != detectionResult)
                {
                    Invoke(detectionResult);
                }
            }
        }
#endif

        public void ManagementCloseBrowserTab()
        {
            //not used
        }

        public void ManagementCloseProxy()
        {
            //not used
        }

        public void ManagementLaunchProxy()
        {
            //not used
        }

        public void ManagementOpenBrowserTab()
        {
            //not used
        }

        public void ManagementSetProxyPort(int port)
        {
            //not used
        }
    }
}
