using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class MusicManager : MonoBehaviour
    {
        float volume = .1f;
        public AudioSource current;
        public AudioSource blend;
        public AudioClip pastMusic;
        public AudioClip presentMusic;
        public AudioClip futureMusic;
        PlayerType audioType;
        public float transitionTime = 3f;
        float timer = 0;
        bool transitioning = false;

        AudioClip getClip(PlayerType type)
        {
            switch (type)
            {
                case PlayerType.Atlas:
                    return pastMusic;
                case PlayerType.Chroma:
                    return futureMusic;
                default:
                    return presentMusic;
            }
        }
        private void Start()
        {
            //volume = Settings.instance.volume
            current.volume = volume;
        }
        public void toggleMusic()
        {
            toggleMusic(PlayerTypeExtensions.getLocalPlayerType());
        }

        public void toggleMusic(PlayerType type)
        {
            if (audioType == type) type = PlayerType.None;
            blend.clip = getClip(type);
            blend.volume = 0;
            blend.Play();
            transitioning = true;
            timer = transitionTime;
            audioType = type;
        }

        private void Update()
        {
            if(timer > 0)
            {
                timer = Mathf.Max(0, timer - Time.deltaTime);
            }
            if (!transitioning) return;
            if(current.volume > 0)
            {
                current.volume = (timer / transitionTime) * volume;
                blend.volume = (1 - (timer / transitionTime)) * volume;
            }
            else
            {
                AudioSource temp = blend;
                blend = current;
                current = temp;
                transitioning = false;
            }
        }

    }
}
