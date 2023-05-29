using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;


namespace Gabe.FriendSystem {

    public class SearchFriendsController : MonoBehaviour {

        #region Public Fields

        #endregion


        #region Private Serializable Fields

        [SerializeField]
        private GameObject _scrollView;
        [SerializeField]
        private Transform _scrollViewContent;
        [SerializeField]
        private GameObject _friendTemplate;
        [SerializeField]
        private TMP_InputField _inputFriendName;
        [SerializeField]
        private TextMeshProUGUI _textInfo;
        [SerializeField]
        private Button _buttonSearch;

        #endregion


        #region Private Fields
        private FriendSystemControls _friendSystemControls;
        #endregion


        #region MonoBehaviour CallBacks

        private void OnEnable() {
            _friendSystemControls.Enable();
        }
        private void OnDisable() {
            _friendSystemControls.Disable();
        }

        void Awake() {
            _friendSystemControls = new();
            _friendSystemControls.UI.Submit.started += _ => SearchForFriends();
            _buttonSearch.onClick.AddListener(() => SearchForFriends());
        }

        void Start() {
            _textInfo.text = "";
            _inputFriendName.Select();
        }
        #endregion


        #region Private Methods

        //After a search result is returned from FirebaseController, this method creates a list of results
        private void CreateListOfResults() {

            var localPlayer = GameManager.Instance.LocalPlayerData;

            //destroy all entries, but don't destroy the text which is at index 0
            for (int i = 1; i < _scrollViewContent.childCount; i++) {
                var item = _scrollViewContent.GetChild(i);
                Destroy(item.gameObject);
            }

            var listOfResults = FirebaseController.Instance.ListSearchResults;
            if (listOfResults.Count == 0) {
                _textInfo.text = "No results found";
                return;
            }

            int numValidResults = 0;
            foreach (var item in listOfResults) {

                //We are going to display a given result only if I'm not blocked by that player, and it's not already in my friend or block list
                if (!item.ListOfBlockedPlayersDocRefs.Contains(GameManager.Instance.LocalPlayerData.DocRef) &&
                    !localPlayer.ListOfFriendsDocRefs.Contains(item.DocRef) &&
                    !localPlayer.ListOfOutgoingFriendRequestsDocRefs.Contains(item.DocRef) &&
                    !localPlayer.ListOfBlockedPlayersDocRefs.Contains(item.DocRef)) {
                    var friendInfoGO = Instantiate(_friendTemplate, _scrollViewContent);
                    friendInfoGO.GetComponent<FriendInfoTemplate>().Initialize(item);
                    numValidResults++;
                }
            }
            _textInfo.text = numValidResults == 0 ? "No results found" : $"{numValidResults} results found";
        }
        #endregion

        #region Public Methods
        public async void SearchForFriends() {

            if (_inputFriendName.text.Length == 0) {
                _textInfo.text = "Please type at least 1 letter";
                _inputFriendName.Select();
                return;
            }

            _textInfo.text = "Searching for players...";

            await FirebaseController.Instance.SearchForPlayersByNameOrNicknameAsync(_inputFriendName.text.ToLower());
            CreateListOfResults();
        }
        #endregion
    }
}