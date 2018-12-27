using JReact;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// a saveable is something that can be saved and loaded from playfab
    /// </summary>
    public abstract class P_Saveable : ScriptableObject, iResettable
    {
        #region FIELDS AND PROPERTIES
        //the possible condition to cancel save commands, they have to be true to schedule a save
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required]
        private JReactiveBool[] _saveConditions;

        //the data identifier

        // --------------- SAVE --------------- //
        //all the group savers that have a reference to this element, we need this to schedule a save
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector]
        private P_SaveGroupQueue _saveQueue;
        //save elements
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _saveRequested = false;
        public bool SaveRequested { get { return _saveRequested; } private set { _saveRequested = value; } }
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _saveFailed = false;
        public bool SaveFailed { get { return _saveFailed; } private set { _saveFailed = value; } }

        // --------------- LOAD --------------- //
        //load elements
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _loadRequested = false;
        public bool LoadRequested { get { return _loadRequested; } private set { _loadRequested = value; } }
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _loadFailed = false;
        public bool LoadFailed { get { return _loadFailed; } private set { _loadFailed = value; } }

        //a general bool to check if this item can be manipulated
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] public bool SaveableReady
        {
            get { return !SaveRequested && !LoadRequested; }
        }

        //debug elements
        [BoxGroup("Debug", true, true, 100), SerializeField] protected bool _debug;
        #endregion

        #region ASBTRACT IMPLEMENTATION
        //used to get the identifier we want to set on this saveable
        public abstract string GetDataIdentifier();

        //this method will implement a way to save this into a json
        public abstract string SaveThisIntoJson();
        
        //the main method to handle the retrieved data
        protected abstract void LoadThisFromJson(string jsonData, DateTime dateTime);
        #endregion

        #region SAVE ELEMENTS
        //the storage that will handle the save system with playfab
        internal void RegisterSaveGroup(P_SaveGroupQueue saveQueue)
        {
            if (_saveQueue != null)
            {
                P_PlayfabConsoleLogger.DisplayWarning(string.Format("Overriding {0} with {1}", _saveQueue.name, saveQueue.name), name);
            }

            _saveQueue = saveQueue;
            InitiateSaveCommandWith(saveQueue);
        }

        //sets up saveable logi
        protected abstract void InitiateSaveCommandWith(P_SaveGroupQueue saveSystem);

        //checks if we can save this, true as default
        protected virtual bool CanSave() { return true; }

        //requests to schedule a save, usually triggeres when this saveable has changed
        public void RequestSaveSchedule()
        {
            //STEP 1 - check if all save conditions are met
            if (!CheckAllConditions() ||
                !CanSave()) return;


            //STEP 2 - make sure everything is ready
            SanityChecks();

            // --------------- CONFIRM THE SAVE --------------- //
            P_PlayfabConsoleLogger.DisplayMessage(string.Format("Requested to schedule a save on {0}", _saveQueue.name), name);
            _saveQueue.ScheduleThis(this);
        }

        private bool CheckAllConditions()
        {
            //STEP 1 - to avoid multiple save
            if (SaveRequested)
            {
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Save canceled. This is saving already"), name);
                return false;
            }

            //STEP 2 - check all other conditions
            //check all the conditions and return false if anyone of them is not met
            for (int i = 0; i < _saveConditions.Length; i++)
            {
                Assert.IsNotNull(_saveConditions[i], string.Format("The save condition at index {0} of {1} is null", name, i));
                if (!_saveConditions[i].CurrentValue)
                {
                    if (_debug)
                        P_PlayfabConsoleLogger.DisplayMessage(string.Format("Save canceled. Condition not met: {0}",
                                                                            _saveConditions[i].name), name);
                    return false;
                }

                //show the condition as confirmed
                if (_debug)
                    P_PlayfabConsoleLogger.DisplayMessage(string.Format("Confirmed condition: {0}", _saveConditions[i].name), name);
            }

            //confirm the conditions if all of them are met
            return true;
        }

        private void SanityChecks()
        {
            Assert.IsNotNull(_saveQueue, string.Format("{0} has no save queue", name));
            Assert.IsFalse(LoadRequested, string.Format("This saveable is currently loading, we cannot save it: {0}", name));
        }

        internal void SaveConfirmed(string itemSavedValue)
        {
            SaveRequested = false;
            SaveFailed    = false;
            if (_debug)
                HelperConsole.DisplayMessage(string.Format("{0} - {1} Saved data: {2}",
                                                           P_Constants.DEBUG_PlayfabInteract, name, itemSavedValue));
        }

        public virtual void SaveError()
        {
            SaveRequested = false;
            SaveFailed    = true;
        }
        #endregion

        #region LOAD ELEMENTS
        //used to register a load group, the load group is not required
        protected internal virtual void RegisterLoadGroup(P_LoadGroupQueue loadGroup) { }

        /// <summary>
        /// this is used to register a load request
        /// </summary>
        internal void RequestLoad() { LoadRequested = true; }

        /// this is used to handle a load fail
        internal void LoadError()
        {
            LoadRequested = false;
            LoadFailed    = true;
        }

        /// <summary>
        /// this method will be used to load a specific saveable
        /// </summary>
        /// <param name="jsonData">the data retrieved</param>
        /// <param name="dateTime">the time of the last update</param>
        /// <returns></returns>
        internal void LoadConfirmed(string jsonData, DateTime dateTime)
        {
            //reset the load request
            LoadRequested = false;
            LoadFailed    = false;
            //send a debug message if requested
            if (_debug)
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Item has been loaded from {0}:\n{1}",
                                                                    dateTime.ToString(CultureInfo.InvariantCulture), jsonData), name);
            LoadThisFromJson(jsonData, dateTime);
        }
        #endregion

        #region DISABLE AND RESET
        //we reset this on disable
        protected virtual void OnDisable() { ResetThis(); }

        public virtual void ResetThis()
        {
            HelperConsole.DisplayMessage(string.Format("{0} is resetting", name), "-SAVEABLE- ");
            //reset all data changer
            _saveQueue = null;
            //reset the saveable state
            _loadRequested = false;
            _loadFailed    = false;
            _saveRequested = false;
            _saveFailed    = false;
        }
        #endregion
    }
    
    [System.Serializable]
    public class SaveableData
    {
    }
}
