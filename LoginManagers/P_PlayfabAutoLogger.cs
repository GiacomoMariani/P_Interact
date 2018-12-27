using MEC;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace JReact.Playfab_Interact.Login
{
    /// <summary>
    /// this class is used to connect to playfab. It also checks if player has lost the connection and sends related events.
    /// It could retry the connection using  a JMonoLoop class
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Login/Auto Logger")]
    public class P_PlayfabAutoLogger : ScriptableObject
    {
        // --------------- SETUP --------------- //
        //the timeout before the error
        [BoxGroup("Setup - Connection", true, true, 0), SerializeField] private float _secondsBeforeTimeout = 15;
        //the interval for the checking
        [BoxGroup("Setup - Connection", true, true, 0), SerializeField] private float _intervalInSeconds = .5f;

        [BoxGroup("Setup - Playfab", true, true, 2), SerializeField, AssetsOnly, Required]
        private P_ConnectorSafeChecks _connectionChecker;
        [BoxGroup("Setup - Playfab", true, true, 2), SerializeField, AssetsOnly, Required]
        private P_CustomIdConnector _defaultLoginManager;

        // --------------- STATE --------------- //
        [BoxGroup("Setup", true, true, 5), ReadOnly, ShowInInspector] private bool _connectionInProgress = false;

        #region INITIALIZATION
        private void Awake() { SanityChecks(); }

        private void SanityChecks()
        {
            Assert.IsNotNull(_connectionChecker,
                             string.Format("This object ({0}) needs an element for the value _playfabChecker", name));
            Assert.IsNotNull(_defaultLoginManager,
                             string.Format("This object ({0}) needs an element for the value _playfabLogger", name));
        }
        #endregion

        /// <summary>
        /// this is the main method to try connecting the logger
        /// </summary>
        public void TryConnect()
        {
            // --------------- ERRORS --------------- //
            //stop if already trying to connect
            if (_connectionInProgress) return;
            //send an error if we're not connected to internet
            if (!_connectionChecker.InternetReady)
            {
                P_PlayfabConsoleLogger.DisplayWarning(P_Constants.ERROR_NoInternetConnection, name);
                return;
            }

            //ignore if we're connected already
            if (_connectionChecker.PlayerLoggedIntoPlayfab) return;

            // --------------- ERROR PASSED --------------- //
            //try connecting in all other cases
            Timing.RunCoroutine(TryingToLog(), Segment.Update, P_Constants.COROUTINE_autoConnectionTag);
        }

        private IEnumerator<float> TryingToLog()
        {
            _connectionInProgress = true;
            //trigger the connection
            _defaultLoginManager.TriggerPlayfabLogin();
            //start the timer
            float countTime = 0f;
            //wait until ready
            while (!_connectionChecker.PlayerLoggedIntoPlayfab)
            {
                //stop if we moved above the max time
                if (countTime > _secondsBeforeTimeout)
                {
                    TimeOut();
                    yield break;
                }

                //wait the time and add it
                yield return Timing.WaitForSeconds(_intervalInSeconds);
                countTime += _intervalInSeconds;
            }

            //confirm the log
            LogConfirmed();
        }

        //stop if we have a time out
        private void TimeOut()
        {
            P_PlayfabConsoleLogger
                .DisplayWarning(string.Format("The connection check had a timeout. {0} second passed.", _secondsBeforeTimeout), name);
        }

        //used to confirm the log
        private void LogConfirmed() { ResetThis(); }

        #region DISABLE AND RESET
        //we reset this on disavle
        protected virtual void OnDisable() { ResetThis(); }

        private void ResetThis()
        {
            _connectionInProgress = false;
            Timing.KillCoroutines(P_Constants.COROUTINE_autoConnectionTag);
        }
        #endregion
    }
}
