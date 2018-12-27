using JReact.Playfab_Interact.PlayerName;
using PlayFab;
using PlayFab.ClientModels;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact
{
    /// <summary>
    /// used to store the data related to this player
    /// TODO Decouple first time interaction from player data
    /// TODO Decouple connection type from player data
    /// TODO catch log success from here
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Basics/Player Data")]
    public class P_PlayfabPlayer : ScriptableObject, iObservable<PlayerProfileModel>
    {
        #region FIELDS AND PROPERTIES
        private event JGenericDelegate<PlayerProfileModel> OnPlayerProfileRetrieved;

        // --------------- SETUP --------------- //
        [BoxGroup("Player Data - Setup", true, true, 0), SerializeField, AssetsOnly, Required]
        private P_PlayerName _playerName;
        //this is used to check if player requires to be initiated
        [BoxGroup("Player Data - Setup", true, true, 0), SerializeField, AssetsOnly, Required]
        private JReactiveBool _initiationRequired;

        // --------------- STATE --------------- //
        //to see if this is the first time play
        [BoxGroup("Player Data - State", true, true, 5), ReadOnly] public bool FirstTimePlay
        {
            get { return _initiationRequired.CurrentValue; }
        }
        [BoxGroup("Player Data - State", true, true, 5), ReadOnly] public string Playfab_PlayerId { get; private set; }
        public PlayerProfileModel PlayerProfile { get { return playerProfile; } private set { playerProfile = value; } }
        //the session ticket
        [BoxGroup("Player Data - State", true, true, 5), ReadOnly] public string SessionTicket { get; private set; }
        //the playfab main profile
        [BoxGroup("Player Data - State", true, true, 5), ReadOnly] private PlayerProfileModel playerProfile;
        //the connection type is an int
        [BoxGroup("Player Data - State", true, true, 5), ReadOnly]
        internal int ConnectionType
        {
            get { return PlayerPrefs.GetInt(P_Constants.PREF_PlayerIsNew); }
            private set { PlayerPrefs.SetInt(P_Constants.PREF_PlayerIsNew, value); }
        }
        #endregion

        #region STARTUP VALUES
        /// <summary>
        /// this method creates an id from the device identifier
        /// </summary>
        /// <returns>an id to be used to save the player</returns>
        internal string GeneratePlayerId() { return SystemInfo.deviceUniqueIdentifier; }

        //used to set the login type, so we remember how this player logged the last time
        internal void SetLoginType(ConnectionType typeOfConnection) { ConnectionType = (int) typeOfConnection; }
        #endregion

        #region INITIALIZATION
        /// <summary>
        /// used to check if player requires initialization
        /// </summary>
        public void CheckInitialization() { _initiationRequired.CurrentValue = ConnectionType == 0; }

        /// <summary>
        /// completes the initialization
        /// </summary>
        public void CompleteInitialization()
        {
            if (_initiationRequired) _initiationRequired.CurrentValue = false;
        }
        #endregion

        #region PLAYER PROFILE GETTER
        /// <summary>
        /// this is used to store the login result
        /// </summary>
        /// <param name="result">the result to be stored</param>
        internal void StoreLoginResult(LoginResult result, ConnectionType typeOfConnection)
        {
            HelperConsole.DisplayMessage(string.Format("{0} set connection type as: {1}", name,
                                                       typeOfConnection.ToString()));
            //save the session ticket
            SessionTicket = result.SessionTicket;
            //store the id
            Playfab_PlayerId = result.PlayFabId;
            //get the entire profile
            GetPlayfabPlayerProfile(Playfab_PlayerId);
            //store the player if no more guest
            SetLoginType(typeOfConnection);
        }

        // used to load the player profile
        private void GetPlayfabPlayerProfile(string playFabId)
        {
            PlayFabClientAPI.GetPlayerProfile(PlayerProfileRequest(playFabId),
                                              ProfileSuccessCallback,
                                              ProfileErrorCallback);
        }

        //the callback when the player profile is retrieved
        private void ProfileSuccessCallback(GetPlayerProfileResult result)
        {
            //message feedback
            P_PlayfabConsoleLogger.DisplayMessage(string.Format("Stored profile for: {0}",
                                                                result.PlayerProfile.DisplayName), name);
            //store the player profile and inject player name
            PlayerProfile = result.PlayerProfile;
            _playerName.InjectName(PlayerProfile.DisplayName);
            //send the event
            if (OnPlayerProfileRetrieved != null) OnPlayerProfileRetrieved(result.PlayerProfile);
        }

        //set the profile request
        private GetPlayerProfileRequest PlayerProfileRequest(string playFabId)
        {
            return new GetPlayerProfileRequest()
            {
                PlayFabId          = playFabId,
                ProfileConstraints = P_Constants.PlayerViewConstrains
            };
        }
        #endregion

        #region DEBUG
        [BoxGroup("Debug", true, true, 100), Button(ButtonSizes.Medium)]
        public void ResetPlayer()
        {
            //resetting the int to 0
            SetLoginType(Playfab_Interact.ConnectionType.NotSet);
            HelperConsole.DisplayWarning(string.Format
                                             ("Remember to remove also the data from playfab.\nPlayfab_PlayerId {0}.\nPlayfab_CustomPlayerId {1}",
                                              Playfab_PlayerId, GeneratePlayerId()));
        }
        #endregion

        #region ERROR CALLBACK
        //the callback when the player profile cannot be retrieved
        private void ProfileErrorCallback(PlayFabError error) { P_PlayfabConsoleLogger.LogErrorFrom(error, name); }
        #endregion

        #region SUBSCRIBERS
        public void Subscribe(JGenericDelegate<PlayerProfileModel> actionToAdd) { OnPlayerProfileRetrieved      += actionToAdd; }
        public void UnSubscribe(JGenericDelegate<PlayerProfileModel> actionToRemove) { OnPlayerProfileRetrieved -= actionToRemove; }
        #endregion
    }
}
