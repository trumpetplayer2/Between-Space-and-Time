using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class SettingsMenu : MonoBehaviour
    {
        public void UpdateVolume(float volume)
        {
            AudioHandler.masterVolume = volume;
        }
    }
}
