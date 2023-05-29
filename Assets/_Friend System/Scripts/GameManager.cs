using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Gabe.FriendSystem {

    public class GameManager : MonoBehaviour {

        #region Public Fields

        public static GameManager Instance;
        public PlayerDataStructure LocalPlayerData { get => FirebaseController.Instance.PlayerData; private set { } }
        public bool IsReady { get => _isReady; private set { } }


        #endregion


        #region Private Serializable Fields

        #endregion


        #region Private Fields

        private bool _isReady = false;

        #endregion


        #region MonoBehaviour CallBacks 
        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
            else {
                Destroy(gameObject);
                return;
            }
        }
        #endregion


        #region Private Methods

        #endregion


        #region Public Methods

        public void QuitApp() {
            FirebaseController.Instance.StopListeningLocalPlayerDataChanges();
            FirebaseController.Instance.SetLocalPlayerOnlineStatusAndLastSeen(false);
            Application.Quit();
        }

        public async void OnPlayerLoggedIn(string playerNickname_) {

            GUIController.Instance.ShowLoadingScreen(true);

            await FirebaseController.Instance.SignInWithEmailAndPassword(playerNickname_);

            await Task.Delay(1000);

            FirebaseController.Instance.ListenLocalPlayerDataChanges(playerNickname_);

            await Task.Delay(1000); // Display the Loading Screen for 1 second, to add some drama :)

            GUIController.Instance.ShowLoadingScreen(false);
            _isReady = true;

            FirebaseController.Instance.SetLocalPlayerOnlineStatusAndLastSeen(true);
        }

        public void Logout() {
            FirebaseController.Instance.StopListeningLocalPlayerDataChanges();
            FirebaseController.Instance.SetLocalPlayerOnlineStatusAndLastSeen(false);
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        #endregion
    }
}