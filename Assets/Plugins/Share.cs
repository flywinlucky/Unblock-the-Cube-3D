using UnityEngine;
using System.Collections;
using System.IO;

/// <summary>
/// Pomocna klasa za manipulaciju galerijom na Aandroid-u
/// </summary>
public class Share : MonoBehaviour {
 
	public static void ShareScreenshot( string destination )
	{
		if (Application.platform == RuntimePlatform.Android)
		{
		 
			AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("parse","file://" + destination);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);
			 intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), "Check out my new design!");
			 intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), "DOLL HOUSE");
			intentObject.Call<AndroidJavaObject>("setType", "image/png");
			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");


			AndroidJavaObject jChooser = intentClass.CallStatic<AndroidJavaObject>("createChooser", intentObject, "SHARE IMAGE USING:");
			currentActivity.Call("startActivity", jChooser);

			 

		}
	}


	public static string ReturnGalleryFolder()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject obj;
			AndroidJavaObject activityContext;
		 
			using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
			}
			 
			using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.webelinx.androidutils.Utils"))
			{
				if(pluginClass != null)
				{
					obj = pluginClass.CallStatic<AndroidJavaObject>("CreateInstance");
					obj.Call("setContext",activityContext);

					var dir  = obj.Call<string>("returnGalleryFolder");
					 return (string) dir;
				}
			}
		}
		return "";
	}

	public static void RefreshGalleryFolder(string path)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject obj;
			AndroidJavaObject activityContext;
			
			using (AndroidJavaClass activityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
			{
				activityContext = activityClass.GetStatic<AndroidJavaObject>("currentActivity");
			}
			
			using (AndroidJavaClass pluginClass = new AndroidJavaClass("com.webelinx.androidutils.Utils"))
			{
				if(pluginClass != null)
				{
					obj = pluginClass.CallStatic<AndroidJavaObject>("CreateInstance");
					obj.Call("setContext",activityContext);


					//obj.Call ("RefreshGallery");
					activityContext.Call("runOnUiThread", new AndroidJavaRunnable(() => {
						obj.Call ("RefreshGallery",path);
					}));
					 
				}
			}
		}
	}

}
