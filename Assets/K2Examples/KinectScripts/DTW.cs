using FramesKinect;
using RefFiles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class DTW
{
    enum Direction
    {
        NULL = -1,
        LEFT = 0,
        DIAGONAL = 1,
        UP = 2,
    };

    // method 1
    private List<Vector2> path_DTW;

    // method 2
    private List<List<Vector2>> paths_DTW;

    private static int noFiles = 0;
    private int noRepetitions = 0;
    private List<FrameData[]> user_frames;
    private FrameData[] ref_frames;
    private int currentRep = 0;

    private double[,] distance;             // cost matrix
    private double[,] results_matrix;       // DTW matrix
    private Direction[,] directions;        // directions

    // errors per Joint
    Dictionary<int, double> errorsForAllJoints;

    // quaternions weights
    private static double[] weights;

    public List<Vector2> Path_DTW
    {
        get { return path_DTW; }
    }

    public List<List<Vector2>> Paths_DTW
    {
        get { return paths_DTW; }
    }

    /// <summary>
    /// Computes the distance between two points in 3D space.
    /// </summary>
    public static double EuclideanDistance(Quaternion point_x, Quaternion point_y)
    {
        return Math.Sqrt(SqrEuclideanDistance(point_x, point_y));
    }

    public static double SqrEuclideanDistance(Quaternion point_x, Quaternion point_y)
    {
        return Math.Pow((point_x.x - point_y.x), 2) + Math.Pow((point_x.y - point_y.y), 2) + Math.Pow((point_x.z - point_y.z), 2) + Math.Pow((point_x.w - point_y.w), 2);
    }

    public static double ManhattanDistance(Quaternion point_x, Quaternion point_y)
    {
        return Math.Abs(point_x.x - point_y.x) + Math.Abs(point_x.y - point_y.y) + Math.Abs(point_x.z - point_y.z) + Math.Abs(point_x.w - point_y.w);
    }

    public static double AkrdisDistance(Quaternion point_x, Quaternion point_y)
    {
        float a = 0.7f;
        return EuclideanDistance(point_x, point_y) * (1 - a) + ManhattanDistance(point_x, point_y) * a;
    }

    /// <summary>
    /// Compare 2 quaternions.
    /// </summary>
    public static double CompareQuaternions(Quaternion q1, Quaternion q2)
    {
        float norm1 = (float)(Math.Sqrt(q1.w * q1.w + q1.x * q1.x + q1.y * q1.y + q1.z * q1.z));
        float norm2 = (float)(Math.Sqrt(q2.w * q2.w + q2.x * q2.x + q2.y * q2.y + q2.z * q2.z));

        if (norm1 == 0)
        {
            return norm2;
        }
        else if (norm2 == 0)
        {
            return norm1;
        }

        // unit quaternions
        Quaternion norm_q1 = new Quaternion(q1.x / norm1, q1.y / norm1, q1.z / norm1, q1.w / norm1);
        Quaternion norm_q2 = new Quaternion(q2.x / norm2, q2.y / norm2, q2.z / norm2, q2.w / norm2);

        //  1 - |<u1, u2>|
        return 1 - Math.Abs(norm_q1.x * norm_q2.x + norm_q1.y * norm_q2.y + norm_q1.z * norm_q2.z + norm_q1.w * norm_q2.w);
    }

    /// <summary>
    /// Compare 2 frames.
    /// </summary>
    public static double CompareFrames(FrameData f1, FrameData f2)
    {
        double sum = 0.0;
        for (int i = 0; i < f1.quaternions.Length; i++)
        {
            sum += weights[i] * CompareQuaternions(f1.quaternions[i], f2.quaternions[i]);
        }

        return sum / f1.quaternions.Length;
    }

    /// <summary>
    /// Read data from user and reference files and init global structures.
    /// </summary>
    private void readData(string user_folder, string ref_file)
    {
        // load user data
        int user_frames_count = 0;
        noFiles = Directory.GetFiles(user_folder).Length;
        user_frames = new List<FrameData[]>();

        noRepetitions = AvatarController.maxRepetitions;

        string user_file;
        for (int i = 0; i < noFiles; i++)
        {
            user_frames_count = 0;
            user_file = user_folder + "UserRecord" + i + ".txt";
            user_frames.Add(AvatarController.loadFrames(user_file, ref user_frames_count));
        }

        // load ref data
        //int ref_frames_count = 0;
        //ref_frames = AvatarController.loadFrames(ref_file, ref ref_frames_count);

        int ref_frames_count = AvatarController.framesCount;
        ref_frames = AvatarController.frames;

        weights = FramesManager.ComputeWeights(ref_file);
    }

    private void initDTW()
    {
        distance = new double[user_frames[currentRep].Length, ref_frames.Length];
        results_matrix = new double[user_frames[currentRep].Length + 1, ref_frames.Length + 1];
        directions = new Direction[user_frames[currentRep].Length + 1, ref_frames.Length + 1];

        // compute initial distance between two gestures
        for (int i = 0; i < user_frames[currentRep].Length; i++)
        {
            for (int j = 0; j < ref_frames.Length; j++)
            {
                distance[i, j] = CompareFrames(user_frames[currentRep][i], ref_frames[j]);
            }
        }

        for (int i = 0; i <= user_frames[currentRep].Length; ++i)
        {
            for (int j = 0; j <= ref_frames.Length; ++j)
            {
                results_matrix[i, j] = -1.0;
            }
        }

        for (int i = 1; i <= user_frames[currentRep].Length; ++i)
        {
            results_matrix[i, 0] = double.PositiveInfinity;
        }

        for (int j = 1; j <= ref_frames.Length; ++j)
        {
            results_matrix[0, j] = double.PositiveInfinity;
        }

        results_matrix[0, 0] = 0.0;
        directions[0, 0] = Direction.NULL;
    }

    /// <summary>
    /// Constructor.
    /// </summary>
    public DTW(string user_folder, string ref_file)
    {
        Debug.Log(user_folder + " >> " + ref_file);
        readData(user_folder, ref_file);
    }

    /// <summary>
    /// DTW algorithm.
    /// </summary>
    private double computeDist()
    {
        double dist, left, diag, up;

        // init
        initDTW();

        for (int i = 1; i <= user_frames[currentRep].Length; i++)
        {
            for (int j = 1; j <= ref_frames.Length; j++)
            {
                dist = distance[i - 1, j - 1];

                left = results_matrix[i, j - 1];
                diag = results_matrix[i - 1, j - 1];
                up = results_matrix[i - 1, j];

                if (left <= up && left <= diag)
                {
                    results_matrix[i, j] = left + dist;
                    directions[i, j] = Direction.LEFT;
                }
                else if (up <= left && up <= diag)
                {
                    results_matrix[i, j] = up + dist;
                    directions[i, j] = Direction.UP;
                }
                else
                {
                    results_matrix[i, j] = diag + dist;
                    directions[i, j] = Direction.DIAGONAL;
                }
            }
        }

        return results_matrix[user_frames[currentRep].Length, ref_frames.Length];
    }

    /// <summary>
    /// Compute DTW path.
    /// </summary>
    private List<Vector2> computePath()
    {
        List<Vector2> path = new List<Vector2>();
        int i = user_frames[currentRep].Length;
        int j = ref_frames.Length;

        while (i > 0 && j > 0)
        {
            path.Add(new Vector2(i - 1, j - 1));
            if (directions[i, j] == Direction.LEFT)
            {
                j--;
            }
            else if (directions[i, j] == Direction.UP)
            {
                i--;
            }
            else
            {
                i--;
                j--;
            }
        }

        path.Reverse();

        return path;
    }

    /// <summary>
    /// Init structures for computing joint's error.
    /// </summary>
    private void initErrorComparator()
    {
        errorsForAllJoints = new Dictionary<int, double>();

        // init dictionary with the most important joints Id
        errorsForAllJoints.Add(RefFilesManager.Hips, 0);
        errorsForAllJoints.Add(RefFilesManager.Spine, 0);
        errorsForAllJoints.Add(RefFilesManager.Chest, 0);
        errorsForAllJoints.Add(RefFilesManager.Neck, 0);

        errorsForAllJoints.Add(RefFilesManager.LeftUpperArm, 0);
        errorsForAllJoints.Add(RefFilesManager.LeftLowerArm, 0);
        errorsForAllJoints.Add(RefFilesManager.LeftHand, 0);

        errorsForAllJoints.Add(RefFilesManager.RightUpperArm, 0);
        errorsForAllJoints.Add(RefFilesManager.RightLowerArm, 0);
        errorsForAllJoints.Add(RefFilesManager.RightHand, 0);

        errorsForAllJoints.Add(RefFilesManager.LeftUpperLeg, 0);
        errorsForAllJoints.Add(RefFilesManager.LeftLowerLeg, 0);
        errorsForAllJoints.Add(RefFilesManager.LeftFoot, 0);

        errorsForAllJoints.Add(RefFilesManager.RightUpperLeg, 0);
        errorsForAllJoints.Add(RefFilesManager.RightLowerLeg, 0);
        errorsForAllJoints.Add(RefFilesManager.RightFoot, 0);
    }

    /// <summary>
    /// Computes the difference between ref and current joint data.
    /// </summary>
    /// <returns></returns>
    public Dictionary<int, double> computeErrorPerJoint()
    {
        Dictionary<int, double> jointErrors = new Dictionary<int, double>();
        double[,,] dist_joints;                       // cost matrix
        double[,,] results_matrix_joints;             // DTW matrix

        dist_joints = new double[FramesManager.no_bones, user_frames[currentRep].Length, ref_frames.Length];
        results_matrix_joints = new double[FramesManager.no_bones, user_frames[currentRep].Length + 1, ref_frames.Length + 1];

        double dist, left, diag, up;

        foreach (int key in errorsForAllJoints.Keys)
        {
            // compute initial distance between two gestures
            for (int i = 0; i < user_frames[currentRep].Length; i++)
            {
                // compute distance matrix for all given joints
                for (int j = 0; j < ref_frames.Length; j++)
                {
                    dist_joints[key, i, j] = CompareQuaternions(user_frames[currentRep][i].quaternions[key], ref_frames[j].quaternions[key]);
                }
            }

            // init 3D structures
            for (int i = 0; i <= user_frames[currentRep].Length; i++)
            {
                for (int j = 0; j <= ref_frames.Length; j++)
                {
                    results_matrix_joints[key, i, j] = -1.0;
                }
            }

            for (int i = 1; i <= user_frames[currentRep].Length; ++i)
            {
                results_matrix_joints[key, i, 0] = double.PositiveInfinity;
            }

            for (int j = 1; j <= ref_frames.Length; ++j)
            {
                results_matrix_joints[key, 0, j] = double.PositiveInfinity;
            }

            results_matrix_joints[key, 0, 0] = 0.0;
            for (int i = 1; i <= user_frames[currentRep].Length; i++)
            {
                for (int j = 1; j <= ref_frames.Length; j++)
                {
                    dist = dist_joints[key, i - 1, j - 1];

                    left = results_matrix_joints[key, i, j - 1];
                    diag = results_matrix_joints[key, i - 1, j - 1];
                    up = results_matrix_joints[key, i - 1, j];

                    if (left <= up && left <= diag)
                    {
                        results_matrix_joints[key, i, j] = left + dist;
                    }
                    else if (up <= left && up <= diag)
                    {
                        results_matrix_joints[key, i, j] = up + dist;
                    }
                    else
                    {
                        results_matrix_joints[key, i, j] = diag + dist;
                    }
                }
            }

            jointErrors.Add(key, results_matrix_joints[key, user_frames[currentRep].Length, ref_frames.Length]);
        }

        return jointErrors;
    }

    /// <summary>
    /// Save computed errors.
    /// </summary>
    /*private void writeErrorsToFile()
    {
        StreamWriter sWriter = new StreamWriter(KinectManager.userProfileConfig, false);

        foreach (KeyValuePair<int, double> entry in errorsForAllJoints)
        {
            sWriter.WriteLine(entry.Key + " - " + ((entry.Value > 0.6) ? 1 : 0));
        }
        sWriter.Close();
    }*/

    /// <summary>
    /// Eval two gestures using DTW.
    /// </summary>
    public double getResult()
    {
        // get the result of the current comparation
        double res_diff = computeDist();

        //  reconstruct path
        path_DTW = computePath();

        // normalize result
        //float MIN_VAL = 15.0f;
        //float MAX_VAL = 100.0f;
        //double resDTW = Math.Max(MIN_VAL, (MAX_VAL - MIN_VAL) / res_diff);

        // normalize result
        const double MAX_VAL = 90;
        double resDTW = (1 - res_diff / MAX_VAL) * 100;

        Debug.Log("Result DTW Simple Method: " + res_diff + "(" + resDTW + ")");

        return resDTW;
    }

    public static void deleteFiles()
    {
        for (int i = 0; i < noFiles; i++)
        {
            string user_file = AvatarController.user_records_directory + "UserRecord" + i + ".txt";
            if(File.Exists(user_file))
            {
                File.Delete(user_file);
            }
        }
        AvatarController.noReps = 0;
    }

    /// <summary>
    /// Eval two gestures using DTW for each gesture file.
    /// </summary>
    public double getCompResult()
    {
        paths_DTW = new List<List<Vector2>>();
        List<double> results = new List<double>();

        List<Vector2> path_DTW;
        double res;

        initErrorComparator();
        List<int> keys = errorsForAllJoints.Keys.ToList<int>();

        for (int i = 0; i < noRepetitions; i++)
        {
            if (i >= noFiles)
            {
                results.Add(0.0f);

                continue;
            }

            // current Rep
            currentRep = i;

            path_DTW = new List<Vector2>();

            // get the result of the current comparation
            res = computeDist();

            //  reconstruct path
            path_DTW = computePath();

            //  save it
            results.Add(res);
            paths_DTW.Add(path_DTW);

            // compute joint errors
            Dictionary<int, double> jointErrors = new Dictionary<int, double>();
            jointErrors = computeErrorPerJoint();

            foreach (var key in keys)
            {
                double value = 0;
                errorsForAllJoints.TryGetValue(key, out value);
                errorsForAllJoints[key] = value + jointErrors[key];
            }
        }

        // compute max val
        double maxVal = results.Max();

        // debug
        string debug = "DTW for each rep: <" + maxVal + "> ";

        double res_diff = 0;
        for (int i = 0; i < noFiles; i++)
        {
            debug += " " + results[i].ToString();

            //results[i] /= maxVal;
            res_diff += (Math.Max(20, maxVal) - results[i]) / Math.Max(15, maxVal) * (100f / noRepetitions);
        }
        Debug.Log(debug);

        // compute errorsForAllJoints
        foreach (var key in keys)
        {
            errorsForAllJoints[key] /= noFiles;
        }

        // normalize result
        double maxError = errorsForAllJoints.Values.Max();
        foreach (var key in keys)
        {
            errorsForAllJoints[key] /= maxError;
        }

        return res_diff;
    }
}