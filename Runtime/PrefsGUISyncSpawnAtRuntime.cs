using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Assertions;
using System.Linq;

#pragma warning disable 0618 // disable Unity warning about UNET being depreciated

namespace PrefsGUI
{
    /* This class does the following:
     *      Spawns PrefsGUISync at runtime instead having it be a scene object.
     * 
     *      In Unity Editor, when application is not playing:
     *      1. manages ignore keys 
     *      1.1 It will copy ignore keys from PrefsGUISync if both are in the same editor scene
     *      1.2 Extra ignore keys can be registered with this class
     *      2. Will add PrefsGUISync Prefab to NetworkManager's list of spawnable prefabs
     * 
     *      In runtime mode:
     *      1 Removes scene version of PrefsGUISync if it exists
     *      2.1 spawns PrefsGUISync Prefab.  
     *      2.2   All ignore keys in this class will be added the spawned prefab.
     * 
     * This class was created because of bugs in Unity's UNET implamentation.
     * On the client, 
     *      A scene game object has PrefsGUISync component and a seperate component that calls DoNotDestroyOnLoad() from it's Awake() function.  
     *      However before Awake() is called, the client loads a new scene and PrefsGUISync game object no longer exists.
     *      Result: Client does not sync any values
     * To fix this problem, PrefsGUISync is instantiated at runtime using this class, PrefsGUISyncSpawnAtRuntime.
     */
    [ExecuteInEditMode]
    public class PrefsGUISyncSpawnAtRuntime : MonoBehaviour
    {

        private List<string> _ignoreKeysCopy = new List<string>();
        public List<string> _ignoreKeysExtra = new List<string>(); // want use HashSet but use List so it will be serialized on Inspector
        public PrefsGUISync prefsGUISyncPrefab = null;
        private PrefsGUISync instance = null;

        private void Awake()
        {
            if ( Application.isPlaying )
                StartRuntime();

        }

        // Use this for initialization
        void Start()
        {

        }

        void StartRuntime()
        {
            DontDestroyOnLoad( this.gameObject );
        }

        void UpdateRuntime()
        {
            if ( Application.isPlaying == false )
                return;
#if UNITY_EDITOR
            if ( UnityEditor.EditorApplication.isPlaying == false )
                return;
#endif

            if ( SyncNet.isServer )
            {
                SpawnPrefsGUISyncServerOrStandalone();
            }
            else if ( SyncNet.isSlave )
            {
                SpawnPrefsGUISyncClientSlave();
            }
        }

        bool removedSceneVersion = false;
        // Note: this function will work on a server or host
        // Note: On a slave client, since the version of PrefsGUISync that is part of the unity scene is disabled initialy, and stays disabled, GameObject.FindObjectsOfType() will never find it.
        void RemoveSceneVersion()
        {
            if ( removedSceneVersion )
                return;

            if ( Time.frameCount < 3000 || Time.frameCount % 60 == 0 )
            {
                PrefsGUISync[] sceneVersionArray = GameObject.FindObjectsOfType<PrefsGUISync>();
                if ( sceneVersionArray == null )
                    return;
                for ( int i = 0; i < sceneVersionArray.Length; ++i )
                {
                    PrefsGUISync sceneVersion = sceneVersionArray[i];
                    if ( sceneVersion != null && sceneVersion.name.Contains( "Clone" ) == false )
                    {
                        Debug.Log( "PrefsGUISyncOffline.SpawnPrefsGUISync() found existing PrefsGUISync. Destroying it so it will be created via runtime network spawn." );
                        this._ignoreKeysCopy.Clear();
                        this._ignoreKeysCopy.AddRange( sceneVersion._ignoreKeys );
                        GameObject.Destroy( sceneVersion.gameObject );
                        removedSceneVersion = true;
                        return; // success!
                    }
                }
            }
        }

        
        void SpawnPrefsGUISyncServerOrStandalone()
        {
            RemoveSceneVersion();

            if ( instance != null )
            {
                // In the current implamentation, this function, SpawnPrefsGUISyncServerOrStandalone(), is called before the 'online scene' has loaded on the server, thus it first creates spawns the prefsGUISyncPrefab in the 'boot' scene
                // Note: On the client, Apprently if the scene changes networkIdentities from the old scene, 'boot scene', that aren't destroyed when that scene is unloaded are disabled instead.
                // This extra call to spawn here makes sure the NetworkIdentity game object is created for the loaded 'online scene' on the client 
                if ( SyncNet.isServer )
                    NetworkServer.Spawn( instance.gameObject );
                return;
            }


            //            else
            {
                Debug.Log( "PrefsGUISyncOffline.SpawnPrefsGUISync() spawning PrefsGUISync prefab" );
                Assert.IsTrue( prefsGUISyncPrefab != null, "PrefsGUISyncOffline.SpawnPrefsGUISync() Must register PrefsGUISync prefab with this game object." );
                instance = Instantiate( prefsGUISyncPrefab );

                AddIgnoreKeysToInstance();

                if ( SyncNet.isServer )
                    NetworkServer.Spawn( instance.gameObject );
            }
        }

        void AddIgnoreKeysToInstance()
        {
            if ( instance == null )
                return;
            instance._ignoreKeys.AddRange( this._ignoreKeysCopy );
            instance._ignoreKeys.AddRange( this._ignoreKeysExtra );
            instance._ignoreKeys = instance._ignoreKeys.Distinct().ToList(); // make sure there are no duplicate keys
        }

        void SpawnPrefsGUISyncClientSlave()
        {
            RemoveSceneVersion();

            if ( instance != null )
                return;


            instance = GameObject.FindObjectOfType<PrefsGUISync>();
            AddIgnoreKeysToInstance();
        }
        // Update is called once per frame
        void Update()
        {
#if UNITY_EDITOR
            UpdateInEditor();
#endif


            UpdateRuntime();
        }

		private void OnDisable()
		{
			Debug.Log( "PrefsGUISyncSpawnAtRuntime.OnDisable()" );
		}

		private void OnDestroy()
		{
            Debug.Log( "PrefsGUISyncSpawnAtRuntime.OnDestroy()" );
        }

		#region Editor
#if UNITY_EDITOR

		private float _interaval = 1f;
        private float _lastTime = 0f;

        virtual protected void UpdateInEditor()
        {
            var time = (float)UnityEditor.EditorApplication.timeSinceStartup;
            if ( time - _lastTime < _interaval )
                return;
            _lastTime = time;

            if ( UnityEditor.EditorApplication.isPlaying )
                return;
            UpdateInEditorNetworkMgr();
        }
        void UpdateInEditorNetworkMgr()
        {
            var networkManager = UnityEditor.SceneAsset.FindObjectOfType<NetworkManager>();
            // register Prefabs with network manager if not already registered
            if ( networkManager == null )
                return;

            AddPrefabToNetworkManager( networkManager, prefsGUISyncPrefab == null ? null : prefsGUISyncPrefab.gameObject );
        }

        protected void AddPrefabToNetworkManager( NetworkManager networkManager, GameObject prefab )
        {
            if ( prefab == null )
                return;
            if ( networkManager.spawnPrefabs.Contains( prefab ) == false )
            {
                // remove null enteries from list
                networkManager.spawnPrefabs.RemoveAll( item => item == null );

                networkManager.spawnPrefabs.Add( prefab );
#if UNITY_EDITOR
                UnityEditor.Undo.RecordObject( networkManager, "undo Add SpawnPrefab" );
                UnityEditor.SceneManagement.EditorSceneManager.MarkAllScenesDirty();
                UnityEditor.EditorUtility.SetDirty( networkManager );
                //UnityEditor.SerializedObject.
#endif
                Debug.LogFormat( "System adds {0} to Network Manager prefab list", prefab.name );

            }
        }

        void UpdateInEditorIgnoreKeyCopy()
        {
            var prefsGUISync = UnityEditor.SceneAsset.FindObjectOfType<PrefsGUISync>();
            if ( prefsGUISync == null )
                return;
            this._ignoreKeysCopy.Clear();
            this._ignoreKeysCopy.AddRange( prefsGUISync._ignoreKeys );

            //for(int i = 0; i < prefsGUISync._ignoreKeys.Count; ++i)
            //{
            //    string key = prefsGUISync._ignoreKeys[i];
            //    if( this._ignoreKeys.Contains(key) == false)
            //        this._ignoreKeys.Add(key);
            //}
        }
#endif
        #endregion // editor
    } // end class
} // end namespace