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
        public GameObject[] blacklist;

        private void Start()
        {
            if(this.transform.parent != null)
            {
                addObjectToBlacklist(this.transform.parent.gameObject);
            }
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
            //We only want to check player first time through. This is to prevent glitches with box
            if (!collision.tag.ToLower().Equals("player")) return;
            if (!onWhitelist(collision)) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp);
        }
        private void OnTriggerStay2D(Collider2D collision)
        {
            if (onBlacklist(collision.gameObject)) return;
            if (checkParent(collision.gameObject)) return;
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
            if (onBlacklist(collision.gameObject)) return;
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

        public void addObjectToBlacklist(GameObject item)
        {
            if (onBlacklist(item)) return;
            GameObject[] temp = new GameObject[blacklist.Length + 1];
            for(int i = 0; i < blacklist.Length; i++)
            {
                temp[i] = blacklist[i];
            }
            temp[blacklist.Length] = item;
            blacklist = temp;
        }

        public bool onBlacklist(GameObject obj)
        {
            foreach(GameObject item in blacklist)
            {
                if(item == obj)
                {
                    return true;
                }
            }
            return false;
        }

        public bool checkParent(GameObject obj)
        {
            //If Parent is null, return false. This is highest level
            if (obj.transform.parent == null) return false;
            //If Parent is this, return true.
            if (obj.transform.parent == this.transform) return true;
            //If parent is not null and not this, check if the parent has a parent.
            return checkParent(obj.transform.parent.gameObject);
        }
    }
}
