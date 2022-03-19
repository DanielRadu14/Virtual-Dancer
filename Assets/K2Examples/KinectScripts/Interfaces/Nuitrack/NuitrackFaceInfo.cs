using UnityEngine;

namespace nuitrack.facedata
{
	
	[System.Serializable]
	public class FaceInfo
	{
		public string Timestamp;
		public Instances[] Instances;
	}

	[System.Serializable]
	public class Instances
	{
		public int id;
		public string @class;
		public Face face;
	}

	[System.Serializable]
	public class Face
	{
		public Rectangle rectangle;
		public Vector2[] landmark;
		public Vector2 left_eye;
		public Vector2 right_eye;
		public Angles angles;
		public Emotions emotions;
		public Age age;
		public string gender;

		public UnityEngine.Vector3 headPos;
		public UnityEngine.Quaternion headRot;
	}

	[System.Serializable]
	public class Rectangle
	{
		public float left;
		public float top;
		public float width;
		public float height;
	}

	[System.Serializable]
	public class Angles
	{
		public float yaw;
		public float pitch;
		public float roll;
	}

	[System.Serializable]
	public class Emotions
	{
		public float neutral;
		public float angry;
		public float surprise;
		public float happy;
	}

	[System.Serializable]
	public class Age
	{
		public string type;
		public float years;
	}

}

