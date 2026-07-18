using UnityEngine;
using UnityEngine.SceneManagement;

namespace CyberRakshak.Runtime
{
    public sealed class SplashController : MonoBehaviour
    {
        [SerializeField] private float minimumDisplayTime = 1.2f;
        private float openedAt;

        private void Awake()
        {
            openedAt = Time.unscaledTime;
        }

        private void Update()
        {
            if (Time.unscaledTime - openedAt >= minimumDisplayTime && Input.anyKeyDown)
                SceneManager.LoadScene("MainMenu");
        }
    }
}