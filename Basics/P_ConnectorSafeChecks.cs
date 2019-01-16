using MEC;
using PlayFab;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace JReact.Playfab_Interact
{
    /// <summary>
    /// checks the connection and automatically request it if missing
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Basics/Connection Safe Checks")]
    public class P_ConnectorSafeChecks : ScriptableObject, iTrackable
    {
        #region PROPERTIES AND VALUES
        private JAction OnMissingInternet;
        private JAction OnMissingPlayfabLogin;

        //the amount of time passing between each checl
        [BoxGroup("Check Time", true, true, 0), ShowInInspector, ReadOnly] private float _checkSecondsInterval = 2.5f;
        //used to track if player is currently checking the connection
        [BoxGroup("Check Time", true, true, 0), ShowInInspector, ReadOnly] private bool _trackingConnection = false;
        public bool IsTracking { get { return _trackingConnection; } private set { _trackingConnection = value; } }

        #region CHECK PROPERTIES
        //used to check if player has internet connection
        [BoxGroup("Connection", true, true, 5), ShowInInspector, ReadOnly]
        public bool InternetReady
        {
            get
            {
                // check if we have a connection
                var connectionValid = Application.internetReachability != NetworkReachability.NotReachable;

                // sends the no reachable if missing
                if (!connectionValid &&
                    OnMissingInternet != null) OnMissingInternet();

                // return it
                return connectionValid;
            }
        }
        //used to check if player is logged into playfab
        [BoxGroup("Connection", true, true, 5), ShowInInspector, ReadOnly] public bool PlayerLoggedIntoPlayfab
        {
            get { return PlayFabClientAPI.IsClientLoggedIn(); }
        }

        // Returns true if internet connection is available and player is logged on playfab
        // Does not send events though  
        [BoxGroup("Connection", true, true, 5), ShowInInspector, ReadOnly] public bool ConnectionAvailable
        {
            get { return InternetReady && PlayerLoggedIntoPlayfab; }
        }
        #endregion
        #endregion

        #region COMMANDS
        /// <summary>
        /// start checking for playfab connection
        /// </summary>
        [BoxGroup("Debug", true, true, 100), Button("Start checking for connection")]
        public void StartTracking()
        {
            //make sure we do not start while we are already going
            if (IsTracking)
            {
                PConsole.Warning("Checking already. Abort to avoid checking multiple times.", name, this);
                return;
            }

            //start checking and enable the first coroutine
            IsTracking = true;
            Timing.RunCoroutine(WaitThenCheck(_checkSecondsInterval), Segment.Update, P_Constants.COROUTINE_ConnectionChecking);
        }

        /// <summary>
        /// stop checking for playfab connection
        /// </summary>
        [BoxGroup("Debug", true, true, 100), Button("Stop checking for connection")]
        public void StopTracking()
        {
            //ignore if we are not checking
            if (!IsTracking)
            {
                PConsole.Warning("This is not checking.", name, this);
                return;
            }

            //stop checking and remove the coroutines
            IsTracking = false;
            Timing.KillCoroutines(P_Constants.COROUTINE_ConnectionChecking);
        }
        #endregion

        #region CHECKS
        //just wait and check again
        private IEnumerator<float> WaitThenCheck(float intervalSeconds)
        {
            yield return Timing.WaitForSeconds(intervalSeconds);
            while (IsTracking)
            {
                CheckingAllConnections();
                yield return Timing.WaitForSeconds(intervalSeconds);
            }
        }

        //this is used to check if any connection is missing
        private bool CheckingAllConnections()
        {
            if (!CheckingInternet()) return false;
            if (!CheckingPlayfabConnection()) return false;
            return true;
        }

        //checking internet connection
        private bool CheckingInternet()
        {
            if (InternetReady) return true;
            if (OnMissingInternet != null) OnMissingInternet();
            PConsole.Warning("No Internet.", name, this);
            return false;
        }

        //this method checks for playfab connection, if missing it enables one
        private bool CheckingPlayfabConnection()
        {
            if (PlayerLoggedIntoPlayfab) return true;
            if (OnMissingPlayfabLogin != null) OnMissingPlayfabLogin();
            return false;
        }
        #endregion

        #region SUBSCRIBERS
        public void SubscribeToMissingInternet(JAction actionToSend) { OnMissingInternet      += actionToSend; }
        public void UnSubscribeToMissingInternet(JAction actionToSend) { OnMissingInternet    -= actionToSend; }
        public void SubscribeToMissingPlayfab(JAction actionToSend) { OnMissingPlayfabLogin   += actionToSend; }
        public void UnSubscribeToMissingPlayfab(JAction actionToSend) { OnMissingPlayfabLogin -= actionToSend; }
        #endregion

        #region DISABLE AND RESET
        //we reset this on disable
        protected virtual void OnDisable() { ResetThis(); }
        private void ResetThis() { StopTracking(); }
        #endregion
    }
}
