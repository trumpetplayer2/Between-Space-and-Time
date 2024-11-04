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
        float cooldown = 0.15f;
        float timer = 0;
        public int[] layerWhitelist;
        public GameObject[] blacklist;
        public SpriteRenderer[] fakePlayers = new SpriteRenderer[2];
        Vector3[] prevContacts = new Vector3[2];
        float check = 0f;
        float updateCheck = 0.2f;
        Rigidbody2D body;
        

        private void Start()
        {
            if(this.transform.parent != null)
            {
                addObjectToBlacklist(this.transform.parent.gameObject);
            }
            foreach(SpriteRenderer r in fakePlayers)
            {
                Vector3 scale = r.transform.localScale;
                r.transform.localScale = Vector3.one;
                r.transform.localScale = new Vector3(scale.x / transform.lossyScale.x, scale.y / transform.lossyScale.y, scale.z / transform.lossyScale.z);
            }
        }

        private void Update()
        {
            if (timer > 0)
            {
                timer -= Time.deltaTime;
            }
            check += Time.deltaTime;
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
            if (collision.tag.ToLower().Equals("player"))
            {
                return;
            }
            if (check > updateCheck)
            {
                fakePlayer(collision);
            }
            if (onBlacklist(collision.gameObject)) return;
            if (checkParent(collision.gameObject)) return;
            PlayerType temp = PlayerTypeExtensions.getEnumOf(collision.gameObject);
            if (temp == PlayerType.None) return;
            updateParentRpc(temp);
        }

        private void fakePlayer(Collider2D collision)
        {
            if (!(collision.gameObject.tag.ToLower().Equals("player"))) return;
            if (!onWhitelist(collision)) return;
            
            int playerId = 0;
            if (collision.gameObject.layer == 6)
            {
                playerId = 0;
            }
            else if (collision.gameObject.layer == 7)
            {
                playerId = 1;
            }
            //This should only be run by owner
            if (PlayerTypeExtensions.getLocalPlayerType() != PlayerTypeExtensions.getTypeFromValue(playerId)) return;
            check = 0f;
            ContactPoint2D[] contacts = new ContactPoint2D[2];
            collision.GetContacts(contacts);
            bool moved = false;
            Vector3[] temp = new Vector3[contacts.Length];
            for (int i = 0; i < contacts.Length; i++)
            {
                //This needs to be done for local, as moving platform
                Vector3 localPos = transform.InverseTransformPoint(contacts[i].point);
                temp[i] = localPos;
                if (Vector2.Distance(localPos, prevContacts[i]) > 0.1f)
                {
                    moved = true;
                    //Hide the fake object, and show real one
                    toggleFakeRpc(false, playerId, transform.InverseTransformPoint(collision.gameObject.transform.position));
                    break;
                }
            }
            if (!moved)
            {
                //Player didn't move, check if the fake player is enabled. If not, enable it
                if (!fakePlayers[playerId].enabled)
                {
                    //Show fake object, hide real one
                    toggleFakeRpc(true, playerId, transform.InverseTransformPoint(collision.gameObject.transform.position));
                }
            }
            //Prev Contacts should be a localized position
            prevContacts = temp;
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
            toggleFakeRpc(false, PlayerTypeExtensions.getIDof(temp), this.transform.position);
            removeParentRpc(temp);
        }

        [Rpc(SendTo.NotMe)]
        void toggleFakeRpc(bool state, int playerId, Vector3 pos)
        {
            //Determine the "Show" pos from local pos
            pos = transform.TransformPoint(pos);
            if (state)
            {
                //Move fake player and show
                fakePlayers[playerId].transform.position = pos;
                //Toggle Fake Player
                fakePlayers[playerId].enabled = true;
                //Hide real player
                GameObject player = PlayerTypeExtensions.getObject(playerId);
                if(player == null)
                {
                    Debug.LogWarning("WARNING: Player was null while toggling fake");
                    return;
                }
                SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
                if(playerRenderer == null)
                {
                    Debug.LogWarning("WARNING: Player did not have a sprite renderer while toggling fake");
                }
                playerRenderer.enabled = false;
            }
            else
            {
                //Toggle Fake Player
                fakePlayers[playerId].enabled = false;
                //Show real player
                GameObject player = PlayerTypeExtensions.getObject(playerId);
                if (player == null)
                {
                    Debug.LogWarning("WARNING: Player was null while toggling fake");
                    return;
                }
                SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
                if (playerRenderer == null)
                {
                    Debug.LogWarning("WARNING: Player did not have a sprite renderer while toggling fake");
                }
                playerRenderer.enabled = true;
            }
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
            try
            {
                PlayerTypeExtensions.getObject(type).transform.parent = null;
                timer = cooldown;
            }
            catch { }
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

        private new void OnDestroy()
        {
            //Prevent accidental destruction of players if on a destroyed moving platform
            foreach(NetPlayer child in this.GetComponentsInChildren<NetPlayer>())
            {
                if(child.tag == "player")
                {
                    removeParentRpc(PlayerTypeExtensions.getTypeof(child.gameObject));
                }
            }
        }
    }
}
