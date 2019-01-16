//using Facebook.Unity;
//using JReact.Facebook_Interact;
//using JReact;
//using MEC;
//using PlayFab;
//using PlayFab.ClientModels;
//using Sirenix.OdinInspector;
//using System.Collections.Generic;
//using UnityEngine;
//
//namespace JReact.Playfab_Interact.Login
//{
//    /// <summary>
//    /// this class will be used to connect playfab using facebook
//    /// </summary>
//    [CreateAssetMenu(menuName = "Playfab Interact/Login/Facebook")]
//    public class P_PlayfabFacebookConnector : P_PlayfabLoginManager
//    {
//        #region FIELDS AND PROPERTIES
//        //the facebook connector
//        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private F_FacebookConnector _facebookConnector;
//
//        //we consider this as facebook connection
//        protected override ConnectionType _connectionType
//        { get { return ConnectionType.Facebook; } }
//
//        //here we store the current attempt
//        [FoldoutGroup("State", false, 5), ReadOnly, ShowInInspector] private int _currentAttempt;
//        protected override string _gameTitle {
//            get { return ""; }
//        }
//        #endregion FIELDS AND PROPERTIES
//
//        #region MAIN IMPLEMENTATION
//        /// trigger the connection using facebook
//        protected override IEnumerator<float> LoginImplementation(int currentAttempt = 0)
//        {
//            //setting the id
//            HelperConsole.DisplayMessage(string.Format("Playfab Facebook Connect Attempt {0} of {1}.\n", currentAttempt, _maxAttempts));
//            if (_wantToDebug) { yield return Timing.WaitForSeconds(_debugDelay); }
//
//            //store the attempt
//            _currentAttempt = currentAttempt;
//            //try to login with playfab and retry if we have an error
//            Timing.RunCoroutine(InitializeThenLogin(), Segment.SlowUpdate, P_Constants.COROUTINE_LoaderTag);
//        }
//
//        #endregion MAIN IMPLEMENTATION
//
//        #region INITIALIZE
//
//        //used to initialize facebook and the login
//        //subscribe to init then init
//        private IEnumerator<float> InitializeThenLogin()
//        {
//            //send the debug if requestes
//            if (_wantToDebug) { P_PlayfabConsoleLogger.DisplayMessage(string.Format("Initializing Facebook. Currently initialized: {0}", _facebookConnector.IsFacebookInitialized), name); yield return Timing.WaitForSeconds(_debugDelay); }
//            //go ahead if already initialized, this may happen if we had an error and we retry to go through these steps
//            if (_facebookConnector.IsFacebookInitialized) { FacebookInitComplete(_facebookConnector.AuthTicket); yield break; }
//            //start tracking and initializing
//            _facebookConnector.SubscribeToInit(FacebookInitComplete);
//            _facebookConnector.InitializeFacebook();
//        }
//
//        //use the facebook initialization complete
//        //stop listening to init and then login
//        private void FacebookInitComplete(string token)
//        {
//            //stop tracking init
//            _facebookConnector.UnSubscribeToInit(FacebookInitComplete);
//
//            //directly connect if we have the token
//            if (!string.IsNullOrEmpty(token))
//            {
//                Timing.RunCoroutine(LoginWithFacebook(token), Segment.SlowUpdate, P_Constants.COROUTINE_LoaderTag);
//                return;
//            }
//            //otherwise start a login session
//            //start tracking and initializing
//            _facebookConnector.SubscribeToLogin(FacebookLoginComplete);
//            _facebookConnector.FacebookLogin();
//        }
//
//        #endregion INITIALIZE
//
//        #region FACEBOOK LOGIN SESSION
//
//        //the facebook login complete
//        private void FacebookLoginComplete(ILoginResult result)
//        {
//            //stop listening to the subscription
//            _facebookConnector.UnSubscribeToLogin(FacebookLoginComplete);
//            //logging on playfab
//            Timing.RunCoroutine(LoginWithFacebook(result.AccessToken.TokenString), Segment.SlowUpdate, P_Constants.COROUTINE_LoaderTag);
//        }
//
//        #endregion FACEBOOK LOGIN SESSION
//
//        #region CONNECT FACEBOOK WITH PLAYFAB
//
//        //used to connect playfab with facebook
//        private IEnumerator<float> LoginWithFacebook(string token)
//        {
//            //send the debug if requested
//            if (_wantToDebug) { P_PlayfabConsoleLogger.DisplayMessage(string.Format("Login in playfab with facebook token: {0}", token), name); yield return Timing.WaitForSeconds(_debugDelay); }
//            // We proceed with making a call to PlayFab API. We pass in current Facebook AccessToken and let it create
//            // and account using CreateAccount flag set to true. We also pass the callback for Success and Failure results
//            PlayFabClientAPI.LoginWithFacebook(CreateRequestFromToken(token),
//                PlayfabLogin_OnSuccess, error => PlayfabLogin_OnError(error, _currentAttempt++));
//        }
//
//        //used to create the facebook request
//        private LoginWithFacebookRequest CreateRequestFromToken(string accessToken)
//        {
//            return new LoginWithFacebookRequest()
//            {
//                TitleId = PlayFabSettings.TitleId,
//                AccessToken = accessToken,
//                CreateAccount = true,
//                InfoRequestParameters = P_Constants.LoginRequestParameters
//            };
//        }
//
//        // When processing both results, we just set the message, explaining what's going on.
//        protected override void PlayfabLogin_OnSuccess(PlayFab.ClientModels.LoginResult result)
//        {
//            //feedback message
//            F_FacebookConsoleLogger.DisplayMessage(string.Format("-{0}-PlayFab Facebook Auth Complete. Session ticket: {1}", name, result.SessionTicket));
//            //save the player data
//            base.PlayfabLogin_OnSuccess(result);
//        }
//
//        #endregion CONNECT FACEBOOK WITH PLAYFAB
//    }
//}