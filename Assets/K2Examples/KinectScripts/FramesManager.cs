using RefFiles;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace FramesKinect
{
    public struct FrameData
    {
        public Quaternion[] quaternions;
        public Vector3 rootPosition;
    }

    public class FramesManager: MonoBehaviour
    {
        public const int no_skel = 25;
        public const int no_bones = 31;

        /// <summary>
        /// Returns true if the frames are similar under a certain tolerance.
        /// </summary>
        /// <param name="userFrame"></param>
        /// <param name="refFrame"></param>
        /// <param name="eps">tolerance</param>
        /// <returns>true/false</returns>
        public static bool matchFrames(FrameData userFrame, FrameData refFrame, float eps)
        {
            for (int i = 0; i < no_bones; i++)
            {
                Vector3 diffEuler = userFrame.quaternions[i].eulerAngles - refFrame.quaternions[i].eulerAngles;
                if (diffEuler.x < 0f)
                {
                    diffEuler.x = 360f + diffEuler.x;
                }

                if (diffEuler.y < 0f)
                {
                    diffEuler.y = 360f + diffEuler.y;
                }

                if (diffEuler.z < 0f)
                {
                    diffEuler.z = 360f + diffEuler.z;
                }

                if ((diffEuler.x >= eps && diffEuler.x <= 360f - eps) ||
                    (diffEuler.y >= eps && diffEuler.y <= 360f - eps) ||
                    (diffEuler.z >= eps && diffEuler.z <= 360f - eps))
                {
                    //Debug.Log("Frames don't match! " + diffEuler.x + " " + diffEuler.y + " " + diffEuler.z);
                    return false;
                }
            }

            Debug.Log("Frames Matched!");
            return true;
        }

        public static double[] ComputeWeights(string ref_file)
        {
            string[] comp = ref_file.Split(Path.DirectorySeparatorChar);
            Dictionary<int, float> refGestureWeights;
            try
            {
                refGestureWeights = RefFilesManager.RefGestureDatabase()[comp[comp.Length - 1]];
            }
            catch (KeyNotFoundException e)
            {
                //default weights
                refGestureWeights = RefFilesManager.upperWeights;
            }

            double[] weights = new double[no_bones];
            for (int i = 0; i < no_bones; i++)
            {
                weights[i] = refGestureWeights[i];
            }

            return weights;
        }
    }
}
