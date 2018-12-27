using System;
using System.Collections.Generic;
using PlayFab.ClientModels;
using PlayFab;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// used to send and get info from playfab
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Data/Playfab Data Transfer")]
    public class P_PlayfabDataTransfer : ScriptableObject
    {
        #region VALUES - DATA
        //Event to handle information when getdata was succesfull
        public delegate void PlayfabDataSaveEvent(Dictionary<string, string> dataMoved);
        public event PlayfabDataSaveEvent OnPlayfabDataSave;
        public delegate void PlayfabDataLoadEvent(Dictionary<string, string> dataMoved,
                                                  Dictionary<string, DateTime> dataTime);
        public event PlayfabDataLoadEvent OnPlayfabDataLoad;

        //the checker to make sure we do not save when we're not connected
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required]
        private P_ConnectorSafeChecks _connectionChecks;

        //to start tracking the collection of rewards
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _isBusy = false;
        public bool IsBusy { get { return _isBusy; } private set { _isBusy = value; } }

        //a dictionary to store the current element
        [BoxGroup("Data", true, true, 10), ReadOnly, ShowInInspector]
        private Dictionary<string, string> _savedData = new Dictionary<string, string>();
        [BoxGroup("Data", true, true, 10), ReadOnly, ShowInInspector]
        private Dictionary<string, string> _loadedData = new Dictionary<string, string>();
        [BoxGroup("Data", true, true, 10), ReadOnly, ShowInInspector]
        private Dictionary<string, DateTime> _loadedTime = new Dictionary<string, DateTime>();

        //if we want some debug to test
        [BoxGroup("Debug", true, true, 100), SerializeField] private bool _debugMode = false;
        #endregion

        #region DATA SAVING
        /// <summary>
        /// This is used store multiple elements at the same time. We will use a dictionary
        /// with all the desired values to save
        /// </summary>
        /// <param name="dataToStore">the data we want to store</param>
        /// <returns> returns true if the data are sent succesfully</returns>
        public bool SaveDataToPlayfab(Dictionary<string, string> dataToStore, bool privatePermission = true)
        {
            // --------------- CHECKS --------------- //
            if (!TransferReady()) return false;
            IsBusy = true;
            //double check that the list contains only what we need
            Assert.IsFalse(dataToStore.Keys.GroupBy(n => n).Any(c => c.Count() > 1), "The given list has duplicates.");

            // --------------- CREATING THE REQUEST --------------- //
            var request = new UpdateUserDataRequest() { Data = dataToStore };
            //set the permission
            request.Permission = privatePermission
                                     ? UserDataPermission.Private
                                     : UserDataPermission.Public;
            //store the data in our dictionary for the final event, triggered in the save handle method
            _savedData = dataToStore;
            //send the debug if requested
            if (_debugMode)
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Sent Data to Playfab:\n{0}",
                                                                    dataToStore.PrintAll()), name);

            // --------------- SEND THE COMMAND --------------- //
            //send the data to playfab
            PlayFabClientAPI.UpdateUserData(request, Save_OnSuccess, Save_OnError);

            //data sent successfully
            return true;
        }

        private void Save_OnSuccess(UpdateUserDataResult result)
        {
            IsBusy = false;
            P_PlayfabConsoleLogger.DisplayMessage("Data has been saved.", name);
            if (OnPlayfabDataSave != null) OnPlayfabDataSave(_savedData);
        }

        private void Save_OnError(PlayFabError error)
        {
            IsBusy = false;
            P_PlayfabConsoleLogger.LogErrorFrom(error, name);
        }
        #endregion

        #region LOAD
        /// <summary>
        /// used to load data from playfab
        /// </summary>
        /// <param name="desiredKeys">the keys we want to load from playfab</param>
        /// <param name="playerPlayfabId">the player id we want to load, if not set we load the current player</param>
        public bool LoadDataFromPlayfab(List<string> desiredKeys, string playerPlayfabId = "")
        {
            //requested the call
            if (_debugMode)
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Load called.\nRequested: {0}",
                                                                    desiredKeys.PrintAll()), name);
            // --------------- CHECKS --------------- //
            //stop if the transfer is not ready
            if (!TransferReady()) return false;
            //set this as busy
            IsBusy = true;

            var request = new GetUserDataRequest() { Keys = desiredKeys, };
            //add the playfab id is requested
            if (playerPlayfabId != "") request.PlayFabId = playerPlayfabId;

            PlayFabClientAPI.GetUserData(request, Load_OnSuccess, Load_OnError);
            return true;
        }

        private void Load_OnSuccess(GetUserDataResult result)
        {
            IsBusy = false;
            //clearing the dictionaries
            _loadedData.Clear();
            _loadedTime.Clear();
            //Set user data variables
            foreach (var item in result.Data)
            {
                //data
                _loadedData[item.Key] = item.Value.Value;
                //time
                _loadedTime[item.Key] = item.Value.LastUpdated;
            }

            P_PlayfabConsoleLogger.DisplayMessage("Data has been loaded. " + result.Data.Keys.PrintAll(), name);
            if (OnPlayfabDataLoad != null) OnPlayfabDataLoad(_loadedData, _loadedTime);
        }

        private void Load_OnError(PlayFabError error)
        {
            IsBusy = false;
            P_PlayfabConsoleLogger.LogErrorFrom(error, name);
        }
        #endregion

        #region CHECKS
        //used to check if we can start the transfer
        private bool TransferReady()
        {
            //1 - stop if we're not connected to playfab
            if (!_connectionChecks.PlayerLoggedIntoPlayfab)
            {
                P_PlayfabConsoleLogger.DisplayWarning("We're trying to send data to playfab, but we're not connected.", name);
                return false;
            }

            //2 - stop if we're busy
            if (IsBusy)
            {
                P_PlayfabConsoleLogger.DisplayWarning
                    ("Something requested to set multiple data when we were still sending. Aborting operation.", name);
                return false;
            }

            //if all check passes we return true
            return true;
        }
        #endregion

        #region SUBRSCRIBE
        //subscribe and unsubscribe to the load event
        public void SubscribeToSave(PlayfabDataSaveEvent actionToSend) { OnPlayfabDataSave += actionToSend; }
        public void UnSubscribeToSave(PlayfabDataSaveEvent actionToSend) { OnPlayfabDataSave -= actionToSend; }

        //subscribe and unsubscribe to the load event
        public void SubscribeToLoad(PlayfabDataLoadEvent actionToSend) { OnPlayfabDataLoad   += actionToSend; }
        public void UnSubscribeToLoad(PlayfabDataLoadEvent actionToSend) { OnPlayfabDataLoad -= actionToSend; }
        #endregion

        #region LISTENERS
        //start and stop tracking on enable
        private void OnEnable() { ResetThis(); }
        private void ResetThis() { IsBusy = false; }
        #endregion
    }
}
