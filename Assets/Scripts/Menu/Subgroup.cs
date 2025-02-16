using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class Subgroup : MonoBehaviour
    {
        public void toggleGroup(Subgroup obj)
        {
            obj.gameObject.SetActive(true);
            this.gameObject.SetActive(false);
        }
    }
}
