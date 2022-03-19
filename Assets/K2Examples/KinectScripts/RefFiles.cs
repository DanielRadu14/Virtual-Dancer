using FramesKinect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RefFiles
{
    public class RefFilesManager : MonoBehaviour
    {
        public const int Hips = 0;
        public const int Spine = 1;
        public const int Chest = 2;
        public const int Neck = 3;
        public const int Head = 4;

        public const int LeftUpperArm = 5;
        public const int LeftLowerArm = 6;
        public const int LeftHand = 7;
        public const int LeftIndexProximal = 8;
        public const int LeftIndexIntermediate = 9;
        public const int LeftThumbProximal = 10;

        public const int RightUpperArm = 11;
        public const int RightLowerArm = 12;
        public const int RightHand = 13;
        public const int RightIndexProximal = 14;
        public const int RightIndexIntermediate = 15;
        public const int RightThumbProximal = 16;

        public const int LeftUpperLeg = 17;
        public const int LeftLowerLeg = 18;
        public const int LeftFoot = 19;
        public const int LeftToes = 20;

        public const int RightUpperLeg = 21;
        public const int RightLowerLeg = 22;
        public const int RightFoot = 23;
        public const int RightToes = 24;

        public const int LeftShoulder = 25;
        public const int RightShoulder = 26;
        public const int LeftIndexProximal_ = 27;
        public const int RightIndexProximal_ = 28;
        public const int LeftThumbProximal_ = 29;
        public const int RightThumbProximal_ = 30;

        public static Dictionary<int, float> upperWeights = new Dictionary<int, float>()
        {
            {Hips, 1.0f},
            {Spine, 1.0f},
            {Chest, 0.0f},
            {Neck, 0.5f},
            {Head, 0.5f},

            {LeftUpperArm, 0.7f},
            {LeftLowerArm, 1.0f},
            {LeftHand, 0.8f},
            {LeftIndexProximal, 0.0f},
            {LeftIndexIntermediate, 0.0f},
            {LeftThumbProximal, 0.0f},

            {RightUpperArm, 0.7f},
            {RightLowerArm, 1.0f},
            {RightHand, 0.8f},
            {RightIndexProximal, 0.0f},
            {RightIndexIntermediate, 0.0f},
            {RightThumbProximal, 0.0f},

            {LeftUpperLeg, 0.0f},
            {LeftLowerLeg, 0.0f},
            {LeftFoot, 0.0f},
            {LeftToes, 0.0f},

            {RightUpperLeg, 0.0f},
            {RightLowerLeg, 0.0f},
            {RightFoot, 0.0f},
            {RightToes, 0.0f},

            {LeftShoulder, 0.0f},
            {RightShoulder, 0.0f},
            {LeftIndexProximal_, 0.0f},
            {RightIndexProximal_, 0.0f},
            {LeftThumbProximal_, 0.0f},
            {RightThumbProximal_, 0.0f},
        };

        public static Dictionary<string, Dictionary<int, float>> ref_gestures = new Dictionary<string, Dictionary<int, float>>();
        public static Dictionary<string, Dictionary<int, float>> RefGestureDatabase()
        {
            ref_gestures["Record0.txt"] = upperWeights;
            ref_gestures["Record1.txt"] = upperWeights;
            ref_gestures["Record2.txt"] = upperWeights;
            ref_gestures["Record3.txt"] = upperWeights;
            ref_gestures["Record4.txt"] = upperWeights;

            return ref_gestures;
        }
    }
}
