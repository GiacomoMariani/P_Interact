using PlayFab.ClientModels;

namespace JReact.Playfab_Interact
{
    //constant useds in playfab interact
    public class P_Constants
    {
        #region COROUTINES
        internal const string COROUTINE_ConnectionChecking = "COROUTINE_CheckTag";
        //the string related to this coroutine
        internal  const string COROUTINE_autoConnectionTag = "Coroutine_PlayfabAutoConnect_Tag";
        #endregion

        #region DEBUG
        #endregion
        
        //wait time
        internal const float WaitTimeForActions = 1f; 
        //the max possible amount of elements in queue
        internal const int MaxRequestAmount = 10;
        // --------------- PLAYER PREFS --------------- //
        //used to check if player is new. 1 = already registered 0 = new player
        internal const string PREF_PlayerIsNew = "Playfab_NewPlayerLog";
        // --------------- LOGIN --------------- //
        internal const string ERROR_NoInternetConnection = "Lacking Internet Connection.\nPlease check your connection.";
        internal const string COROUTINE_LoaderTag = "COROUTINE_Playfab_LoggerTag";
        
        #region PARAMETERS
        //the view constains
        internal static readonly PlayerProfileViewConstraints PlayerViewConstrains = new PlayerProfileViewConstraints() { ShowDisplayName = true };

        //the request for the login
        internal static readonly GetPlayerCombinedInfoRequestParams LoginRequestParameters = new GetPlayerCombinedInfoRequestParams()
        {
            GetUserInventory = true,
            GetPlayerProfile = true,
            GetPlayerStatistics = true,
            GetTitleData = true,
            GetUserVirtualCurrency = true,
            ProfileConstraints = PlayerViewConstrains
        };
        #endregion

        #region DEBUG
        internal const string DEBUG_PlayfabInteract = "-Playfab_Interact- ";
        internal const string DEBUG_PlayfabInteractImportant = "-Playfab_Interact_Important- ";
        #endregion
    }
} 