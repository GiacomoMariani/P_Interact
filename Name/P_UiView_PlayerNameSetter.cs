using JReact;
using JReact.Playfab_Interact.PlayerName;
using JReact.UiView;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;

namespace JReact.Playfab_Interact
{
    /// <summary>
    /// this is used to set the player name
    /// </summary>
    public class P_UiView_PlayerNameSetter : J_UiView_ButtonElement
    {
        #region FIELDS AND PROPERTIES
        //the place where to get the name
        [BoxGroup("State", true, true, 5), Required, SerializeField] private TextMeshProUGUI _nameText;

        //the name controls
        [BoxGroup("State", true, true, 5), Required, SerializeField, AssetsOnly] private P_PlayerName _playerName;
        #endregion

        //save the name when requested
        protected override void ButtonCommand() { _playerName.SetPlayerName(_nameText.text); }
    }
}
