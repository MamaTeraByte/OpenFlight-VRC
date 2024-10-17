﻿using UdonSharp;
using UnityEngine;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.SDK3.Data;
using VRC.Udon;
using VRC.SDK3.Components;

namespace OpenFlightVRC.UI
{
    public enum PlayerUIDropdownCallback
	{
        ValueChanged
	}

    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    public class PlayerUIDropdown : CallbackUdonSharpBehaviour
    {
        public TMPro.TMP_Dropdown dropdown;
        public VRCPlayerApi selectedPlayer;
        void Start()
        {
            //make sure the dropdown is empty
            dropdown.ClearOptions();
        }

        public void OnValueChanged()
        {
            //get the player
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            selectedPlayer = players[dropdown.value];
            RunCallback(PlayerUIDropdownCallback.ValueChanged);
        }

        //on player join, rebuild the dropdown and reselect
        public override void OnPlayerJoined(VRCPlayerApi player)
        {
            string[] displayName = new string[1];
            displayName[0] = player.displayName;
            dropdown.AddOptions(displayName);
        }

        public override void OnPlayerLeft(VRCPlayerApi player)
        {
            //save the current player list and value
            //this works as the player array isnt updated until *after* this callback is called
            int value = dropdown.value;
            VRCPlayerApi[] players = new VRCPlayerApi[VRCPlayerApi.GetPlayerCount()];
            VRCPlayerApi.GetPlayers(players);
            VRCPlayerApi currentPlayer = players[value];

            //we need to remove the player from the list of players
            DataList cleanPlayersList = new DataList();
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i] != player)
                {
                    cleanPlayersList.Add(new DataToken(players[i].playerId));
                }
            }

            dropdown.ClearOptions();

            for (int i = 0; i < cleanPlayersList.Count; i++)
            {
                //get the current player
                VRCPlayerApi optionPlayer = VRCPlayerApi.GetPlayerById(cleanPlayersList[i].Int);
                string[] displayName = new string[1];
                displayName[0] = optionPlayer.displayName;
                dropdown.AddOptions(displayName);
            }

            //we need to re find the index of the current player
            int newIndex = cleanPlayersList.IndexOf(new DataToken(currentPlayer.playerId));
            //if its -1, then the player left was the current player. We need to set the value to ourselves
            if (newIndex == -1)
            {
                newIndex = cleanPlayersList.IndexOf(new DataToken(Networking.LocalPlayer.playerId));
                dropdown.value = newIndex;
            }
            else
            {
                dropdown.SetValueWithoutNotify(newIndex);
            }
        }
    }
}