namespace UnityWebGLSpeechDetection
{
    public interface IWWW
    {
        bool IsDone();

        string GetError();

        string GetText();

        void Dispose();
    }
}
