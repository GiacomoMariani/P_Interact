using JReact;
using JReact.Playfab_Interact.PlayerName;
using JReact.UiView;
using Sirenix.OdinInspector;
using UnityEngine;

namespace JReact.Playfab_Interact
{
    /// <summary>
    /// this class is used to show the player name on a text
    /// </summary>
    public class P_UiView_PlayerNameText : J_UiView_TextElement
    {
        #region FIELDS AND PROPERTIES
        [BoxGroup("Setup", true, true, 0), SerializeField, AssetsOnly, Required] private P_PlayerName _playerName;
        #endregion

        //track changes
        private void NameChanged(string value) { SetText(value); }

        #region LISTENERS
        //start and stop tracking on enable
        private void OnEnable()
        {
            NameChanged(_playerName.CurrentValue);
            _playerName.Subscribe(NameChanged);
        }

        private void OnDisable() { _playerName.UnSubscribe(NameChanged); }
        #endregion
    }
}
