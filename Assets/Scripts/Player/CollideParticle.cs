using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public class CollideParticle : NetworkBehaviour
    {
        public LineRenderer line;
        public int[] layerCheck;
        public ContactFilter2D filter;
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!onWhitelist(collision)) return;
            if (!IsOwner) return;
            ContactPoint2D[] contacts = new ContactPoint2D[2];
            int count = Physics2D.GetContacts(collision, filter, contacts);
            line.positionCount = count;
            if (line == null) return;
            for (int i = 0; i < count; i++)
            {
                line.SetPosition(i, contacts[i].point);
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!onWhitelist(collision)) return;
            if (!IsOwner) return;
            line.positionCount = 0;
        }

        private void Update()
        {
            if (!IsOwner) return;
            Vector3[] temp = new Vector3[100];
            line.GetPositions(temp);
            updateLineRpc(line.positionCount, temp);
        }

        [Rpc(SendTo.NotOwner)]
        public void updateLineRpc(int count, Vector3[] pos)
        {
            line.positionCount = count;
            line.SetPositions(pos);
        }

        bool onWhitelist(Collider2D collision)
        {
            foreach (int i in layerCheck)
            {
                if (i == collision.gameObject.layer) return true;
            }
            return false;
        }
    }
}
