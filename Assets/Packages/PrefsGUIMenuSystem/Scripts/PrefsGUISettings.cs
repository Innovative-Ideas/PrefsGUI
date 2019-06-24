using UnityEngine;
using System.Collections;
using PrefsGUI;

namespace PrefsGUI
{

	public class PrefsGUISettings : PrefsGUI.MenuBase //MonoBehaviour
	{
		[Tooltip("Default File Save/Load location")]
		public Prefs.FileLocation fileLocation = Prefs.FileLocation.PersistantData;
		public string pathPrefix = "";
		private Prefs.FileLocation fileLastLoadedFrom = Prefs.FileLocation.PersistantData;

		protected override float MinWidth => 700;

		//[System.Serializable]
		//public class PrefsEnum : PrefsParam<Prefs.FileLocation>
		//{
		//	public PrefsEnum( string key, Prefs.FileLocation defaultValue = default( Prefs.FileLocation ) ) : base( key, defaultValue ) { }
		//}
		//public PrefsEnum _prefsEnum;


		void Awake()
		{
			if(this.isActiveAndEnabled == false)
				return;
			PrefsGUI.Prefs.SetFileLocation(fileLocation);
			PrefsGUI.Prefs.SetFilePathPrefix(pathPrefix);

			/*
			// work in progress
		//#if (UNITY_EDITOR == false)
			// if file location is streaming assets, change it to persistant data
			if (fileLocation == Prefs.FileLocation.StreamingAssets)
			{
				System.DateTime defaultFileDT = PrefsGUI.Prefs.GetFileTimeStamp();

				// if fileLocationno persistant data file exists, copy latest streaming assets file to persistant data
				// if persistant data file is older than streaming assets file, copy latest streaming assets file to persistant data
			}
		//#endif
		*/

			// reload file after changing path location
			PrefsGUILoad();

			//_prefsEnum = new PrefsEnum("Save_Load_Location", fileLocation);
		}

		//public void OnGUIInternal()
		public override void OnGUIInternal()
		{
			// pring defaut save location

			GUILayout.Label(string.Format("Default Save/Load Location:\t{0}", PrefsGUI.Prefs.GetFileLocation()), GUILayout.MinWidth(200f));
			GUILayout.Label("\t(To change 'default Location', change in Unity Editor)"
				#if (UNITY_EDITOR == false)
				+ " and rebuild .exe"
				#endif
				);
			
			GUILayout.Label("");
			GUILayout.Label(string.Format("File Last Loaded From:\t{0}", fileLastLoadedFrom));
			GUILayout.Label("");

			ShowAllSaveLocationPathsGUI();

			// save to Streaming Assets
			if (GUILayout.Button("SAVE in Streaming Assets & ALL LOCATIONS"))
			{
				PrefsGUI.Prefs.SaveInAllLocations();
				//PrefsGUI.Prefs.Save();
			}

			// offer options to load settings files from other locations than the default location
			for (Prefs.FileLocation i = 0; i < Prefs.FileLocation.NumLocations; ++i)
			{
				if (i != fileLocation)
				{ 
					// provide option to load from stream assets
					// load from Streaming Assets
					if (GUILayout.Button(string.Format("LOAD from {0}", i) ))
					{
						PrefsGUI.Prefs.SetFileLocation(i);
						PrefsGUILoad();
						PrefsGUI.Prefs.SetFileLocation(fileLocation);
					}
				}
			}

			GUILayout.Label("Settings File Timestamps:");
			for (Prefs.FileLocation i = 0; i < Prefs.FileLocation.NumLocations; ++i)
			{
				PrefsGUI.Prefs.SetFileLocation(i);
				System.DateTime dt = PrefsGUI.Prefs.GetFileTimeStamp();
				bool validSettingFile = dt.Equals(System.DateTime.MinValue) == false;

				GUILayout.Label(string.Format("{0}:\t {1}", i, validSettingFile ? dt.ToString() : "No File / Unknown"));
			}
			PrefsGUI.Prefs.SetFileLocation(fileLocation);


			base.OnGUIInternal();
		}

		void ShowAllSaveLocationPathsGUI()
		{
			GUILayout.Label("File Paths:");
			PrefsGUI.Prefs.FileLocation backup = PrefsGUI.Prefs.GetFileLocation();
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical();

			for(int i = 0; i < (int)PrefsGUI.Prefs.FileLocation.NumLocations; ++i)
			{
				GUILayout.Label( ((PrefsGUI.Prefs.FileLocation)i).ToString() );
			}
			GUILayout.EndVertical();

			GUILayout.BeginVertical();
			for(int i = 0; i < (int)PrefsGUI.Prefs.FileLocation.NumLocations; ++i)
			{
				PrefsGUI.Prefs.SetFileLocation((PrefsGUI.Prefs.FileLocation)i);
				GUILayout.Label( PrefsGUI.Prefs.GetFileNameAndPath() );
			}
			GUILayout.EndVertical();

			GUILayout.EndHorizontal();

			// restore origional location
			PrefsGUI.Prefs.SetFileLocation(backup);
		}


		private void PrefsGUILoad()
		{
			PrefsGUI.Prefs.Load();
			fileLastLoadedFrom = PrefsGUI.Prefs.GetFileLocation();
		}
	} // end class
} // end namespace