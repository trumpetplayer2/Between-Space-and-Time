using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    [System.Serializable]
    public class AudioSettings
    {
        public float sfxVolume = 1f;
        public float sfxPitch = 1f;
        public float startTime = 0f;
        public float timeout = -1f;
        public AudioClip[] sfx = new AudioClip[2];
    }

    public class SfxHandler : MonoBehaviour
    {
        public AudioSettings audioSettings = new AudioSettings();

        public void playClip(int i)
        {
            if (getSfx(i) == null) return;
            playClip(getSfx(i));
        }

        public void playClip(bool b)
        {
            if (b)
            {
                playClip(0);
            }
            else
            {
                playClip(1);
            }
        }

        public void playClip(AudioClip clip)
        {
            AudioHandler.instance.queueClip(clip, audioSettings.sfxVolume, audioSettings.sfxPitch, audioSettings.timeout, audioSettings.startTime);
        }

        public AudioClip getSfx(bool t)
        {
            if (t)
            {
                return audioSettings.sfx[0];
            }
            else
            {
                return audioSettings.sfx[1];
            }
        }
        
        public AudioClip getSfx(int i)
        {
            if(audioSettings.sfx.Length > i)
            {
                return audioSettings.sfx[i];
            }
            else
            {
                return null;
            }
        }
    }
}
