using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using Firebase.Firestore;


namespace Gabe.FriendSystem {

    public class GUIController : MonoBehaviour {

        #region Public Fields
        public static GUIController Instance;
        #endregion


        #region Private Serializable Fields

        [SerializeField]
        private GameObject _loginScreen, _leftMenuMain, _leftMenuFriends, _friendsScreen, _searchPlayersScreen, _loadingScreen, _friendRequestsScreen, _resetDatabaseConfirmationWindow;
        [SerializeField]
        private FriendListController _friendListController;
        [SerializeField]
        private SearchFriendsController _searchFriendsController;
        [SerializeField]
        private FriendRequestsController _friendRequestsController;
        [SerializeField]
        private Button _buttonOpenFriendsMenu, _buttonFriendList, _buttonSearchPlayers, _buttonCloseFriendsMenu, _buttonFriendRequests, _buttonLogOut, _buttonResetDatabase, _buttonResetDatabaseYes, _buttonQuit;
        [SerializeField]
        private Button _buttonLoginGabe, _buttonLoginJack, _buttonLoginPaul, _buttonLoginFred, _buttonLoginCarol;
        [SerializeField]
        private ChatPanelController _chatPanelController;
        [SerializeField]
        private TextMeshProUGUI _textLoggedInAs;

        #endregion


        #region Private Fields
        private FriendSystemControls _friendSystemControls;
        #endregion

        #region MonoBehaviour CallBacks

        private void OnEnable() {

            _friendSystemControls.Enable();

            _buttonOpenFriendsMenu.onClick.AddListener(OpenFriendsMenu);
            _buttonCloseFriendsMenu.onClick.AddListener(CloseFriendsMenu);
            _buttonFriendList.onClick.AddListener(OpenFriendsList);
            _buttonSearchPlayers.onClick.AddListener(OpenSearchForPlayers);
            _buttonFriendRequests.onClick.AddListener(OpenFriendRequestScreen);
            _buttonLogOut.onClick.AddListener(Logout);
            _buttonQuit.onClick.AddListener(QuitApp);
            _buttonLoginGabe.onClick.AddListener(() => LoginAs("gabe"));
            _buttonLoginJack.onClick.AddListener(() => LoginAs("jack"));
            _buttonLoginPaul.onClick.AddListener(() => LoginAs("paul"));
            _buttonLoginFred.onClick.AddListener(() => LoginAs("fred"));
            _buttonLoginCarol.onClick.AddListener(() => LoginAs("carol"));
            _buttonResetDatabase.onClick.AddListener(ResetDatabase);
            _buttonResetDatabaseYes.onClick.AddListener(ResetDatabaseConfirmed);
        }

        private void OnDisable() {
            _friendSystemControls.Disable();

            _buttonOpenFriendsMenu.onClick.RemoveAllListeners();
            _buttonCloseFriendsMenu.onClick.RemoveAllListeners();
            _buttonFriendList.onClick.RemoveAllListeners();
            _buttonSearchPlayers.onClick.RemoveAllListeners();
            _buttonFriendRequests.onClick.RemoveAllListeners();
            _buttonLogOut.onClick.RemoveAllListeners();
            _buttonQuit.onClick.RemoveAllListeners();
            _buttonLoginGabe.onClick.RemoveAllListeners();
            _buttonLoginJack.onClick.RemoveAllListeners();
            _buttonLoginPaul.onClick.RemoveAllListeners();
            _buttonLoginFred.onClick.RemoveAllListeners();
            _buttonLoginCarol.onClick.RemoveAllListeners();
            _buttonResetDatabase.onClick.RemoveAllListeners();
            _buttonResetDatabaseYes.onClick.RemoveAllListeners();
        }

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }

            _friendSystemControls = new();
        }

        IEnumerator Start() {

            _friendSystemControls.UI.Cancel.started += _ => CloseFriendsMenu();
            _loginScreen.SetActive(true);
            _loadingScreen.SetActive(false);
            _leftMenuMain.SetActive(true);
            _leftMenuFriends.SetActive(false);
            _textLoggedInAs.text = $"Logged out";

            //Wait for the GameManager to be "ready"
            yield return new WaitUntil(() => GameManager.Instance.IsReady);
            FirebaseController.Instance.ListenForNewChats(NewChatArrived);
        }
        #endregion


        #region Private Methods

        private void LoginAs(string nickname_) {
            _textLoggedInAs.text = $"Logged in as {nickname_}";
            GameManager.Instance.OnPlayerLoggedIn(nickname_);
        }

        private void Logout() {
            _textLoggedInAs.text = $"Logged out";
            GameManager.Instance.Logout();
        }

        private void QuitApp() {
            GameManager.Instance.QuitApp();
        }

        private void OpenFriendsMenu() {
            _leftMenuMain.SetActive(false);
            _leftMenuFriends.SetActive(true);

            _buttonFriendList.Select(); //just for looks
            OpenFriendsList();
        }
        private void CloseFriendsMenu() {
            _leftMenuFriends.SetActive(false);
            _leftMenuMain.SetActive(true);
            _searchPlayersScreen.SetActive(false);
            _friendsScreen.SetActive(false);
            _friendRequestsScreen.SetActive(false);
            _chatPanelController.gameObject.SetActive(false);
        }

        private void OpenFriendsList() {
            _searchPlayersScreen.SetActive(false);
            _friendRequestsScreen.SetActive(false);
            _friendsScreen.SetActive(true);
            _friendListController.RefreshListOfFriends();
        }
        private void OpenSearchForPlayers() {
            _searchPlayersScreen.SetActive(true);
            _friendsScreen.SetActive(false);
            _friendRequestsScreen.SetActive(false);
        }

        private void OpenFriendRequestScreen() {
            _friendRequestsScreen.SetActive(true);
            _searchPlayersScreen.SetActive(false);
            _friendsScreen.SetActive(false);
            _friendRequestsController.LoadFriendRequestList();
        }

        private void NewChatArrived(ChatDataStructure chatData_) {
            _friendListController.ShowNewMessageBadge(chatData_);
        }
        private void ResetDatabase() {
            _resetDatabaseConfirmationWindow.SetActive(true);
        }

        private async void ResetDatabaseConfirmed() {
            await FirebaseController.Instance.ResetFirebaseData();
            GameManager.Instance.Logout();
        }

        #endregion


        #region Public Methods
        public void ShowLoadingScreen(bool loading_) {
            _loadingScreen.SetActive(loading_);
        }

        public void ShowChat(ChatDataStructure listChat_) {
            _chatPanelController.gameObject.SetActive(true);
            _chatPanelController.LoadChat(listChat_);
        }

        public void ShowEmptyChat(DocumentReference recipientDocRef_) {
            _chatPanelController.gameObject.SetActive(true);
            _chatPanelController.LoadNewChat(recipientDocRef_);
        }

        public void HideChat() {
            _chatPanelController.gameObject.SetActive(false);
        }
        #endregion
    }
}