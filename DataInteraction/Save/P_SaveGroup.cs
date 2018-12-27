using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// a saveable task
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Data/Save Elements Queue")]
    public class P_SaveGroupQueue : P_InteractionTasks<P_SaveTask>
    {
        //if we want these to be private or public
        [BoxGroup("State", true, true, 5), ReadOnly, ShowInInspector] private bool _isPrivate = true;

        #region INITIATION
        // this is used to initiate the save loop. It register all the containing elements, so they can talk to each other
        protected override void SpecificInitialization()
        {
            //register all elements
            for (int i = 0; i < _groupOfElements.Length; i++)
                _groupOfElements[i].RegisterSaveGroup(this);
        }
        #endregion

        #region TASK SETUP
        protected override void SetupCurrentTask(int maxElementsToSend)
        {
            _CurrentTask.SetupSaveTask(_isPrivate, GetArraySegment(maxElementsToSend));
        }
        #endregion

        #region DISABLE RESET
        private void OnDisable() { ResetThis(); }

        public override void ResetThis()
        {
            base.ResetThis();
            _saveableInQueue.Clear();
        }
        #endregion
    }
}
