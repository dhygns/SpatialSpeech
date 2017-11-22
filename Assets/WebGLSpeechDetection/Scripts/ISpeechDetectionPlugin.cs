using System;

namespace UnityWebGLSpeechDetection
{
    public interface ISpeechDetectionPlugin
    {
        void Abort();

        void AddListenerOnDetectionResult(BaseSpeechDetectionPlugin.DelegateHandleDetectionResult listener);

        void GetLanguages(Action<LanguageResult> callback);

        bool IsAvailable();

        void ManagementCloseBrowserTab();

        void ManagementCloseProxy();

        void ManagementLaunchProxy();

        void ManagementOpenBrowserTab();

        void ManagementSetProxyPort(int port);

        void RemoveListenerOnDetectionResult(BaseSpeechDetectionPlugin.DelegateHandleDetectionResult listener);

        void SetLanguage(string langauge);

        void Invoke(LanguageChangedResult languageChangedResult);
    }
}
