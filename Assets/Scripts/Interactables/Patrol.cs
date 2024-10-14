using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public class Patrol : MonoBehaviour
    {
        public Transform[] locations;
        int currentPos = 0;
        public float speed;
        public float error = 0.01f;
        public bool moving = true;
        float clock = 0;
        Vector3 initialPos;
        Transform previousTransform;
        
        private void Start()
        {
            initialPos = this.transform.position;
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            if (locations == null) return;
            if (locations.Length == 0) return;
            if(speed == 0)
            {
                speed = 0.000001f;
            }
            if (!moving) return;
            if(previousTransform == null)
            {
                if (currentPos == 0)
                {
                    previousTransform = locations[locations.Length - 1];
                }
                else
                {
                    previousTransform = locations[currentPos - 1];
                }
            }
            //Goal Transform
            Transform goal = locations[currentPos];
            //Lerp from Previous Transform to Goal.
            //Calculate time it should take to go from start to end
            float time = ((Vector2.Distance(previousTransform.position, goal.position))) / speed;
            //Since a 0 time is undefined, round to a low value if it happens to be 0
            if(time == 0)
            {
                time = 0.000000001f;
            }
            Vector3 temp = Vector3.Lerp(previousTransform.position, goal.position, clock/time);
            //Prevent z from changing, since we're doing 2D with 3D positions
            temp.z = transform.position.z;
            //Update position to temp
            this.transform.position = temp;
            clock += Time.fixedDeltaTime;
            //Check if we should roll to next position
            if (Vector2.Distance(this.transform.position, goal.position) < error)
            {
                currentPos++;
                clock = 0;
                //Check Current Pos is in bounds
                if (currentPos >= locations.Length)
                {
                    currentPos = 0;
                }
                //Assign Previous Transform
                if (currentPos == 0)
                {
                    previousTransform = locations[locations.Length - 1];
                }
                else
                {
                    previousTransform = locations[currentPos - 1];
                }
            }
        }

        public void addLocation(Transform t)
        {
            Transform[] temp = new Transform[locations.Length + 1];
            for(int i = 0; i < locations.Length; i++)
            {
                temp[i] = locations[i];
            }
            temp[locations.Length] = t;
            locations = temp;
        }

        public void removeLocation(int index)
        {
            Transform[] temp = new Transform[locations.Length - 1];
            int tempLoc = 0;
            for (int i = 0; i < locations.Length; i++)
            {
                if (i == index)
                {
                    continue;
                }
                temp[tempLoc] = locations[i];
                tempLoc++;
            }
            locations = temp;
            if(currentPos == index)
            {
                currentPos++;
            }
        }

        public void removeLocation(Transform t)
        {
            int position = -1;
            for(int i = 0; i < locations.Length; i++)
            {
                if(locations[i] == t)
                {
                    position = i;
                    break;
                }
            }
            if (position == -1) return;
            removeLocation(position);
        }

        public void toggleMoving(bool m)
        {
            moving = m;
        }

        public void reset(bool status)
        {
            if (!status) return;
            reset();
        }

        public void reset()
        {
            this.transform.position = initialPos;
        }
    }
}
