using UnityEngine;
using System.Collections;
using System.Runtime.InteropServices;

public class WindowsVoice : MonoBehaviour {
  [DllImport("WindowsVoice")]
  public static extern void initSpeech();
  [DllImport("WindowsVoice")]
  public static extern void destroySpeech();
  [DllImport("WindowsVoice")]
  public static extern void addToSpeechQueue( string s);

  public static WindowsVoice theVoice = null;
	// Use this for initialization
	void Start () {
        if (theVoice == null)
        {
          theVoice = this;
          DontDestroyOnLoad(gameObject);
          initSpeech();
        }
        //else
          //Destroy(gameObject);
	}
  public void test()
  {
    speak("Testing");
  }
  public void speak(string msg) {
    addToSpeechQueue(msg);
  }
  void OnDestroy()
  {
    if (theVoice == this)
    {
      Debug.Log("Destroying speech");
      destroySpeech();
      theVoice = null;
    }
  }
}
