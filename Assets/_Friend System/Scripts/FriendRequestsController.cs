using UnityEngine;

namespace Gabe.FriendSystem {

    public class FriendRequestsController : MonoBehaviour {

        #region Public Fields

        #endregion


        #region Private Serializable Fields

        [SerializeField]
        private GameObject _scrollView;
        [SerializeField]
        private Transform _scrollViewContent;
        [SerializeField]
        private GameObject _incomingRequestTemplate, _outgoingRequestTemplate, _incomingSeparator, _outgoingSeparator;

        #endregion


        #region Private Fields
        #endregion


        #region MonoBehaviour CallBacks

        #endregion


        #region Private Methods

        #endregion


        #region Public Methods
        public void LoadFriendRequestList() {

            var localPlayer = GameManager.Instance.LocalPlayerData;

            for (int i = 0; i < _scrollViewContent.childCount; i++) {
                var item = _scrollViewContent.GetChild(i);
                Destroy(item.gameObject);
            }

            //Instantiate an info banner
            Instantiate(_incomingSeparator, _scrollViewContent);

            //Create a list of Incoming Requests
            var listOfIncomingFriendRequests = localPlayer.ListOfIncomingFriendRequests;
            foreach (var item in listOfIncomingFriendRequests) {
                var friendInfoGO = Instantiate(_incomingRequestTemplate, _scrollViewContent);
                friendInfoGO.GetComponent<FriendInfoTemplate>().Initialize(item);
            }

            //Instantiate an info banner
            Instantiate(_outgoingSeparator, _scrollViewContent);

            //Create a list of Outgoing Requests
            var listOfOutgoingFriendRequests = localPlayer.ListOfOutgoingFriendRequests;

            foreach (var item in listOfOutgoingFriendRequests) {
                var friendInfoGO = Instantiate(_outgoingRequestTemplate, _scrollViewContent);
                friendInfoGO.GetComponent<FriendInfoTemplate>().Initialize(item);
            }
        }
        #endregion
    }
}