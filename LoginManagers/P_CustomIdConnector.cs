using JReact;
using MEC;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;
using UnityEngine;

namespace JReact.Playfab_Interact.Login
{
    /// <summary>
    /// this script is used to log player into playfab
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Login/Custom Id")]
    public class P_CustomIdConnector : P_PlayfabLoginManager
    {
        #region FIELDS AND PROPERTIES
        protected override ConnectionType _connectionType { get { return ConnectionType.Silent; } }
        protected override string _gameTitle { get { return ""; } }
        #endregion

        #region STEP TWO - LOG INTO PLAYFAB
        protected override IEnumerator<float> LoginImplementation(int currentAttempt = 0)
        {
            //setting the id
            P_PlayfabConsoleLogger.DisplayMessage(string.Format("Playfab Connect Attempt {0} of {1}.\nDesire new account: {2}"
                                                                , currentAttempt, _maxAttempts, _playerData.FirstTimePlay), name);

            //create the login request
            var request = SetupLoginRequest();
            if (_wantToDebug)
            {
                P_PlayfabConsoleLogger.DisplayMessage(string.Format("Logging with custom id", P_Constants.DEBUG_PlayfabInteract), name);
                yield return Timing.WaitForSeconds(_debugDelay);
            }

            PlayFabClientAPI.LoginWithCustomID(request, PlayfabLogin_OnSuccess,
                                               error => PlayfabLogin_OnError(error, currentAttempt));
        }

        private LoginWithCustomIDRequest SetupLoginRequest()
        {
            var request = new LoginWithCustomIDRequest()
            {
                TitleId = _gameTitle,
                //the desire of creating a new account, checked via player prefs
                CreateAccount = _playerData.FirstTimePlay,
                //the player name
                CustomId = _playerData.GeneratePlayerId(),
                //the request parameters
                InfoRequestParameters = P_Constants.LoginRequestParameters
            };
            //return the request
            return request;
        }
        #endregion
    }
}
