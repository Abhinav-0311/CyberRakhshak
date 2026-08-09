using UnityEngine;

namespace CyberRakshak.PATCH
{
    [CreateAssetMenu(menuName = "CyberRakshak/PATCH/Dialogue Line", fileName = "PATCH_DialogueLine")]
    public sealed class PatchDialogueLine : ScriptableObject
    {
        [SerializeField] private string speaker = "PATCH";
        [SerializeField, TextArea(2, 4)] private string text;
        [SerializeField] private float displaySeconds = 4f;

        public string Speaker => speaker;
        public string Text => text;
        public float DisplaySeconds => displaySeconds;
    }
}

