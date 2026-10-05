using UnityEngine;

namespace CyberRakshak.PATCH
{
    [CreateAssetMenu(menuName = "CyberRakshak/PATCH/Dialogue Sequence", fileName = "DialogueSequence")]
    public sealed class PatchDialogueSequence : ScriptableObject
    {
        [SerializeField] private bool useOfficeBackdrop;
        [SerializeField] private PatchDialoguePresenter.DialogueMessage[] messages;

        public bool UseOfficeBackdrop => useOfficeBackdrop;
        public PatchDialoguePresenter.DialogueMessage[] Messages => messages;
    }
}
