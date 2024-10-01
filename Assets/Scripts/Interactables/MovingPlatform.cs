using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public class MovingPlatform : NetworkBehaviour
    {
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.tag.ToLower().Equals("player")) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp, false);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.tag.ToLower().Equals("player")) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp, true);
        }

        [Rpc(SendTo.Server)]
        void updateParentRpc(PlayerType type, bool temp)
        {
            if (temp)
            {
                PlayerTypeExtensions.getObject(type).transform.parent = null;
            }
            else
            {
                PlayerTypeExtensions.getObject(type).transform.parent = this.transform;
            }
        }
    }
}
