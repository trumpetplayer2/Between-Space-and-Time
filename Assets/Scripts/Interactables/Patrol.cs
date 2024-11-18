using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace tp2
{
    public enum PatrolEndBehavior { 
        Continue, Stop, Invert
    }
    public class Patrol : SfxHandler
    {
        public Transform[] locations;
        int currentPos = 0;
        public float speed;
        public float error = 0.01f;
        public bool moving = true;
        public bool inversion = false;
        public PatrolEndBehavior endBehavior = PatrolEndBehavior.Continue;
        float clock = 0;
        Vector3 initialPos;
        Transform previousTransform;
        Transform tempTransform = null;
        
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
                updateTransformGoal(true);
            }
            //Goal Transform
            Transform goal = locations[currentPos];
            //Prevent breaking on Null
            if(goal == null)
            {
                removeLocation(currentPos);
                if(currentPos >= locations.Length)
                {
                    currentPos = locations.Length - 1;
                }
                return;
            }
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
                updateTransformGoal();
            }
        }

        void updateTransformGoal(bool initialize = false)
        {
            if (initialize)
            {
                overridePreviousGoal();
                return;
            }
            if (!inversion)
            {
                currentPos++;
            }
            else
            {
                currentPos--;
            }
            clock = 0;
            //If at first position and inverted, or last position and not inverted, check behavior
            if ((currentPos < 0 && inversion) || (currentPos > locations.Length - 1 && !inversion))
            {
                switch (endBehavior)
                {
                    case PatrolEndBehavior.Invert:
                        inversion = !inversion;
                        if (currentPos >= locations.Length)
                        {
                            currentPos = locations.Length - 1;
                        }
                        if (currentPos < 0)
                        {
                            currentPos = 0;
                        }
                        break;
                    case PatrolEndBehavior.Stop:
                        moving = false;
                        if(currentPos >= locations.Length)
                        {
                            currentPos = locations.Length - 1;
                        }
                        if(currentPos < 0)
                        {
                            currentPos = 0;
                        }
                        return;
                    case PatrolEndBehavior.Continue:
                        //Check Current Pos is in bounds
                        if (currentPos >= locations.Length)
                        {
                            currentPos = 0;
                        }
                        if (currentPos < 0)
                        {
                            currentPos = locations.Length - 1;
                        }
                        break;
                }
            }
            //Assign Previous Transform
            //if (!inversion)
            //{

            //    if (currentPos == 0)
            //    {
            //        if (endBehavior == PatrolEndBehavior.Continue)
            //        {
            //            previousTransform = locations[locations.Length - 1];
            //        }
            //        else
            //        {
            //            previousTransform = locations[0];
            //        }
            //    }
            //    else
            //    {
            //        previousTransform = locations[currentPos - 1];
            //    }
            //}
            //else
            //{
            //    if (currentPos < locations.Length - 1)
            //    {
            //        previousTransform = locations[currentPos + 1];
            //    }
            //    else
            //    {
            //        previousTransform = locations[0];
            //    }
            //}
            overridePreviousGoal();
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
            playClip(m);
            moving = m;
        }

        public void reset(bool status)
        {
            if (!status) return;
            reset();
        }

        public void toggleInversionMove()
        {
            toggleDirection(!inversion);
        }

        public void toggleDirection(bool inv)
        {
            inversion = inv;
            toggleMoving(true);
            updateTransformGoal();
        }

        public void invToggleDirection(bool inv)
        {
            toggleDirection(!inv);
        }

        public void overridePreviousGoal()
        {
            if (tempTransform == null)
            {
                tempTransform = new GameObject("tempPlatformLoc").transform;
            }
            tempTransform.position = transform.position;
            previousTransform = tempTransform;
            clock = 0;
        }
        public void reset()
        {
            this.transform.position = initialPos;
            currentPos = 0;
            overridePreviousGoal();
            clock = 0;
        }
    }
}
