using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

namespace PrefsWrapperJson
{
    public class JSONData
    {
        public static JSONData Instance = new JSONData();

        Dictionary<string, object> _dic = new Dictionary<string, object>();

        public JSONData() { Load(); }

        public bool HasKey(string key) { return _dic.ContainsKey(key); }
        public void DeleteKey(string key) { _dic.Remove(key); }

        public object Get(string key) { return _dic[key]; }

        public void Set(string key, object value) { _dic[key] = value; }

        string path { get { return pathPrefix + "/Prefs.json"; } }

        public void Save()
        {
			string tempPath = path;

			// make sure the directory exists for the file path. 
			// (If it doesn't exist we will create it)
			string directory = Path.GetDirectoryName(tempPath);
			if(Directory.Exists(directory) == false)
				Directory.CreateDirectory(directory);

			Debug.Log(@tempPath);
            var str = MiniJSON.Json.Serialize(_dic);
            File.WriteAllText(@tempPath, str);
        }

        public void Load()
        {
            if (File.Exists(path))
            {
                var str = File.ReadAllText(path);
                _dic = (Dictionary<string, object>)MiniJSON.Json.Deserialize(str);
            }
        }

        public void DeleteAll()
        {
            _dic.Clear();
        }

		// Returns DateTime.MinValue if file doens't exist
		public DateTime GetFileTimeStamp()
		{
			if (File.Exists(path))
			{
				DateTime dt = File.GetLastWriteTime(path);
				return dt;
			}
			return DateTime.MinValue;
		}

		private PrefsGUI.Prefs.FileLocation _fileLocation = PrefsGUI.Prefs.FileLocation.PersistantData;
		private string _PathPrefix = "";
		public void SetFileLocation(PrefsGUI.Prefs.FileLocation fileLocation) {	_fileLocation = fileLocation; }
		public PrefsGUI.Prefs.FileLocation GetFileLocation() { return _fileLocation; }
		public void SetFilePathPrefix(string prefix)
		{
			_PathPrefix = prefix;
		}
		private string pathPrefix
		{
			get
			{
				string ret = "";
				switch(_fileLocation)
				{
					case PrefsGUI.Prefs.FileLocation.PersistantData: ret = Application.persistentDataPath; break;
					case PrefsGUI.Prefs.FileLocation.StreamingAssets: ret = Application.streamingAssetsPath; break;
				}
				ret += _PathPrefix;
				return ret;
			}
		}
    }

    class PlayerPrefsGlobal
    {
        public static void Save() { JSONData.Instance.Save(); }
        public static void Load() { JSONData.Instance.Load(); }
        public static void DeleteAll() { JSONData.Instance.DeleteAll(); }
		public static void SetFileLocation(PrefsGUI.Prefs.FileLocation fileLocation) { JSONData.Instance.SetFileLocation(fileLocation); }
		public static PrefsGUI.Prefs.FileLocation GetFileLocation() { return JSONData.Instance.GetFileLocation(); }
		public static void SetFilePathPrefix(string prefix) { JSONData.Instance.SetFilePathPrefix(prefix); }
		public static DateTime GetFileTimeStamp() { return JSONData.Instance.GetFileTimeStamp();  }

	}


    class PlayerPrefsStrandard<T>
    {
        static Type type
        {
            get
            {
                return (typeof(T) == typeof(bool) || typeof(T).IsEnum)
                    ? typeof(int)
                    : typeof(T);
            }
        }

        public static bool HasKey(string key)
        {
            return JSONData.Instance.HasKey(key);
        }

        public static void DeleteKey(string key)
        {
            JSONData.Instance.DeleteKey(key);
        }

        public static object Get(string key, object defaultValue)
        {
            if (!HasKey(key)) Set(key, defaultValue);

            var ret = JSONData.Instance.Get(key);
            return (typeof(T).IsEnum ? (T)Enum.Parse(typeof(T), ret.ToString()) : Convert.ChangeType(ret, typeof(T)));
        }

        public static void Set(string key, object val)
        {
            JSONData.Instance.Set(key, Convert.ChangeType(val, type));
        }
    }

}
