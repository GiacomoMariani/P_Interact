using JReact;
using MEC;
using PlayFab;
using PlayFab.ClientModels;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace JReact.Playfab_Interact.Login
{
    /// <summary>
    /// generic class to handle playfab logins
    /// </summary>
    public abstract class P_PlayfabLoginManager : ScriptableObject
    {
        #region FIELDS AND PROPERTIES
        public event PlayfabLoginEvent OnPlayfabLogin;

        [BoxGroup("Setup", true, true, 0), ReadOnly] protected abstract string _gameTitle { get; }
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] protected P_PlayfabPlayer _playerData;
        [BoxGroup("State", true, true, 5), ReadOnly] protected abstract ConnectionType _connectionType { get; }

        [BoxGroup("State", true, true, 5), ReadOnly] private LoginResult _loginResult;
        public LoginResult ThisLoginResult { get { return _loginResult; } private set { _loginResult = value; } }

        //connection elements, the max attemps for connection and the connection safe checks
        [BoxGroup("Connection", true, true, 20), SerializeField] protected int _maxAttempts = 5;
        [BoxGroup("Connection", true, true, 20), SerializeField, AssetsOnly, Required]
        protected P_ConnectorSafeChecks _connectionChecks;

        //a way to check the debug
        [BoxGroup("Debug", true, true, 100), SerializeField] protected bool _wantToDebug = false;
        [BoxGroup("Debug", true, true, 100), SerializeField] protected float _debugDelay = 2.0f;
        #endregion

        #region STEP ONE - CHECKING THE CONNECTION
        /// <summary>
        /// triggers playfab login
        /// </summary>
        [BoxGroup("Connection", true, true, 20), Button("Login To Playfab - Custom Id", ButtonSizes.Large)]
        public void TriggerPlayfabLogin() { Timing.RunCoroutine(InitiateLogin(), Segment.Update, P_Constants.COROUTINE_LoaderTag); }

        private IEnumerator<float> InitiateLogin()
        {
            if (CheckIfLoggedAlready()) yield break;
            if (CheckInternetAvailability()) yield break;

            //log in the game using the type of login we want
            if (_wantToDebug)
            {
                HelperConsole.DisplayMessage(string.Format("{0} - Logging in", P_Constants.DEBUG_PlayfabInteract));
                yield return Timing.WaitForSeconds(_debugDelay);
            }

            SetPlayfabTitle(_gameTitle);
            //triggering the login
            Timing.RunCoroutine(LoginImplementation(), Segment.SlowUpdate, P_Constants.COROUTINE_LoaderTag);
        }

        private bool CheckInternetAvailability()
        {
            if (!_connectionChecks.InternetReady)
            {
                P_PlayfabConsoleLogger.DisplayWarning(P_Constants.ERROR_NoInternetConnection, name);
                return true;
            }

            return false;
        }

        private bool CheckIfLoggedAlready()
        {
            if (_connectionChecks.PlayerLoggedIntoPlayfab)
            {
                P_PlayfabConsoleLogger.DisplayWarning
                    ("We're trying to connect, to playfab, but we're already connected.\n This should never happen.", name);
                return true;
            }

            return false;
        }

        private void SetPlayfabTitle(string gameTitle) { PlayFabSettings.TitleId = gameTitle; }
        #endregion

        #region STEP 2 - SPECIFIC IMPLEMENTATION
        protected abstract IEnumerator<float> LoginImplementation(int currentAttempt = 0);
        #endregion

        #region SUCCESS HANDLING
        protected virtual void PlayfabLogin_OnSuccess(LoginResult result)
        {
            Timing.RunCoroutine(ConfirmPlayfabLogin(result), Segment.Update, P_Constants.COROUTINE_LoaderTag);
        }

        protected virtual IEnumerator<float> ConfirmPlayfabLogin(LoginResult result)
        {
            //send the event of the connection
            ThisLoginResult = result;
            if (OnPlayfabLogin != null) OnPlayfabLogin(result);
            //store and authenticate player
            _playerData.StoreLoginResult(result, _connectionType);
            P_PlayfabConsoleLogger.DisplayMessage(string.Format("Playfab Connect Succesfull. Player custom id: {0}. Player id: {1}"
                                                                , _playerData.GeneratePlayerId(), result.PlayFabId), name);
            if (_wantToDebug) { yield return Timing.WaitForSeconds(_debugDelay); }
        }
        #endregion

        #region ERROR HANDLING
        //register one error and retry if we've not reached the max attempts
        protected virtual void PlayfabLogin_OnError(PlayFabError receivedError, int currentAttempt)
        {
            P_PlayfabConsoleLogger.DisplayWarning(string.Format("Playfab Login Attempt {0} is failed. Error: {1}", currentAttempt,
                                                                receivedError.ErrorMessage), name);
            //adding one attempt and stop if we made all the attempts
            currentAttempt++;
            if (currentAttempt > _maxAttempts)
            {
                OnPlayFabError(receivedError);
                return;
            }

            Timing.RunCoroutine(LoginImplementation(currentAttempt), Segment.Update, P_Constants.COROUTINE_LoaderTag);
        }

        //used to show the error
        protected virtual void OnPlayFabError(PlayFabError error) { P_PlayfabConsoleLogger.LogErrorFrom(error, name); }
        #endregion

        #region RESET
        protected virtual void OnDisable() { ResetThis(); }

        protected virtual void ResetThis() { Timing.KillCoroutines(P_Constants.COROUTINE_LoaderTag); }
        #endregion
    }
}
