using UnityEngine;
using UnityEngine.UI;

namespace CyberRakshak.Runtime
{
    public sealed class PauseBackdropController : MonoBehaviour
    {
        [SerializeField] private RawImage blurredFrame;

        private Texture2D capturedFrame;

        public void Capture()
        {
            var camera = Camera.main;
            if (camera == null)
                return;

            if (capturedFrame != null)
                Destroy(capturedFrame);

            var width = Screen.width;
            var height = Screen.height;
            var renderTarget = RenderTexture.GetTemporary(width, height, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;

            camera.targetTexture = renderTarget;
            camera.Render();
            RenderTexture.active = renderTarget;

            capturedFrame = new Texture2D(width, height, TextureFormat.RGBA32, false);
            capturedFrame.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            capturedFrame.Apply();

            RenderTexture.active = previousActive;
            camera.targetTexture = previousTarget;
            RenderTexture.ReleaseTemporary(renderTarget);

            blurredFrame.texture = capturedFrame;
            blurredFrame.gameObject.SetActive(true);
        }

        public void Hide()
        {
            blurredFrame.texture = null;
            blurredFrame.gameObject.SetActive(false);

            if (capturedFrame != null)
            {
                Destroy(capturedFrame);
                capturedFrame = null;
            }
        }
    }
}

