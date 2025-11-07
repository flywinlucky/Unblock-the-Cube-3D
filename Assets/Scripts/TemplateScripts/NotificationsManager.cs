using UnityEngine;
using System.Collections;
using System;
#if UNITY_IOS
using NotificationServices = UnityEngine.iOS.NotificationServices;
using NotificationType = UnityEngine.iOS.NotificationType;
using LocalNotification = UnityEngine.iOS.LocalNotification;
#endif
/*Scene:N/A
 *Object:NottificationsManager
 *Opis:Klasa koja sadrzi funkcije potrebne za setovanje i uklanjanje lokalnih notifikacija.
 *Napomena: na IOS-u je potrebno pre setovanja lokalne notifikacije( prvi put ) OBAVEZNO registorvati aplikaciju za lokalne notifikacije.
 *
 *
 */
public class NotificationsManager : MonoBehaviour {

	void Awake()
	{
		// Setujemo notifikacije
		string[] texts = {"New world cuisines are waiting. Hurry up!", "The words are impatient to be discovered!", "Pick up the pace and reveal new dishes with new words!"};

		if (!PlayerPrefs.HasKey("NotificationsSet"))
		{
			PlayerPrefs.SetInt("NotificationsSet", 1);
			PlayerPrefs.Save();
			SetNottification(172800, "Word Chef: Word Search Puzzle", texts[UnityEngine.Random.Range(0, texts.Length)], 1554400);
//			SetNottification(345600, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554401);
//			SetNottification(518400, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554402);
//			SetNottification(691200, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554403);
		}
		else
		{
			CancelNottificationWithID(1554400);
//			CancelNottificationWithID(1554401);
//			CancelNottificationWithID(1554402);
//			CancelNottificationWithID(1554403);

			SetNottification(172800, "Word Chef: Word Search Puzzle", texts[UnityEngine.Random.Range(0, texts.Length)], 1554400);
//			SetNottification(345600, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554401);
//			SetNottification(518400, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554402);
//			SetNottification(691200, "Red Hand Slap-2 Player Reaction Game", texts[UnityEngine.Random.Range(0, texts.Length)], 1554403);
		}
	}

	/// <summary>
	/// Setuje lokalnu notifikaciju
	/// </summary>
	/// <param name="timeOffset">Vreme u sekundama od tekuceg vremena kada treba prikazati notifikaciju</param>
	/// <param name="title">Naslov notifikacije</param>
	/// <param name="message">Telo (poruka) notifkacije</param>
	/// <param name="id">ID notifikacije.Za IOS ovo predstavlja redni broj na badge-u.</param>
	public void SetNottification(int timeOffset,string title,string message,int id)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
		{
			using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) 
			{	
				obj_Activity.Call("SendNotification",timeOffset.ToString(),title,message,id);
			}
		}
#endif

#if UNITY_IOS && !UNITY_EDITOR
		LocalNotification notification = new LocalNotification ();
		notification.fireDate = System.DateTime.Now.AddSeconds(timeOffset);
		notification.alertAction = title;
		notification.alertBody = message;
		notification.hasAction = false;
		notification.applicationIconBadgeNumber=id;
		NotificationServices.ScheduleLocalNotification (notification);
#endif
	}
	/// <summary>
	/// Uklanja sve setovane  lokalne notifikacije (Samo IOS)
	/// </summary>
	public void CancelAllNotifications()
	{
#if UNITY_IOS && !UNITY_EDITOR

		//Empty notification to clear badge number
		LocalNotification l = new LocalNotification ();
		l.applicationIconBadgeNumber = -1;
		NotificationServices.PresentLocalNotificationNow (l);
		NotificationServices.CancelAllLocalNotifications();
		NotificationServices.ClearLocalNotifications();
		NotificationServices.ClearLocalNotifications ();
#endif
	
	}
	/// <summary>
	/// Uklanja setovanu lokalnu notifikaciju sa odgovarajucim ID-jem. (samo Android)
	/// </summary>
	/// <returns><c>true</c> if this instance cancel nottification with I the specified id; otherwise, <c>false</c>.</returns>
	/// <param name="id">Identifier.</param>
	public void CancelNottificationWithID(int id)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer")) 
		{
			using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity")) 
			{
				obj_Activity.Call("CancelNotification",id);

			}
		}
#endif

	}
	/// <summary>
	/// Registruje aplikaciju za setovanje i primanje lokalnih notifikacija na IOS-u.
	/// </summary>
	public void RegisterForLocalNottifications()
	{
#if UNITY_IOS && !UNITY_EDITOR
		//za tip notifikacije promeniti argument funkcije
		//ovo je default poziv
		NotificationServices.RegisterForNotifications(NotificationType.Alert | NotificationType.Badge | NotificationType.Sound);

#endif
	}
}
