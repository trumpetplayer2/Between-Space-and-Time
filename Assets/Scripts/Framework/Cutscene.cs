using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace tp2
{
    public class Cutscene : MonoBehaviour
    {
        public Transform[] cameraPositions;
        public float[] timing;
        public Dialogue[] dialogueBox;
        public int redirectScene = -1;
        [SerializeField] private UnityEvent CutsceneEnded;
        public void cutsceneEnd()
        {
            CutsceneEnded.Invoke();
        }
    }
}
