using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

namespace tp2
{
    public class MovingPlatform : NetworkBehaviour
    {
        //public NetworkObject parent;
        //public float xOffset;
        //public float yOffset;
        float cooldown = 0.2f;
        float timer = 0;
        public int[] layerWhitelist;

        private void Start()
        {
            //initializeParentRpc();
        }

        private void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
        }
        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.tag.ToLower().Equals("player")) return;
            if (!onWhitelist(collision)) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (!collision.tag.ToLower().Equals("player")) return;
            if (collision.gameObject.transform.parent == this.transform) return;
            if (!onWhitelist(collision)) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp);
        }

        bool onWhitelist(Collider2D collision)
        {
            foreach (int i in layerWhitelist)
            {
                if (i == collision.gameObject.layer) return true;
            }
            return false;
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.tag.ToLower().Equals("player")) return;
            if (!onWhitelist(collision)) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            removeParentRpc(temp);
        }

        [Rpc(SendTo.Server)]
        void updateParentRpc(PlayerType type)
        {
            if (timer > 0) return;
            PlayerTypeExtensions.getObject(type).transform.parent = this.transform;
        }
        [Rpc(SendTo.Server)]
        void removeParentRpc(PlayerType type)
        {
            PlayerTypeExtensions.getObject(type).transform.parent = null;
            timer = cooldown;
        }

        [Rpc(SendTo.Server)]
        void initializeParentRpc()
        {
            //if (parent != null)
            //{
            //    this.transform.parent = parent.transform;
            //    this.transform.localPosition = new Vector3(xOffset, yOffset, 0);
            //}
        }
    }
}
