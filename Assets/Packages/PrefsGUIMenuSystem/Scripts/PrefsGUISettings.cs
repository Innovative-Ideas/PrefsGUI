using UnityEngine;
using System.Collections;
using PrefsGUI;

public class PrefsGUISettings : ObjectsInTea.MenuBase //MonoBehaviour
{
	public  Prefs.FileLocation fileLocation  = Prefs.FileLocation.PersistantData;
	public string pathPrefix = "";

	//[System.Serializable]
	//public class PrefsEnum : PrefsParam<Prefs.FileLocation>
	//{
	//	public PrefsEnum( string key, Prefs.FileLocation defaultValue = default( Prefs.FileLocation ) ) : base( key, defaultValue ) { }
	//}
	//public PrefsEnum _prefsEnum;


	void Awake()
	{
		PrefsGUI.Prefs.SetFileLocation(fileLocation);
		PrefsGUI.Prefs.SetFilePathPrefix(pathPrefix);
		// reload file after changing path location
		PrefsGUI.Prefs.Load();

		//_prefsEnum = new PrefsEnum("Save_Load_Location", fileLocation);
	}

	//public void OnGUIInternal()
	public override void OnGUIInternal()
	{


		//if(Prefs.GetFileLocation() != Prefs.FileLocation.NumLocations)
		//{ 
		//	var previous = _prefsEnum.Get();
		//	_prefsEnum.OnGUI("Save/Load Location:");
		//	// don't let user change values to Prefs.FileLocation.NumLocations
		//	if(_prefsEnum.Get() == Prefs.FileLocation.NumLocations )
		//		_prefsEnum.Set(previous);

		//	if ( _prefsEnum.Get() != fileLocation )
		//	{
		//		if (GUILayout.Button( "Load Settings File from New Location?"))
		//		{
		//			fileLocation = _prefsEnum.Get();
		//			PrefsGUI.Prefs.SetFileLocation(fileLocation);
		//			PrefsGUI.Prefs.Load();
		//		}
		//	}
		//}
		// pring defaut save location
		GUILayout.Label(string.Format("Default Save/Load Location: {0}", PrefsGUI.Prefs.GetFileLocation() ));

		// save to Streaming Assets
		if (GUILayout.Button( "SAVE in Streaming Assets & ALL LOCATIONS"))
		{
			PrefsGUI.Prefs.SaveInAllLocations();
			//PrefsGUI.Prefs.Save();
		}
		// load from Streaming Assets
		if (GUILayout.Button( "LOAD from Streaming Assets"))
		{
			PrefsGUI.Prefs.SetFileLocation( Prefs.FileLocation.StreamingAssets );
			PrefsGUI.Prefs.Load();
			PrefsGUI.Prefs.SetFileLocation( fileLocation );
		}

		base.OnGUIInternal();
	}

}
