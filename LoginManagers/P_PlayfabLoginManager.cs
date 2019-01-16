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
        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] protected abstract ConnectionType _connectionType { get; }

        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] private LoginResult _loginResult;
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
                PConsole.Log($"Logging in", name, this);
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
                PConsole.Warning(P_Constants.ERROR_NoInternetConnection, name, this);
                return true;
            }

            return false;
        }

        private bool CheckIfLoggedAlready()
        {
            if (_connectionChecks.PlayerLoggedIntoPlayfab)
            {
                PConsole.Warning("Already connected to playfab stop connection.\n This should never happen.", name, this);
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
            PConsole.Log($"Playfab Connect Successful. Player custom id: {_playerData.GeneratePlayerId()}. Player id: {result.PlayFabId}",
                         name, this);
            if (_wantToDebug) { yield return Timing.WaitForSeconds(_debugDelay); }
        }
        #endregion

        #region ERROR HANDLING
        //register one error and retry if we've not reached the max attempts
        protected virtual void PlayfabLogin_OnError(PlayFabError receivedError, int currentAttempt)
        {
            PConsole.Warning($"Playfab Login Attempt {currentAttempt} is failed. Error: {receivedError.ErrorMessage}", name, this);
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
        protected virtual void OnPlayFabError(PlayFabError error) { PConsole.ErrorFrom(error, name); }
        #endregion

        #region RESET
        protected virtual void OnDisable() { ResetThis(); }

        protected virtual void ResetThis() { Timing.KillCoroutines(P_Constants.COROUTINE_LoaderTag); }
        #endregion
    }
}
