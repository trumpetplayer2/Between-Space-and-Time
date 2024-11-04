using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace tp2
{
    public class Console : MonoBehaviour
    {
        public string[] entries = new string[5];
        public static Console instance;
        public TextMeshProUGUI text;
        void Start()
        {
            instance = this;
        }
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
        
        public void Log(string newLog)
        {
            string[] temp = new string[entries.Length];
            for(int i = 1; i < entries.Length; i++)
            {
                temp[i - 1] = entries[i];
            }
            temp[entries.Length - 1] = System.DateTime.Now.Hour + ":" + System.DateTime.Now.Minute + ": " + newLog;
            entries = temp;
            updateConsole();
        }

        public void updateConsole()
        {
            string temp = "";
            foreach(string entry in entries)
            {
                if (entry == null) continue;
                temp += entry;
                temp += "\n";
            }
            text.text = temp;
        }
    }
}
