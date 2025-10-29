using System.Threading.Tasks;

namespace CoreShared
{
    /// <summary>
    /// Minimal interface to allow Core to control a splash screen without depending on UI assembly.
    /// Implemented by UI's SplashScreenController.
    /// </summary>
    public interface ISplashScreenController
    {
        void SetTitle(string title);
        Task PlaySequenceAsync(float holdSeconds, float fadeSeconds);
    }
}
