using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact.Login
{
    /// <summary>
    /// this class is used to decide which type of connection use, depending on the last connection
    /// </summary>
    [CreateAssetMenu(menuName = "Playfab Interact/Login/Switcher")]
    public class P_ConnectionSwitcher : ScriptableObject
    {
        #region FIELDS AND PROPERTIES
        //the main player
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_PlayfabPlayer _player;
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_PlayfabLoginManager _defaultLoginManager;
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_CustomIdConnector _customIdLoginManager;
//        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_PlayfabFacebookConnector _facebookLoginManager;
        #endregion

        /// <summary>
        /// this is the main method to connect with playfab, it uses the last type of connection to retry that
        /// </summary>
        public void StartConnection()
        {
            switch ((ConnectionType)_player.ConnectionType)
            {
                case ConnectionType.NotSet:
                    _defaultLoginManager.TriggerPlayfabLogin();
                    break;
                case ConnectionType.Silent:
                    _customIdLoginManager.TriggerPlayfabLogin();
                    break;
//                case ConnectionType.Facebook:
//                    _facebookLoginManager.TriggerPlayfabLogin();
//                    break;
                default:
                    break;
            }
        }
    }
}
