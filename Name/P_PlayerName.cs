using PlayFab;
using PlayFab.ClientModels;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact.PlayerName
{
    /// <summary>
    /// this class is used to work on player name
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Player/Name")]
    public class P_PlayerName : JReactiveString
    {
        #region FIELDS AND PROPERTIES
        private event JGenericDelegate<string> OnNameChange;
        //the event of the error
        private event JGenericDelegate<string> OnNameChangeError;

        //reference to the player profile
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_PlayfabPlayer _playerProfile;
        //the default name if the display name has not been set yet
        [BoxGroup("Setup", true, true, 0), SerializeField] private string _defaultName = "Player Name";

        //we override the main string to avoid having non wanted names
        public override string CurrentValue
        {
            get
            {
                if (!DisplayNameValid()) return _defaultName;
                return base.CurrentValue;
            }
            set { base.CurrentValue = value; }
        }
        #endregion

        #region START SETUP
        /// the name will be set at startup
        internal void InjectName(string name) { CurrentValue = name; }
        #endregion

        #region SETTER
        public void SetPlayerName(string playerName)
        {
            var request = CreateRequest(playerName);
            PlayFabClientAPI.UpdateUserTitleDisplayName(request, SetName_OnSuccess, SetName_OnError);
        }

        private UpdateUserTitleDisplayNameRequest CreateRequest(string nameToSet)
        {
            return new UpdateUserTitleDisplayNameRequest { DisplayName = nameToSet };
        }

        private void SetName_OnSuccess(UpdateUserTitleDisplayNameResult result)
        {
            //send a feedback message
            P_PlayfabConsoleLogger.DisplayMessage(string.Format("Player Display Name Change into: {0}",
                                                                result.DisplayName), name);
            //confirm the name set
            CurrentValue = result.DisplayName;
            if (OnNameChange != null) OnNameChange(CurrentValue);
        }
        #endregion

        #region CHECKER
        // returns true if the display name is valid
        private bool DisplayNameValid()
        {
            if (_currentValue == _playerProfile.GeneratePlayerId()) return false;
            if (_currentValue == _playerProfile.Playfab_PlayerId) return false;
            if (_currentValue == "") return false;
            return true;
        }
        #endregion

        #region ERROR CALLBACK
        private void SetName_OnError(PlayFabError error)
        {
            //send the message and the event
            P_PlayfabConsoleLogger.LogErrorFrom(error, name);
            if (OnNameChangeError != null) OnNameChangeError(error.ErrorMessage);
        }
        #endregion

        #region SUBSCRIBERS
        public void SubscribeToNameChange(JGenericDelegate<string> actionToAdd) { OnNameChange += actionToAdd; }
        public void UnSubscribeToNameChange(JGenericDelegate<string> actionToRemove) { OnNameChange -= actionToRemove; }
        public void SubscribeToNameChangeError(JGenericDelegate<string> actionToAdd) { OnNameChangeError += actionToAdd; }
        public void UnSubscribeToNameChangeError(JGenericDelegate<string> actionToRemove) { OnNameChangeError -= actionToRemove; }
        #endregion
    }
}
