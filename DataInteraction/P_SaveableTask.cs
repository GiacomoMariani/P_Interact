using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// a base save task
    /// </summary>
    public abstract class P_SaveableTask : iTask
    {
        #region TASK IMPLEMENTATION
        [BoxGroup("Task", true, true, -25), ShowInInspector, ReadOnly] public abstract string TaskName { get; }
        public event JAction OnComplete;
        public JAction ThisTask { get { return ProcessTask; } }
        public bool IsRunning { get; private set; }
        [BoxGroup("Task", true, true, -25), ShowInInspector, ReadOnly] protected int _taskId;
        #endregion

        #region FIELDS AND PROPERTIS
        //the list of elements to be saved, we can have different groups by instantiating different scriptable objects
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector]
        protected P_Saveable[] _taskData = new P_Saveable[P_Constants.MaxRequestAmount];

        //the transfer control that will send the data to playfab
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector]
        protected P_PlayfabDataTransfer _dataTransfer;
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] public int CurrentData { get { return _taskData.Length; } }
        #endregion

        #region CONSTRUCTOR AND SETUP
        protected P_SaveableTask() { IsRunning = false; }

        public void InjectData(P_PlayfabDataTransfer dataTransfer, int taskId)
        {
            _dataTransfer = dataTransfer;
            _taskId       = taskId;
        }
        #endregion

        #region SAVE PROCESSING - TASK IMPLEMENTATION
        private void ProcessTask()
        {
            IsRunning = true;
            SanityCheck();
            SendGroup();
        }

        private void SanityCheck()
        {
            //make sure we do not have too many elements
            Assert.IsTrue(_taskData.Length <= P_Constants.MaxRequestAmount,
                          $"{TaskName} has {_taskData.Length} elements. Max is {P_Constants.MaxRequestAmount}");
        }
        #endregion

        #region TASK COMMANDS
        protected abstract void SendGroup();

        protected void CompleteTask()
        {
            //send the complete event and reset
            if (OnComplete != null) OnComplete();
            IsRunning = false;
            _taskData = null;
        }
        #endregion
    }
}
