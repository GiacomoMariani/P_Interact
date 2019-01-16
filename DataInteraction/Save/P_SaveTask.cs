using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// this task is used to manage a save
    /// </summary>
    public class P_SaveTask : P_SaveableTask
    {
        //the task name
        public override string TaskName { get { return "PlayfabSave_" + _taskId; } }
        //if we want these to be private or public
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] private bool _isPrivate = true;
        //the dictionary of data
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector]
        protected Dictionary<string, string> _dataContainer = new Dictionary<string, string>();

        #region SETUP - TASK
        //this is used to setup the array to save
        internal void SetupSaveTask(bool isPrivate, P_Saveable[] elementsToSave)
        {
            _isPrivate = isPrivate;
            _taskData  = elementsToSave;
        }
        #endregion

        protected override void SendGroup()
        {
            //clear the data transporter
            _dataContainer.Clear();
            //get the items from the queue
            for (int i = 0; i < _taskData.Length; i++)
                AddElementToTransporter(_taskData[i]);

            //subscribe to the load result of the data transfer
            _dataTransfer.SubscribeToSave(Handle_SaveData);
            //send the data to playfab using the transfer controls
            _dataTransfer.SaveDataToPlayfab(_dataContainer, _isPrivate);
        }


        //saving a single element, it takes the json and identifier and send it to save
        private void AddElementToTransporter(P_Saveable saveable)
        {
            //ignore nulls
            if (saveable == null) return;
            //get the key to save
            var keyValue = saveable.GetDataIdentifier();
            //get the json from the savable
            var jsonToSave = saveable.SaveThisIntoJson();
            //add this onto the dictionary
            Assert.IsFalse(_dataContainer.ContainsKey(keyValue),
                           $"We already have a key {keyValue} on {TaskName}. Coming from {saveable.name}");
            _dataContainer.Add(keyValue, jsonToSave);
        }

        #region SAVE RESULTS
        private void Handle_SaveData(Dictionary<string, string> dataSaved)
        {
            //unsubscribe to the event to avoid multiple calls
            _dataTransfer.UnSubscribeToSave(Handle_SaveData);

            //check and confirm all saveables
            foreach (var item in dataSaved) ConfirmItemSaved(item);

            //check all items
            CheckElements();
            //complete
            CompleteTask();
        }

        //check the element inside our request and then confirm the save
        private void ConfirmItemSaved(KeyValuePair<string, string> itemSaved)
        {
            // 1 - Retrieved the saveable from the item key and make sure it exists
            var saveableToSend =
                _taskData.SingleOrDefault(saveable => saveable.GetDataIdentifier() == itemSaved.Key);

            // 1b - Safe check, this should never happen
            if (saveableToSend == null)
            {
                PConsole.Warning($"The saveable (key {itemSaved.Key} - value {itemSaved.Value}) returned from the save command is not in the list of {TaskName}.\nAborting Load.",
                                 TaskName);
                return;
            }

            //2 - send the retrieved data to the saveable
            saveableToSend.SaveConfirmed(itemSaved.Value);
        }

        private void CheckElements()
        {
            //if any of the save elements are still scheduled to save sends the error
            foreach (var saveable in _taskData)
            {
                if (!saveable.SaveRequested) continue;
                PConsole.Warning($"The item {saveable.name} has not been successfully load", TaskName);
                saveable.SaveError();
            }
        }
        #endregion
    }
}
