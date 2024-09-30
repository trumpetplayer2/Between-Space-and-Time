using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace tp2
{
    public class CameraFollow : MonoBehaviour
    {
        //Camera Follow Variables
        public float maxX = 7;
        public float minX = -7;
        public float maxY = 10;
        public float minY = 0;
        public float distance = -10;
        public Transform playerTracker;
        public float smoothSpeed = 0.125f;
        public Vector3 locationOffset;
        public Vector3 rotationOffset;
        //Camera Shake Variables
        public Transform cameraTransform;
        public float shakeAmount = 0.25f;
        public float decreaseSpeed = 1.0f;
        public float shakeDuration = 0f;
        public static CameraFollow instance;
        //Cinematic Camera
        public bool cinematicMode = false;
        public int cinematicNumber = 0;
        public Cutscene[] cutscenes;
        int sceneNumber = 0;
        int positionNumber = 0;
        float time = 0;
        Vector3 startPos;

        public void Awake()
        {
            instance = this;
        }

        public void Start()
        {
            if (NetManager.instance != null)
            {
                foreach (InitializePlayer init in NetManager.instance.players)
                {
                    if (init == null) continue;
                    //Update spawn positions and camera
                    init.updateCameraRpc();
                }
            }
            if (cinematicMode)
            {
                startCutscene(0);
            }
        }

        public void startCutscene(int number)
        {
            sceneNumber = number;
            positionNumber = 0;
            time = 0;
            NetPlayer.paused = true;
            startPos = this.transform.position;
            GameManager.instance.finishDialogueRpc(PlayerType.None, true);
        }

        public void endCutscene()
        {
            cinematicMode = false;
            NetPlayer.paused = false;
            cutscenes[sceneNumber].cutsceneEnd();
            //Attempt to redirect scenes if the cutscene redirects player
            if (cutscenes[sceneNumber].redirectScene > 0)
            {
                NetManager.instance.updateScene(cutscenes[sceneNumber].redirectScene);
            }
        }

        public void FixedUpdate()
        {
            if (cinematicMode)
            {
                cinematicCamUpdate();
            }
            else
            {
                followCamUpdate();
            }
        }

        private void cinematicCamUpdate()
        {
            NetPlayer.paused = true;
            if (sceneNumber > cutscenes.Length) return;
            Cutscene cutscene = cutscenes[sceneNumber];
            if (positionNumber >= cutscene.cameraPositions.Length)
            {
                switch (playerTracker.gameObject.layer)
                {
                    case 6:
                        //Atlas
                        GameManager.instance.finishDialogueRpc(PlayerType.Atlas, false);
                        break;
                    case 7:
                        //Chroma
                        GameManager.instance.finishDialogueRpc(PlayerType.Chroma, false);
                        break;
                }
                if (!(GameManager.instance.getAtlasInCutscene() || GameManager.instance.getChromaInCutscene()))
                {
                    endCutscene();
                }
                else
                {
                    //Add waiting text
                }
                return;
            }
            time += Time.fixedDeltaTime;
            if(cutscene.timing[positionNumber] <= 0)
            {
                try
                {
                    if (cutscene.dialogueBox[positionNumber].complete)
                    {
                        positionNumber += 1;
                        return;
                    }
                }
                catch
                {
                    Debug.Log("Dialogue error?");
                    return;
                }
                //This is a freeze frame. Return until both players update
                if(cutscene.dialogueBox[positionNumber] != null)
                {
                    cutscene.dialogueBox[positionNumber].gameObject.SetActive(true);
                }
                return;
            }
            if(time >= cutscene.timing[positionNumber])
            {
                time = 0;
                positionNumber += 1;
                startPos = this.transform.position;
                if(positionNumber >= cutscene.cameraPositions.Length)
                {
                    return;
                }

            }
            if (cutscene.timing[positionNumber] <= 0)
            {
                return;
            }
            Transform cutscenePos = cutscene.cameraPositions[positionNumber];
            Vector3 trackerPos = new Vector3(cutscenePos.position.x, cutscenePos.position.y, distance);
            //Check player relation to camera
            Vector3 desiredPosition = trackerPos + cutscenePos.rotation * locationOffset;
            Vector3 smoothedPosition = Vector3.Lerp(startPos, desiredPosition, time/cutscene.timing[positionNumber]);

            transform.position = smoothedPosition;

            Quaternion desiredrotation = cutscenePos.rotation * Quaternion.Euler(rotationOffset);
            Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothSpeed);
            transform.rotation = smoothedrotation;
        }

        private void followCamUpdate()
        {
            
            if (playerTracker == null) return;
            if (GameManager.instance.getAtlasInCutscene() || GameManager.instance.getChromaInCutscene())
            {
                switch (playerTracker.gameObject.layer)
                {
                    case 6:
                        //Atlas
                        GameManager.instance.finishDialogueRpc(PlayerType.Atlas, false);
                        break;
                    case 7:
                        //Chroma
                        GameManager.instance.finishDialogueRpc(PlayerType.Chroma, false);
                        break;
                }
            }
            
            Vector3 tempTracker = new Vector3(playerTracker.position.x, playerTracker.position.y, distance);

            if (playerTracker.position.x < minX)
            {
                tempTracker = new Vector3(minX, playerTracker.position.y, distance);
            }
            else if (playerTracker.position.x > maxX)
            {
                tempTracker = new Vector3(maxX, playerTracker.position.y, distance);
            }

            if (playerTracker.position.y < minY)
            {
                tempTracker = new Vector3(tempTracker.x, minY, distance);
            }
            else if (playerTracker.position.y > maxY)
            {
                tempTracker = new Vector3(tempTracker.x, maxY, distance);
            }
            //Check player relation to camera
            Vector3 desiredPosition = tempTracker + playerTracker.rotation * locationOffset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);

            transform.position = smoothedPosition;

            Quaternion desiredrotation = playerTracker.rotation * Quaternion.Euler(rotationOffset);
            Quaternion smoothedrotation = Quaternion.Lerp(transform.rotation, desiredrotation, smoothSpeed);
            transform.rotation = smoothedrotation;
        }

        private void Update()
        {
            if (shakeDuration > 0)
            {
                cameraTransform.localPosition = cameraTransform.position + Random.insideUnitSphere * shakeAmount;

                shakeDuration -= Time.deltaTime * decreaseSpeed;
            }
        }
    }
}
