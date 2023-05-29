using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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

        Debug.Log("0");
        GUIController.Instance.ShowLoadingScreen(true);
        Debug.Log("0");

        await FirebaseController.Instance.SignInWithEmailAndPassword(playerNickname_);
        Debug.Log("0");

        await Task.Delay(1000);

        FirebaseController.Instance.ListenLocalPlayerDataChanges(playerNickname_);
        Debug.Log("0");

        await Task.Delay(1000); // Display the Loading Screen for 1 second, to add some drama :)
        Debug.Log("0");

        GUIController.Instance.ShowLoadingScreen(false);
        Debug.Log("0");
        _isReady = true;
        Debug.Log("0");
        FirebaseController.Instance.SetLocalPlayerOnlineStatusAndLastSeen(true);
    }

    public void Logout() {
        FirebaseController.Instance.StopListeningLocalPlayerDataChanges();
        FirebaseController.Instance.SetLocalPlayerOnlineStatusAndLastSeen(false);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    #endregion
}