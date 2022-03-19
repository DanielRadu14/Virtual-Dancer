using UnityEngine;
using System.Collections;

public class LocateAvatarsAndGestureListeners : MonoBehaviour 
{

	void Start () 
	{
		KinectManager kinectManager = KinectManager.Instance;
		
		if(kinectManager)
		{
			// remove all users, filters and avatar controllers
			kinectManager.avatarControllers.Clear();
			kinectManager.ClearKinectUsers();

			// get the mono scripts. avatar controllers and gesture listeners are among them
			MonoBehaviour[] monoScripts = FindObjectsOfType(typeof(MonoBehaviour)) as MonoBehaviour[];
			
			// locate the available avatar controllers
			foreach(MonoBehaviour monoScript in monoScripts)
			{
//				if(typeof(AvatarController).IsAssignableFrom(monoScript.GetType()) &&
//				   monoScript.enabled)
				if((monoScript is AvatarController) && monoScript.enabled)
				{
					AvatarController avatar = (AvatarController)monoScript;
					kinectManager.avatarControllers.Add(avatar);
				}
			}

			// locate Kinect gesture manager, if any
			kinectManager.gestureManager = null;
			foreach(MonoBehaviour monoScript in monoScripts)
			{
//				if(typeof(KinectGestures).IsAssignableFrom(monoScript.GetType()) && 
//				   monoScript.enabled)
				if((monoScript is KinectGestures) && monoScript.enabled)
				{
					kinectManager.gestureManager = (KinectGestures)monoScript;
					break;
				}
			}

			// locate the available gesture listeners
			kinectManager.gestureListeners.Clear();

			foreach(MonoBehaviour monoScript in monoScripts)
			{
//				if(typeof(KinectGestures.GestureListenerInterface).IsAssignableFrom(monoScript.GetType()) &&
//				   monoScript.enabled)
				if((monoScript is KinectGestures.GestureListenerInterface) && monoScript.enabled)
				{
					//KinectGestures.GestureListenerInterface gl = (KinectGestures.GestureListenerInterface)monoScript;
					kinectManager.gestureListeners.Add(monoScript);
				}
			}

			// check for gesture manager
			if (kinectManager.gestureListeners.Count > 0 && kinectManager.gestureManager == null) 
			{
				Debug.Log("Found " + kinectManager.gestureListeners.Count + " gesture listener(s), but no gesture manager in the scene. Adding KinectGestures-component...");
				kinectManager.gestureManager = kinectManager.gameObject.AddComponent<KinectGestures>();
			}

		}
	}
	
}
