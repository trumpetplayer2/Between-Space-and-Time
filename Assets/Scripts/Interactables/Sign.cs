using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Sign : MonoBehaviour
{
    public float fadeTime = .5f;
    float alpha = 0;
    public float updateTimes = 100;
    public Object[] items;
    bool showing = false;
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag.ToLower().Equals("player"))) return;
        //Show Information
        showing = true;
        show();
    }

    public void OnTriggerExit2D(Collider2D collision)
    {
        if (!(collision.gameObject.tag.ToLower().Equals("player"))) return;
        //Hide Information
        showing = false;
        hide();
    }

    public void show()
    {
        //Prevent recurring if person left before fully shown
        if (!showing) return;
        //Primative loop
        if (alpha > 1)
        {
            alpha = 1;
            return;
        }
        //Loop through each obj and change alpha based on type
        foreach(Object obj in items)
        {
            //IDK if there is a way to do a switch for types in C#
            //I know there is in other languages but this method is messier but works
            if(obj.GetType() == typeof(SpriteRenderer))
            {
                SpriteRenderer tempSprite = (SpriteRenderer)obj;
                Color c = tempSprite.color;
                c.a = alpha;
                tempSprite.color = c;
                continue;
            }
            if(obj.GetType() == typeof(TextMeshPro))
            {
                TextMeshPro tempText = (TextMeshPro)obj;
                Color c = tempText.color;
                c.a = alpha;
                tempText.color = c;
                continue;
            }
        }
        //Update alpha, and wait until next update time to repeat
        alpha += (1 / updateTimes);
        Invoke("show", fadeTime/updateTimes);
    }

    public void hide()
    {
        //Prevent recurring if a person entered or alpha reached 0
        if (showing) return;
        if (alpha < 0)
        {
            alpha = 0;
            return;
        }
        foreach (Object obj in items)
        {
            //IDK if there is a way to do a switch for types in C#
            //I know there is in other languages but this method is messier but works
            if (obj.GetType() == typeof(SpriteRenderer))
            {
                SpriteRenderer tempSprite = (SpriteRenderer)obj;
                Color c = tempSprite.color;
                c.a = alpha;
                tempSprite.color = c;
                continue;
            }
            if (obj.GetType() == typeof(TextMeshPro))
            {
                TextMeshPro tempText = (TextMeshPro)obj;
                Color c = tempText.color;
                c.a = alpha;
                tempText.color = c;
                continue;
            }
        }
        //Reduce alpha by update times
        alpha -= (1 / updateTimes);
        Invoke("hide", fadeTime / updateTimes);
    }
}
