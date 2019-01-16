using System;
using JReact;
using PlayFab;
using PlayFab.ClientModels;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact
{
    /// <summary>
    /// This is triggered in the loading scene and it stores the current time.
    /// this class is used to get the time from the server
    /// </summary>

    [CreateAssetMenu(menuName = "Playfab Interact/Basics/Time Getter")]
    public class P_TimeLoader : ScriptableObject, iObservable<DateTime>
    {
        #region FIELDS AND PROPERTIES
        private event JGenericDelegate<DateTime> OnTimeRetrieved;

        //a bool to check if this is currently loading the time
        [BoxGroup("State", true, true, 0), ReadOnly] private bool _currentlyLoading = false;
        public bool CurrentlyLoading { get { return _currentlyLoading; } private set { _currentlyLoading = value; } }
        //to check if we retrieved the time already
        [BoxGroup("State", true, true, 0), ReadOnly] private bool _timeRetrieved = false;
        public bool TimeRetrieved { get { return _timeRetrieved; } private set { _timeRetrieved = value; } }

        //the date retrieved by the system
        [BoxGroup("Time", true, true, 5), ReadOnly] private string _timeToString { get { return CurrentTime.ToString(); } }
        private DateTime _currentTime;
        public DateTime CurrentTime { get { return _currentTime; } private set { _currentTime = value; } }

        //if we want some debug to test
        [BoxGroup("Debug", true, true, 10), SerializeField] private bool _debugMode = false;
        #endregion

        /// <summary>
        /// this is the main method to start asking for the current time
        /// </summary>
        public void GetCurrentTime()
        {
            if (AvoidMultipleLoading()) return;

            //set this as busy
            CurrentlyLoading = true;

            //create the new request
            var request = new GetTimeRequest();

            //send the request
            PlayFabClientAPI.GetTime(request, GetTime_OnSuccess, GetTime_OnFail);
        }

        private bool AvoidMultipleLoading()
        {
            if (CurrentlyLoading)
            {
                PConsole.Warning("We should never ask the time if we're already retrieving it. Message from {0}", name, this);
                return true;
            }

            return false;
        }

        private void GetTime_OnSuccess(GetTimeResult timeRetrieved)
        {
            if (_debugMode) PConsole.Log($"Time Retrieved: {timeRetrieved.Time}", name, this);
            //stop loading and confirm the time retrieved
            CurrentlyLoading = false;
            TimeRetrieved = true;
            //set the time
            CurrentTime = timeRetrieved.Time;
            if (OnTimeRetrieved != null) OnTimeRetrieved(CurrentTime);
        }

        private void GetTime_OnFail(PlayFabError error)
        {
            //stop loading
            CurrentlyLoading = false;
            //send the error
            PConsole.ErrorFrom(error, name, this);
        }

        #region SUBSCRIBE EVENTS
        public void Subscribe(JGenericDelegate<DateTime> actionToSend) { OnTimeRetrieved += actionToSend; }
        public void UnSubscribe(JGenericDelegate<DateTime> actionToSend) { OnTimeRetrieved -= actionToSend; }
        #endregion

        #region DISABLE AND RESET
        protected virtual void OnDisable() { ResetThis(); }
        private void ResetThis() { TimeRetrieved = false; CurrentlyLoading = false; }
        #endregion
    }
}