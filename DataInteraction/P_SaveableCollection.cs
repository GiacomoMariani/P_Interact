using System;
using System.Collections.Generic;
using System.Linq;
using JReact.Collections;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Data
{
    /// <summary>
    /// this class is used to save a collection of elements
    /// </summary>
    /// <typeparam name="T">type related to the collection</typeparam>
    /// <typeparam name="V">type converted into a saveable</typeparam>
    public abstract class P_SaveableCollection<T, V> : P_Saveable
        where V : SaveableData
    {
        #region FIELDS AND PROPERTIES
        // --------------- STATE --------------- //
        //the collection we want to save
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector]
        protected abstract J_ReactiveCollection<T> _CollectionToSave { get; }
        //this dictionary is used to track the data
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] protected Dictionary<T, V> _tracker = new Dictionary<T, V>();
        //the data to be save
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector]
        private P_CollectionDataToSave _dataToSave = new P_CollectionDataToSave();
        #endregion

        #region INITIALIZATION
        //used to retrieve the element
        public override string GetDataIdentifier() { return _CollectionToSave.name; }

        //start tracking
        protected override void InitiateSaveCommandWith(P_SaveGroupQueue saveSystem)
        {
            Assert.IsNotNull(_CollectionToSave, $"{name} requires a _allDroneRequests");
            //first save
            TrackCollection(_CollectionToSave);
            //listen to all possible changes
            _CollectionToSave.SubscribeToCollectionAdd(ElementAddedToCollection);
            _CollectionToSave.SubscribeToCollectionRemove(ElementRemovedFromCollection);
        }

        //this is used to start tracking all collection from the start, just in case if it has any elements already
        private void TrackCollection(J_ReactiveCollection<T> currentCollection)
        {
            //check all the elements
            for (int i = 0; i < currentCollection.Count; i++)
                UpdateElementData(currentCollection[i], false);
        }
        #endregion

        #region COLLECTION CHANGES
        private void ElementAddedToCollection(T elementAdded)
        {
            //sanity check
            Assert.IsFalse(_tracker.ContainsKey(elementAdded),
                           $"{P_Constants.DEBUG_PlayfabInteract}{name} is trying to add the element {elementAdded.ToString()} -of- {_CollectionToSave.name}, but we're already tracking it.");
            StartTrackingElement(elementAdded);
            UpdateElementData(elementAdded);
        }

        //sent when a element is removed from the collection
        private void ElementRemovedFromCollection(T elementRemoved)
        {
            //sanity check
            Assert.IsTrue(_tracker.ContainsKey(elementRemoved),
                          $"{P_Constants.DEBUG_PlayfabInteract}{name} is trying to remove the element {elementRemoved.ToString()} -of- {_CollectionToSave.name}, but it's not tracked yet.");
            StopTrackingElement(elementRemoved);
            _tracker.Remove(elementRemoved);
            RequestSaveSchedule();
        }
        #endregion

        #region TRACKING
        //used to track a single element
        private void UpdateElementData(T element, bool wantToSave = true)
        {
            //add the elements to the tracker
            _tracker[element] = ConvertRequestToData(element);
            //request to save
            if (wantToSave) RequestSaveSchedule();
        }

        // start tracking any changes for the elements we have
        protected virtual void StartTrackingElement(T elementToTrack) { }

        // stop tracking an element
        protected virtual void StopTrackingElement(T elementToStopTracking) { }
        #endregion

        #region CONVERTERS
        /// <summary>
        /// converts an element into data
        /// </summary>
        /// <param name="element">the element we want to convert</param>
        /// <returns>the converted element</returns>
        protected abstract V ConvertRequestToData(T element);

        /// <summary>
        /// main method to convert this as json
        /// </summary>
        /// <returns>the converted json</returns>
        public override string SaveThisIntoJson()
        {
            _dataToSave.ArrayOfData = _tracker.Values.ToArray();
            //convert the class into a json
            return JsonUtility.ToJson(_dataToSave);
        }
        #endregion

        #region LOAD METHODS
        protected override void LoadThisFromJson(string jsonData, DateTime dateTime)
        {
            //convert the data
            var dataLoaded = JsonUtility.FromJson<P_CollectionDataToSave>(jsonData);
            //load the data, we will pass also the package converter
            LoadCompleted(dataLoaded, dateTime);
        }

        /// <summary>
        /// this is the main method used to load this element
        /// </summary>
        /// <param name="dataLoaded">the data retrieved</param>
        /// <param name="dateTime">the last save time</param>
        protected abstract void LoadCompleted(P_CollectionDataToSave dataLoaded, DateTime dateTime);
        #endregion

        #region DISABLE AND RESET
        //we reset this on disable
        public override void ResetThis()
        {
            base.ResetThis();
            //clear the tracker
            _tracker.Clear();

            //stop tracking the change of requests
            if (_CollectionToSave == null) return;
            _CollectionToSave.UnSubscribeToCollectionAdd(ElementAddedToCollection);
            _CollectionToSave.UnSubscribeToCollectionRemove(ElementRemovedFromCollection);
            //stop tracking each specific element
            for (int i = 0; i < _CollectionToSave.Count; i++)
            {
                //stop tracking
                StopTrackingElement(_CollectionToSave[i]);
            }

            //reset this
            _CollectionToSave.ResetThis();
        }
        #endregion

        //the class we want to save
        [System.Serializable] public class P_CollectionDataToSave
        {
            public V[] ArrayOfData;
        }
    }
}
