using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.InputSystem;
using Firebase.Firestore;
using System.Linq;

namespace Gabe.FriendSystem {

    public class ChatPanelController : MonoBehaviour {

        #region Public Fields

        #endregion

        #region Private Serializable Fields

        [SerializeField]
        private Button _buttonSendMessage;
        [SerializeField]
        private TMP_InputField _inputPlayerMessage, _inputMessages;
        #endregion

        #region Private Fields
        private ChatDataStructure _currentChatData;
        private FriendSystemControls _friendSystemControls;
        private DocumentReference _newChatRecipientDocRef;
        #endregion


        #region MonoBehaviour CallBacks
        void Awake() {
            _friendSystemControls = new();
            _friendSystemControls.Enable();
        }

        void Start() {
            _buttonSendMessage.onClick.AddListener(SendMessage);
            _friendSystemControls.UI.Submit.started += _ => SendMessage();
            FirebaseController.Instance.ListenForNewChats(NewChatArrived);
        }

        #endregion


        #region Private Methods
        private async void SendMessage() {

            //Avoid sending empty messages
            if (_inputPlayerMessage.text == "" || _inputPlayerMessage.text == "\n") {
                return;
            }



            var localPlayer = GameManager.Instance.LocalPlayerData;
            Dictionary<string, object> messageData = new() {
            { "sender",  localPlayer.Nickname },
            { "text", _inputPlayerMessage.text },
            { "timestamp", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") },
            { "readByRecipient", false}
        };
            _currentChatData.ListOfMessages.Add(messageData);

            //check if it's a new chat, if so, add Local Player and Friend to the list of participants
            if (_newChatRecipientDocRef != null && _currentChatData.ListOfParticipantsDocRefs.Count == 0) {
                _currentChatData.ListOfParticipantsDocRefs.Add(localPlayer.DocRef);
                _currentChatData.ListOfParticipantsDocRefs.Add(_newChatRecipientDocRef);
            }

            Dictionary<string, object> data = new() {
            { "messages", _currentChatData.ListOfMessages },
            { "participants", _currentChatData.ListOfParticipantsDocRefs }
        };
            DocumentReference docRef = null;
            if (_currentChatData.DocRef == null) {
                //it's a new chat, use the AddNewChat method
                docRef = await FirebaseController.Instance.AddNewChat(data);
                _currentChatData.DocRef = docRef;

                //now update Local Player's list of chats in Firebase
                var listOfChats = localPlayer.ListOfChatsDocRefs;
                listOfChats.Add(docRef);
                Dictionary<string, object> chat = new() {
                { "chats", listOfChats }
            };
                FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, chat);


                //Update the other player's list of chats in Firebase
                var otherParticipantDocRef = _currentChatData.ListOfParticipantsDocRefs.First(x => x != localPlayer.DocRef);
                var friendData = localPlayer.ListOfFriends.First(x => x.DocRef == otherParticipantDocRef);

                var friendListOfChats = friendData.ListOfChatsDocRefs;
                friendListOfChats.Add(docRef);
                chat = new() {
                { "chats", friendListOfChats }
            };
                FirebaseController.Instance.UpdateFirebaseDoc(otherParticipantDocRef, chat);
            }
            else {
                FirebaseController.Instance.UpdateFirebaseDoc(_currentChatData.DocRef, data);
            }

            _inputPlayerMessage.text = "";
            LoadChat(_currentChatData);
        }

        private void NewChatArrived(ChatDataStructure newChat_) {

            //if we received a new chat and it's exactly the one that is currently open, just load the message
            if (newChat_.DocRef == _currentChatData.DocRef) {
                LoadChat(newChat_);
            }
            else {
                //display a new message icon
                //this is done by the GUIController
            }
        }


        #endregion

        #region Public Methods

        //this method reads the list of messages from a Chat and displays it
        //It also paints local player's messages grey, just for aesthetics
        public void LoadChat(ChatDataStructure chatData_) {

            _inputMessages.text = "";
            _currentChatData = chatData_;

            var localPlayerNickname = GameManager.Instance.LocalPlayerData.Nickname;

            foreach (var chat in _currentChatData.ListOfMessages) {
                var sender = chat["sender"].ToString();
                var timeStamp = chat["timestamp"].ToString();
                var date = DateTime.ParseExact(timeStamp.Replace("Timestamp: ", "").Trim(), "yyyy-MM-ddTHH:mm:ss.fffZ", null).ToShortDateString() + " " +
                    DateTime.ParseExact(timeStamp.Replace("Timestamp: ", "").Trim(), "yyyy-MM-ddTHH:mm:ss.fffZ", null).ToShortTimeString();
                var text = chat["text"].ToString();

                //If this is a Local Player's message, paint it grey
                string message = "";
                if (sender == localPlayerNickname) {
                    message = $"<color=grey>{date} - {sender}: {text}</color>";
                }
                else {
                    message = $"{date} - {sender}: {text}";
                }
                _inputMessages.text += $"\n{message}";

                //if the sender is not Local Player, mark the message as read in Firebase
                var hasChanges = false;
                if (sender != localPlayerNickname) {
                    if (!(bool)chat["readByRecipient"]) {
                        chat["readByRecipient"] = true;
                        hasChanges = true;
                    }
                }
                if (hasChanges) {
                    Dictionary<string, object> data = new() {
                    { "messages", _currentChatData.ListOfMessages }
                };
                    FirebaseController.Instance.UpdateFirebaseDoc(_currentChatData.DocRef, data);
                }
            }
        }

        //Simply displays a blank chat window
        public void LoadNewChat(DocumentReference recipientDocRef_) {
            _newChatRecipientDocRef = recipientDocRef_;
            _currentChatData = new() {
                DocRef = null,
                ListOfMessages = new(),
                ListOfParticipantsDocRefs = new()
            };

            _inputMessages.text = "";
            _inputPlayerMessage.text = "";
        }
        #endregion
    }
}