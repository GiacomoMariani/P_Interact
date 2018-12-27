using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// the task used to load the data
    /// </summary>
    public class P_LoadTask : P_SaveableTask
    {
        public override string TaskName { get { return "PlayfabLoad_" + _taskId; } }

        //the list of elements we want to load
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] private List<string> _desiredKeys = new List<string>();
        [BoxGroup("State", true, true, 5), ShowInInspector, ReadOnly] private string _playfabIdToLoad = "";


        #region LOAD PROCESSING - TASK IMPLEMENTATION
        public void SetupLoadTask(string idToLoad, P_Saveable[] elementsToSave)
        {
            _playfabIdToLoad = idToLoad;
            _taskData        = elementsToSave;
        }

        protected override void SendGroup()
        {
            //clear the data transporter
            _desiredKeys.Clear();
            //add an amount of elements to the queue to save
            for (int i = 0; i < _taskData.Length; i++)
                AddSaveable(_taskData[i]);

            //subscribe to the load result of the data transfer
            _dataTransfer.SubscribeToLoad(Handle_LoadData);
            //send the data to playfab using the transfer controls
            _dataTransfer.LoadDataFromPlayfab(_desiredKeys, _playfabIdToLoad);
        }


        //used to add the desired key to the request
        private void AddSaveable(P_Saveable saveable)
        {
            //add the key and register the load on the saveable 
            _desiredKeys.Add(saveable.GetDataIdentifier());
            saveable.RequestLoad();
        }

        //this method is used to get the loaded data and handle them
        private void Handle_LoadData(Dictionary<string, string> dataLoaded, Dictionary<string, DateTime> timeOfUpdate)
        {
            //unsubscribe to avoid multiple callbacks
            _dataTransfer.UnSubscribeToLoad(Handle_LoadData);
            //running through all elements and connecting them to the related saveable
            foreach (var item in dataLoaded) { LoadSingleItem(item, timeOfUpdate[item.Key]); }

            //make sure all item has been loaded if there's nothing more in queue
            CheckLoadElements();
            //send the event
            CompleteTask();
        }

        //load a single item, connecting it to the saveable
        private void LoadSingleItem(KeyValuePair<string, string> itemToLoad, DateTime dateTime)
        {
            // 1 -Retrieved the saveable from the item key and make sure it exists
            var saveableToLoad = _taskData.SingleOrDefault(saveable => saveable.GetDataIdentifier() == itemToLoad.Key);

            // 2 - Safe check, this should never happen
            if (saveableToLoad == null)
            {
                HelperConsole.DisplayWarning(string.Format
                                                 ("The saveable (key {0} - value {1}) returned from the load command is not in the list of {2}.\nAborting Load.",
                                                  itemToLoad.Key, itemToLoad.Value, TaskName));
                return;
            }

            // 3 - make sure the item has not been loaded already
            Assert.IsTrue(saveableToLoad.LoadRequested, string.Format
                              ("The loader group {0} is trying to load {1}, that has not required to be loaded", TaskName,
                               saveableToLoad.name));

            //4 - send the retrieved data to the saveable

            //message log
            HelperConsole.DisplayMessage(string.Format("{0} - <color=#FF0000>{1}</color>\n{2}", "Element_Load_Tag", itemToLoad.Key,
                                                       itemToLoad.Value));

            saveableToLoad.LoadConfirmed(itemToLoad.Value, dateTime);
        }

        /// <summary>
        /// used to check if all saveable have been successfully loaded. Some element could be missing, maybe because this is
        /// the first time the player logged and has no loaded data, or because we changed a key
        /// </summary>
        private void CheckLoadElements()
        {
            //if any of the save elements are still scheduled to save return false
            foreach (var saveable in _taskData)
            {
                if (saveable.LoadRequested)
                {
                    HelperConsole.DisplayWarning(string.Format("The item {0} has not been successfully load on {1}",
                                                               saveable.name, TaskName));
                    saveable.LoadError();
                }
            }
        }
        #endregion
    }
}
