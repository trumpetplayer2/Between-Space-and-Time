using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace tp2
{
    public class Dialogue : MonoBehaviour
    {
        public TextMeshProUGUI textbox;
        public string[] text;
        public Sprite[] image;
        int currentText = 0;
        float textTypeDelay = 0.05f;
        float updateTime = 0;
        int currentCharacter = 0;
        public bool complete = false;
        public Image pic;
        
        private void Start()
        {
            this.gameObject.SetActive(false);
        }
        private void Update()
        {
            if (complete) return;
            if (currentText >= text.Length)
            {
                complete = true;
                gameObject.SetActive(false);
                return;
            }
            if (Input.GetButtonDown("Submit"))
            {
                if(currentCharacter < text[currentText].Length)
                {
                    textbox.text = text[currentText];
                    currentCharacter = text[currentText].Length;
                }
                else
                {
                    currentText += 1;
                    currentCharacter = 0;
                    updateTime = 0;
                }
            }
            if (currentText >= text.Length) return;
            pic.sprite = image[currentText];
            updateTime += Time.deltaTime;
            if (!(updateTime >= textTypeDelay)) return;
            updateTime = 0;
            if (currentCharacter >= text[currentText].Length) return;
            currentCharacter += 1;
            //Play text character sound here
            textbox.text = text[currentText].Substring(0, currentCharacter);
        }
    }
}
