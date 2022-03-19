using UnityEngine;
//using Windows.Kinect;
using System.Collections;
using System;


public class SimplePoseListener : MonoBehaviour, KinectGestures.GestureListenerInterface
{
	[Tooltip("Index of the player, tracked by this component. 0 means the 1st player, 1 - the 2nd one, 2 - the 3rd one, etc.")]
	public int playerIndex = 0;

	[Tooltip("UI-Text to display gesture-listener messages and gesture information.")]
	public UnityEngine.UI.Text gestureInfo;
	
	// private bool to track if progress message has been displayed
	private bool progressDisplayed;
	private float progressGestureTime;

	// singleton instance of the class
	private static SimplePoseListener instance = null;

	// whether the needed gesture has been detected or not
	private bool touchLeftElbow = false;
	private bool touchRightElbow = false;


	// returns the singleton instance of the class
	public static SimplePoseListener Instance
	{
		get
		{
			return instance;
		}
	}


	// determines whether the user touched his/her left elbow
	public bool IsTouchingLeftElbow()
	{
		if(touchLeftElbow)
		{
			touchLeftElbow = false;
			return true;
		}

		return false;
	}

	// determines whether the user touched his/her right elbow
	public bool IsTouchingRightElbow()
	{
		if(touchRightElbow)
		{
			touchRightElbow = false;
			return true;
		}

		return false;
	}


	public void UserDetected(long userId, int userIndex)
	{
		if (userIndex != playerIndex)
			return;

		// as an example - detect these user specific gestures
		KinectManager manager = KinectManager.Instance;

		manager.DetectGesture(userId, KinectGestures.Gestures.TouchRightElbow);
		manager.DetectGesture(userId, KinectGestures.Gestures.TouchLeftElbow);

		if(gestureInfo != null)
		{
			gestureInfo.text = "Looking for TouchedRightElbow or TouchedLeftElbow";
		}
	}
	
	public void UserLost(long userId, int userIndex)
	{
		if (userIndex != playerIndex)
			return;

		if(gestureInfo != null)
		{
			gestureInfo.text = string.Empty;
		}
	}

	public void GestureInProgress(long userId, int userIndex, KinectGestures.Gestures gesture, 
	                              float progress, KinectInterop.JointType joint, Vector3 screenPos)
	{
		if (userIndex != playerIndex)
			return;

		if (progress >= 0.1f && gestureInfo != null) 
		{
			string sGestureText = string.Format ("{0} {1:F0}%", gesture, progress * 100f);
			gestureInfo.text = sGestureText;

			progressDisplayed = true;
			progressGestureTime = Time.realtimeSinceStartup;
		}
	}

	public bool GestureCompleted(long userId, int userIndex, KinectGestures.Gestures gesture, 
	                              KinectInterop.JointType joint, Vector3 screenPos)
	{
		if (userIndex != playerIndex)
			return false;

		string sGestureText = string.Format ("{0} detected", gesture);
		if(gestureInfo != null)
		{
			gestureInfo.text = sGestureText;
			progressDisplayed = false;
		}

		if(gesture == KinectGestures.Gestures.TouchLeftElbow)
			touchLeftElbow = true;
		else if(gesture == KinectGestures.Gestures.TouchRightElbow)
			touchRightElbow = true;

		return true;
	}

	public bool GestureCancelled(long userId, int userIndex, KinectGestures.Gestures gesture, 
	                              KinectInterop.JointType joint)
	{
		if (userIndex != playerIndex)
			return false;

		if(progressDisplayed)
		{
			progressDisplayed = false;

			if(gestureInfo != null)
			{
				gestureInfo.text = String.Empty;
			}
		}
		
		return true;
	}


	void Awake()
	{
		instance = this;
	}


	public void Update()
	{
		if(progressDisplayed && ((Time.realtimeSinceStartup - progressGestureTime) > 2f))
		{
			progressDisplayed = false;
			
			if(gestureInfo != null)
			{
				gestureInfo.text = String.Empty;
			}

			Debug.Log("Forced gesture progress to end.");
		}
	}
	
}
