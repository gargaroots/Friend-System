using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;


namespace Gabe.FriendSystem {

    public class FriendInfoTemplate : MonoBehaviour {

        #region Public Fields
        public PlayerDataStructure FriendData { get => _friendData; private set { } }
        #endregion

        #region Private Serializable Fields

        [SerializeField]
        private TextMeshProUGUI _textPlayerNickname, _textLastSeen;
        [SerializeField]
        private GameObject _onlineStatusLight, _offlineStatusLight, _iconNewMessage;
        [SerializeField]
        private Button _buttonChat, _buttonBlock, _buttonUnfriend, _buttonFriend, _buttonCancelRequest, _buttonApproveRequest, _buttonRejectRequest, _buttonUnblock;

        #endregion


        #region Private Fields

        private PlayerDataStructure _friendData;
        private UnityEvent _evtReloadFriendList = new();

        #endregion


        #region MonoBehaviour CallBacks

        private void OnEnable() {
            _buttonChat?.onClick.AddListener(LoadChat);
            _buttonCancelRequest?.onClick.AddListener(CancelFriendRequest);
            _buttonApproveRequest?.onClick.AddListener(ApproveFriendRequest);
            _buttonFriend?.onClick.AddListener(SendFriendRequest);
            _buttonRejectRequest?.onClick.AddListener(RejectFriendRequest);
            _buttonBlock?.onClick.AddListener(BlockFriend);
            _buttonUnblock?.onClick.AddListener(UnblockFriend);
            _buttonUnfriend?.onClick.AddListener(UnfriendFriend);
        }

        private void OnDisable() {
            _buttonChat?.onClick.RemoveAllListeners();
            _buttonCancelRequest?.onClick.RemoveAllListeners();
            _buttonApproveRequest?.onClick.RemoveAllListeners();
            _buttonFriend?.onClick.RemoveAllListeners();
            _buttonRejectRequest?.onClick.RemoveAllListeners();
            _buttonBlock?.onClick.RemoveAllListeners();
            _buttonUnblock?.onClick.RemoveAllListeners();
            _buttonUnfriend?.onClick.RemoveAllListeners();
        }
        #endregion


        #region Private Methods

        //This method looks for a chat that has the friend as a participant, if it can't find, opens a new chat
        private void LoadChat() {
            _iconNewMessage.SetActive(false);

            var localPlayer = GameManager.Instance.LocalPlayerData;

            //Check the list of chats and load it, there is only 1 chat per friend, ever
            foreach (var chat in localPlayer.ListOfChats) {
                if (chat.ListOfParticipantsDocRefs.Contains(_friendData.DocRef)) {
                    GUIController.Instance.ShowChat(chat);
                    return;
                }
            }

            //if we got here, there is no chat, open an empty window
            GUIController.Instance.ShowEmptyChat(_friendData.DocRef);
        }


        private void CancelFriendRequest() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfOutgoingFriendRequestsDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfOutgoingFriendRequests.Remove(_friendData);
            Dictionary<string, object> dict = new() {
            { "outgoingFriendRequests", localPlayer.ListOfOutgoingFriendRequestsDocRefs }
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            //Update Friend Data in Firebase
            _friendData.ListOfIncomingFriendRequestsDocRefs.Remove(localPlayer.DocRef);
            dict = new() {
            { "incomingFriendRequests", _friendData.ListOfIncomingFriendRequestsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(_friendData.DocRef, dict);

            Destroy(gameObject);
        }

        private void ApproveFriendRequest() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfIncomingFriendRequestsDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfIncomingFriendRequests.Remove(_friendData);
            localPlayer.ListOfFriendsDocRefs.Add(_friendData.DocRef);

            Dictionary<string, object> dict = new() {
            { "incomingFriendRequests", localPlayer.ListOfIncomingFriendRequestsDocRefs },
            { "friends", localPlayer.ListOfFriendsDocRefs }
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            //Update friend's firestore data
            _friendData.ListOfOutgoingFriendRequestsDocRefs.Remove(localPlayer.DocRef);
            _friendData.ListOfFriendsDocRefs.Add(localPlayer.DocRef);
            dict = new() {
            { "outgoingFriendRequests", _friendData.ListOfOutgoingFriendRequestsDocRefs },
            { "friends", _friendData.ListOfFriendsDocRefs }
        };
            FirebaseController.Instance.UpdateFirebaseDoc(_friendData.DocRef, dict);

            Destroy(gameObject);
        }

        private void RejectFriendRequest() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfIncomingFriendRequestsDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfIncomingFriendRequests.Remove(_friendData);

            Dictionary<string, object> dict = new() {
            { "incomingFriendRequests", localPlayer.ListOfIncomingFriendRequestsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            //Update friend's firestore data
            _friendData.ListOfOutgoingFriendRequestsDocRefs.Remove(localPlayer.DocRef);
            dict = new() {
            { "outgoingFriendRequests", _friendData.ListOfOutgoingFriendRequestsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(_friendData.DocRef, dict);

            Destroy(gameObject);
        }

        private void SendFriendRequest() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfOutgoingFriendRequestsDocRefs.Add(_friendData.DocRef);
            localPlayer.ListOfOutgoingFriendRequests.Add(_friendData);

            Dictionary<string, object> dict = new() {
            { "outgoingFriendRequests", localPlayer.ListOfOutgoingFriendRequestsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            //Update friend's firestore data
            _friendData.ListOfIncomingFriendRequestsDocRefs.Add(localPlayer.DocRef);
            dict = new() {
            { "incomingFriendRequests", _friendData.ListOfIncomingFriendRequestsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(_friendData.DocRef, dict);
            Destroy(gameObject);
        }

        private void BlockFriend() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfFriendsDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfFriends.Remove(_friendData);
            localPlayer.ListOfBlockedPlayersDocRefs.Add(_friendData.DocRef);
            localPlayer.ListOfBlocked.Add(_friendData);

            Dictionary<string, object> dict = new() {
            { "friends", localPlayer.ListOfFriendsDocRefs},
            { "blocked", localPlayer.ListOfBlockedPlayersDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            _evtReloadFriendList.Invoke();

            Destroy(gameObject);
        }

        private void UnblockFriend() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfBlockedPlayersDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfBlocked.Remove(_friendData);

            Dictionary<string, object> dict = new() {
            { "blocked", localPlayer.ListOfBlockedPlayersDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            Destroy(gameObject);
        }

        private void UnfriendFriend() {

            //Update Local Player Data in Firebase
            var localPlayer = GameManager.Instance.LocalPlayerData;
            localPlayer.ListOfFriendsDocRefs.Remove(_friendData.DocRef);
            localPlayer.ListOfFriends.Remove(_friendData);

            Dictionary<string, object> dict = new() {
            { "friends", localPlayer.ListOfFriendsDocRefs },
        };
            FirebaseController.Instance.UpdateFirebaseDoc(localPlayer.DocRef, dict);

            Destroy(gameObject);
        }


        #endregion

        #region Public Methods

        public void Initialize(PlayerDataStructure friendData_, UnityAction reloadListCallback_ = null) {

            _friendData = friendData_;

            _onlineStatusLight.SetActive(_friendData.IsOnline);
            _offlineStatusLight.SetActive(!_friendData.IsOnline);

            _textLastSeen.text = _friendData.LastSeen;
            _textPlayerNickname.text = _friendData.Nickname;

            if (reloadListCallback_ != null) {
                _evtReloadFriendList.AddListener(reloadListCallback_);
            }
        }

        public void ShowNewChatBadge(bool show_) {
            _iconNewMessage.SetActive(show_);
        }

        #endregion
    }
}