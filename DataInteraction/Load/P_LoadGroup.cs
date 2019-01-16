using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// this class is used to load a group of elements
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Data/Load Elements Queue")]
    public class P_LoadGroupQueue : P_InteractionTasks<P_LoadTask>
    {
        #region VALUES AND PROPERTIES
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] private string _playfabIdToLoad;
        //the keys we want to request
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] private List<string> _desiredKeys = new List<string>();
        #endregion

        #region INITIATION
        /// <summary>
        /// setting a different id to load
        /// </summary>
        /// <param name="idToLoad">the playfab id we want to load</param>
        public void SetIdToLoad(string idToLoad) { _playfabIdToLoad = idToLoad; }

        /// registers the save queue to all saveables
        protected override void SpecificInitialization()
        {
            for (int i = 0; i < _groupOfElements.Length; i++)
                _groupOfElements[i].RegisterLoadGroup(this);
        }

        protected override void SetupCurrentTask(int maxElements)
        {
            _CurrentTask.SetupLoadTask(_playfabIdToLoad, GetArraySegment(maxElements));
        }
        #endregion

        #region DISABLE RESET
        public override void ResetThis()
        {
            base.ResetThis();
            _playfabIdToLoad = "";
        }
        #endregion
    }
}
