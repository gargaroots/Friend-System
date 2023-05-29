using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Extensions;
using Unity.VisualScripting;
using System;

public class FriendListController : MonoBehaviour {

    #region Public Fields

    #endregion


    #region Private Serializable Fields
    [SerializeField]
    private GameObject _scrollView;
    [SerializeField]
    private Transform _scrollViewContent;
    [SerializeField]
    private GameObject _friendTemplate, _friendsSeparator, _blockedPlayerSeparator, _blockedPlayerTemplate;

    #endregion


    #region Private Fields

    private List<FriendInfoTemplate> _listOfFriendButtons = new();
    #endregion


    #region MonoBehaviour CallBacks
    #endregion


    #region Private Methods

    //Load both the list of friends, and blocked friends
    private void LoadListOfFriends() {

        Instantiate(_friendsSeparator, _scrollViewContent);
        var listOfFriends = GameManager.Instance.LocalPlayerData.ListOfFriends;

        foreach (var friend in listOfFriends) {
            var friendInfoGO = Instantiate(_friendTemplate, _scrollViewContent);
            var friendInfoTemplate = friendInfoGO.GetComponent<FriendInfoTemplate>();
            friendInfoTemplate.Initialize(friend, RefreshListOfFriends);

            _listOfFriendButtons.Add(friendInfoTemplate);
        }

        Instantiate(_blockedPlayerSeparator, _scrollViewContent);

        var listOfBlocks = GameManager.Instance.LocalPlayerData.ListOfBlocked;

        foreach (var block in listOfBlocks) {
            var blockInfoGO = Instantiate(_blockedPlayerTemplate, _scrollViewContent);
            var blockInfoTemplate = blockInfoGO.GetComponent<FriendInfoTemplate>();
            blockInfoTemplate.Initialize(block, RefreshListOfFriends);

            _listOfFriendButtons.Add(blockInfoTemplate);
        }
    }

    //Checks for any unread chats, if there's any, show the New Message Badge
    private void CheckForUnreadChats(List<ChatDataStructure> listChats_) {
        var localPlayerNickname = GameManager.Instance.LocalPlayerData.Nickname;

        //These are ugly deeply nested foreachs, but it has breaks to avoid unnecessary loops
        foreach(var friendButton in _listOfFriendButtons) { 
            foreach(var chatData in listChats_) {
                if (chatData.ListOfParticipantsDocRefs.Contains(friendButton.FriendData.DocRef)){
                    foreach(var message in chatData.ListOfMessages) {

                        //if the message sender is not Local Player
                        if (message["sender"].ToString() != localPlayerNickname) {
                            //if Local Player did not read the message yet
                            if (!(bool)message["readByRecipient"]) {
                                ShowNewMessageBadge(friendButton);
                                break;
                            }
                        }
                    }
                    break;
                }
            }
        }
    }
    #endregion


    #region Public Methods

    public void RefreshListOfFriends() {

        var playerData = GameManager.Instance.LocalPlayerData;

        _listOfFriendButtons.Clear();
        foreach (Transform friendInfo in _scrollViewContent) {
            Destroy(friendInfo.gameObject);
        }

        LoadListOfFriends();
        CheckForUnreadChats(playerData.ListOfChats);
    }

    public void ShowNewMessageBadge(ChatDataStructure chatData_) {

        foreach(var friend in _listOfFriendButtons) {
            if(chatData_.ListOfParticipantsDocRefs.Contains(friend.FriendData.DocRef)) {
                friend.ShowNewChatBadge(true);
                break;
            }
        }
    }

    public void ShowNewMessageBadge(FriendInfoTemplate friendButton_) {
        friendButton_.ShowNewChatBadge(true);
    }
    #endregion
}
