using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Firestore;
using Firebase.Extensions;
using Unity.VisualScripting;
using System.Threading.Tasks;
using System;
using UnityEngine.Events;
using Firebase.Auth;

public class FirebaseController : MonoBehaviour {

    #region Public Fields

    public static FirebaseController Instance;
    public PlayerDataStructure PlayerData { get => _playerData; private set { } }
    public List<PlayerDataStructure> ListSearchResults { get; private set; } = new();

    #endregion


    #region Private Serializable Fields

    #endregion


    #region Private Fields
    private FirebaseFirestore _db;
    private FirebaseAuth _auth;
    private PlayerDataStructure _playerData = new();
    private UnityEvent<ChatDataStructure> _evtNewChat = new();
    private bool _searchRunning = false;
    private ListenerRegistration _localPlayerDataListener;
    #endregion


    #region MonoBehaviour CallBacks 
    void Awake(){
        if(Instance == null){
            Instance = this;
        }
        else{
            Destroy(gameObject);
            return;
        }

        _db = FirebaseFirestore.DefaultInstance;
        _auth = FirebaseAuth.DefaultInstance;
    }

    #endregion


    #region Private Methods

    

    //Get data like Docref, name, status for all players blocked by the Local Player
    private async void GetLocalPlayerBlockedPlayersData() {
        _playerData.ListOfBlocked.Clear();

        //for each friend I have in my list, get their data
        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfBlockedPlayersDocRefs.Count; i++) {
            var docRef = _playerData.ListOfBlockedPlayersDocRefs[i];
            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists) {
                    Dictionary<string, object> blockData = snapshot.ToDictionary();

                    PlayerDataStructure blockedPlayer = new() {
                        Name = blockData["name"].ToString(),
                        Nickname = blockData["nickname"].ToString(),
                        IsOnline = (bool)blockData["isOnline"],
                        DocRef = docRef,
                        LastSeen = (bool)blockData["isOnline"] ? "Online" : CalculateLastSeenOnline(blockData["lastSeen"].ToString()),
                    };
                    if (!_playerData.ListOfBlocked.Contains(blockedPlayer)) {
                        _playerData.ListOfBlocked.Add(blockedPlayer);
                    }
                }
            });
        }
    }

    //Get data like Docref, name, status for all Local Player's friends
    private async void GetLocalPlayerFriendsData() {
        _playerData.ListOfFriends.Clear();

        //for each friend I have in my list, get their data
        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfFriendsDocRefs.Count; i++) {
            var docRef = _playerData.ListOfFriendsDocRefs[i];

            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists) {
                    Dictionary<string, object> friendData = snapshot.ToDictionary();

                    PlayerDataStructure friend = new() {
                        Name = friendData["name"].ToString(),
                        Nickname = friendData["nickname"].ToString(),
                        IsOnline = (bool)friendData["isOnline"],
                        DocRef = docRef,
                        LastSeen = (bool)friendData["isOnline"] ? "Online" : CalculateLastSeenOnline(friendData["lastSeen"].ToString()),
                        ListOfBlockedPlayersDocRefs = friendData["blocked"].ConvertTo<List<DocumentReference>>(),
                        ListOfFriendsDocRefs = friendData["friends"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequestsDocRefs = friendData["incomingFriendRequests"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequests = new(),
                        ListOfOutgoingFriendRequestsDocRefs = friendData["outgoingFriendRequests"].ConvertTo<List<DocumentReference>>(),
                        ListOfOutgoingFriendRequests = new(),
                        ListOfChatsDocRefs = friendData["chats"].ConvertTo<List<DocumentReference>>()
                    };

                    //check if my friend hasn't blocked me
                    if (!friend.ListOfBlockedPlayersDocRefs.Contains(_playerData.DocRef)) {

                        //Yay! I'm not blocked
                        if (!_playerData.ListOfFriends.Contains(friend)) { //avoid duplicates, sometimes if happens
                            _playerData.ListOfFriends.Add(friend);
                        }
                    }
                }
            });
        }
    }

    //Get data like Docref, name, status for all Incoming Friend Requests
    private async void GetListOfIncomingRequestsData() {
        _playerData.ListOfIncomingFriendRequests.Clear();

        //for each incoming request I have in my list, get their data
        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfIncomingFriendRequestsDocRefs.Count; i++) {
            var docRef = _playerData.ListOfIncomingFriendRequestsDocRefs[i];

            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists) {
                    Dictionary<string, object> friendData = snapshot.ToDictionary();

                    PlayerDataStructure friend = new() {
                        Name = friendData["name"].ToString(),
                        Nickname = friendData["nickname"].ToString(),
                        IsOnline = (bool)friendData["isOnline"],
                        DocRef = docRef,
                        LastSeen = (bool)friendData["isOnline"] ? "Online" : CalculateLastSeenOnline(friendData["lastSeen"].ToString()),
                        ListOfBlockedPlayersDocRefs = friendData["blocked"].ConvertTo<List<DocumentReference>>(),
                        ListOfFriendsDocRefs = friendData["friends"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequestsDocRefs = new(),
                        ListOfOutgoingFriendRequestsDocRefs = new(),
                    };

                    if (friendData.ContainsKey("incomingFriendRequests")) {
                        friend.ListOfIncomingFriendRequestsDocRefs = friendData["incomingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    }
                    if (friendData.ContainsKey("outgoingFriendRequests")) {
                        friend.ListOfOutgoingFriendRequestsDocRefs = friendData["outgoingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    }
                    if (!_playerData.ListOfIncomingFriendRequests.Contains(friend)) {
                        _playerData.ListOfIncomingFriendRequests.Add(friend);
                    }
                }
            });
        }
    }

    //Get data like Docref, name, status for all Outgoing Friend Requests
    private async void GetListOfOutgoingRequestsData() {
        _playerData.ListOfOutgoingFriendRequests.Clear();

        //for each outgoing request I have in my list, get their data
        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfOutgoingFriendRequestsDocRefs.Count; i++) {
            var docRef = _playerData.ListOfOutgoingFriendRequestsDocRefs[i];
            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                DocumentSnapshot snapshot = task.Result;

                if (snapshot.Exists) {
                    Dictionary<string, object> friendData = snapshot.ToDictionary();

                    PlayerDataStructure friend = new() {
                        Name = friendData["name"].ToString(),
                        Nickname = friendData["nickname"].ToString(),
                        IsOnline = (bool)friendData["isOnline"],
                        DocRef = docRef,
                        LastSeen = (bool)friendData["isOnline"] ? "Online" : CalculateLastSeenOnline(friendData["lastSeen"].ToString()),
                        ListOfBlockedPlayersDocRefs = friendData["blocked"].ConvertTo<List<DocumentReference>>(),
                        ListOfFriendsDocRefs = friendData["friends"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequestsDocRefs = new(),
                        ListOfOutgoingFriendRequestsDocRefs = new(),
                    };

                    if (friendData.ContainsKey("incomingFriendRequests")) {
                        friend.ListOfIncomingFriendRequestsDocRefs = friendData["incomingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    }
                    if (friendData.ContainsKey("outgoingFriendRequests")) {
                        friend.ListOfOutgoingFriendRequestsDocRefs = friendData["outgoingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    }
                    if (!_playerData.ListOfOutgoingFriendRequests.Contains(friend)) {
                        _playerData.ListOfOutgoingFriendRequests.Add(friend);
                    }
                }
            });
        }
    }

    //Read all chats where the Local Player is a participant
    private async void GetListOfChatsData() {
        _playerData.ListOfChats.Clear();

        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfChatsDocRefs.Count; i++) {
            var docRef = _playerData.ListOfChatsDocRefs[i];
            await docRef.GetSnapshotAsync().ContinueWithOnMainThread(task => {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists) {
                    Dictionary<string, object> myData = snapshot.ToDictionary();

                    List<DocumentReference> listParticipants = myData["participants"].ConvertTo<List<DocumentReference>>();
                    List<Dictionary<string, object>> messages = myData["messages"].ConvertTo<List<Dictionary<string, object>>>();

                    ChatDataStructure chat = new() {
                        DocRef = docRef,
                        ListOfMessages = messages,
                        ListOfParticipantsDocRefs = listParticipants
                    };

                    _playerData.ListOfChats.Add(chat);
                }
            });
        }
    }
    #endregion


    #region Public Methods

    public async Task SignInWithEmailAndPassword(string nickname_) {

        string email = $"{nickname_}@test.com";
        string password = "123123";
        Debug.Log($"Logging in as {email}");

        await _auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            if (task.IsCanceled) {
                Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted) {
                Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                return;
            }

            AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    public void StopListeningLocalPlayerDataChanges() {
        Debug.Log("Stopped Listening for Data Changes");
        _localPlayerDataListener.Stop();

    }

    //Keep listening for any changes that might have occurred to player data, e.g. a new chat or friend request incoming
    public void ListenLocalPlayerDataChanges(string playerNickname_) {

        _playerData.Nickname = playerNickname_;

        //Allocate all Lists, some of them might be empty on Firestore and cause issues
        _playerData.ListOfOutgoingFriendRequestsDocRefs = new();
        _playerData.ListOfOutgoingFriendRequests = new();
        _playerData.ListOfIncomingFriendRequestsDocRefs = new();
        _playerData.ListOfIncomingFriendRequests = new();
        _playerData.ListOfFriendsDocRefs = new();
        _playerData.ListOfFriends = new();
        _playerData.ListOfChatsDocRefs = new();
        _playerData.ListOfChats = new();
        _playerData.ListOfBlockedPlayersDocRefs = new();
        _playerData.ListOfBlocked = new();

        var LocalPlayerDocRef = _db.Collection("players").Document(_playerData.Nickname);
        _playerData.DocRef = LocalPlayerDocRef;

        Debug.Log("Will Start Listening for Data Changes");
        _localPlayerDataListener = LocalPlayerDocRef.Listen(snapshot=> {
            if (snapshot.Exists) {
                Dictionary<string, object> myData = snapshot.ToDictionary();

                if (myData.ContainsKey("name")) {
                    _playerData.Name = myData["name"].ToString();
                }

                if (myData.ContainsKey("isOnline")) {
                    _playerData.IsOnline = (bool)myData["isOnline"];
                }

                if (myData.ContainsKey("lastSeen")) {
                    var fireStoreTimestamp = myData["lastSeen"].ToString();
                    _playerData.LastSeen = _playerData.IsOnline ? "Online" : CalculateLastSeenOnline(fireStoreTimestamp);
                }

                if (myData.ContainsKey("incomingFriendRequests")) {
                    _playerData.ListOfIncomingFriendRequestsDocRefs = myData["incomingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    GetListOfIncomingRequestsData();
                }

                if (myData.ContainsKey("outgoingFriendRequests")) {
                    _playerData.ListOfOutgoingFriendRequestsDocRefs = myData["outgoingFriendRequests"].ConvertTo<List<DocumentReference>>();
                    GetListOfOutgoingRequestsData();
                }
                if (myData.ContainsKey("blocked")) {
                    _playerData.ListOfBlockedPlayersDocRefs = myData["blocked"].ConvertTo<List<DocumentReference>>();
                    GetLocalPlayerBlockedPlayersData();
                }
                if (myData.ContainsKey("friends")) {
                    _playerData.ListOfFriendsDocRefs = myData["friends"].ConvertTo<List<DocumentReference>>();
                    GetLocalPlayerFriendsData();
                }
                if (myData.ContainsKey("chats")) {
                    _playerData.ListOfChatsDocRefs = myData["chats"].ConvertTo<List<DocumentReference>>();
                    GetListOfChatsData();
                }
            }
        });
        Debug.Log("Started Listening for Data Changes");
    }

    //Keep listening for new chats and Invoke an event so lists can auto-refresh
    public void ListenForNewChats(UnityAction<ChatDataStructure> callback_) {

        _evtNewChat.AddListener(callback_);
        //foreach(var chatDocRef in _playerData.ListOfChatsDocRefs) {
        //avoiding using foreach because the collection may get modified by other process
        for (int i = 0; i < _playerData.ListOfChatsDocRefs.Count; i++) {
            var chatDocRef = _playerData.ListOfChatsDocRefs[i];
            chatDocRef.Listen(snapshot => {
                if (snapshot.Exists) {
                    Dictionary<string, object> myData = snapshot.ToDictionary();
                    List<Dictionary<string, object>> messages = myData["messages"].ConvertTo<List<Dictionary<string, object>>>();

                    //update the list of chats with the new data
                    ChatDataStructure chat = new();
                    for (int i = 0; i < _playerData.ListOfChats.Count; i++) {
                        chat = _playerData.ListOfChats[i];
                        if (chat.DocRef == chatDocRef) {
                            if(chat.ListOfMessages.Count < messages.Count) { 
                                chat.ListOfMessages = messages;
                                _playerData.ListOfChats[i] = chat;
                                _evtNewChat.Invoke(chat);
                                break;
                            }
                        }
                    }
                }
            });
        }
    }

    //This just sets the local player's current online status, and last time seen online
    public void SetLocalPlayerOnlineStatusAndLastSeen(bool isOnline_) {
        var playerNickname = _playerData.Nickname;

        DocumentReference docRef = _db.Collection("players").Document(playerNickname);
        Dictionary<string, object> dataMap = new()
        {
            { "isOnline", isOnline_ },
            { "lastSeen", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ") }
        };

        docRef.UpdateAsync(dataMap);
    }

    
    //This is used by the Search function to search for players by name and/or nickname
    public async Task SearchForPlayersByNameOrNicknameAsync(string searchString_) {

        if (_searchRunning) {
            return;
        }

        _searchRunning = true;        
        CollectionReference allPlayersRef = _db.Collection("players"); //fetch all the players
        Query allPlayersQuery = _db.Collection("players");
        await allPlayersQuery.GetSnapshotAsync().ContinueWithOnMainThread(task =>
        {
            QuerySnapshot allPlayersQuerySnapshot = task.Result;

            ListSearchResults.Clear();
            foreach (DocumentSnapshot documentSnapshot in allPlayersQuerySnapshot.Documents) {
                Dictionary<string, object> document = documentSnapshot.ToDictionary();
                var playerName = document["name"].ToString();
                var playerNickname = document["nickname"].ToString();

                if (!playerName.Contains(_playerData.Name) && !playerName.Contains(_playerData.Nickname) && 
                    (playerName.Contains(searchString_) || playerNickname.Contains(searchString_))) {

                    var playerInfo = new PlayerDataStructure() {
                        DocRef = _db.Collection("players").Document(playerNickname),
                        Name = playerName,
                        Nickname = playerNickname,
                        IsOnline = (bool)document["isOnline"],
                        LastSeen = (bool)document["isOnline"] ? "Online" : CalculateLastSeenOnline(document["lastSeen"].ToString()),
                        ListOfBlockedPlayersDocRefs = document["blocked"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequestsDocRefs = document["incomingFriendRequests"].ConvertTo<List<DocumentReference>>(),
                        ListOfIncomingFriendRequests = new(),
                        ListOfOutgoingFriendRequestsDocRefs = document["outgoingFriendRequests"].ConvertTo<List<DocumentReference>>(),
                        ListOfOutgoingFriendRequests = new(),
                        ListOfChatsDocRefs = document["chats"].ConvertTo<List<DocumentReference>>()
                    };

                    //check if local player isn't blocked by the searched player
                    if (!playerInfo.ListOfBlockedPlayersDocRefs.Contains(_playerData.DocRef)) {
                        ListSearchResults.Add(playerInfo);
                    }
                };
            }
        });
        _searchRunning = false;
    }

    //used by many methods to send simple updates to Firebase
    public async void UpdateFirebaseDoc(DocumentReference docRef_, Dictionary<string, object> data_) {

        await docRef_.UpdateAsync(data_);

    }

    //Add a new chat Document to the collection of chats
    public async Task<DocumentReference> AddNewChat(Dictionary<string, object> data_) {
        CollectionReference chatRef = _db.Collection("chats");
        DocumentReference addedDocRef = await chatRef.AddAsync(data_);
        return addedDocRef;
    }

    //Reset all data from Firebase to start fresh
    public async Task ResetFirebaseData() {
        //Delete all chats
        Dictionary<string, object> data = new();
        

        Query allChatsQuery = _db.Collection("chats");
        await allChatsQuery.GetSnapshotAsync().ContinueWithOnMainThread(task => {
            QuerySnapshot allChatsQuerySnapshot = task.Result;
            foreach (DocumentSnapshot documentSnapshot in allChatsQuerySnapshot.Documents) {
                DocumentReference docRef = documentSnapshot.Reference;
                docRef.DeleteAsync();
            }
        });

        //Reset players' data
        List<DocumentReference> emptyList = new();
        data.Add("friends", emptyList);
        data.Add("blocked", emptyList);
        data.Add("chats", emptyList);
        data.Add("incomingFriendRequests", emptyList);
        data.Add("outgoingFriendRequests", emptyList);
        data.Add("isOnline", false);
        data.Add("lastSeen", DateTime.Now.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ"));

        var docRef = _db.Collection("players").Document("gabe");
        data.Add("name", "gabriel rocha");
        data.Add("nickname", "gabe");
        await docRef.SetAsync(data);

        docRef = _db.Collection("players").Document("carol");
        data["name"] = "caroline";
        data["nickname"] = "carol";
        await docRef.SetAsync(data);

        docRef = _db.Collection("players").Document("fred");
        data["name"] = "fred flintstone";
        data["nickname"] = "fred";
        await docRef.SetAsync(data);

        docRef = _db.Collection("players").Document("jack");
        data["name"] = "jackson jack";
        data["nickname"] = "jack";
        await docRef.SetAsync(data);

        docRef = _db.Collection("players").Document("paul");
        data["name"] = "paul smith";
        data["nickname"] = "paul";
        await docRef.SetAsync(data);
    }

    //Simply calculates how many days/hours/minutes a given player was last seen online
    public static string CalculateLastSeenOnline(string fireStoreTimestamp) {
        //Calculate how long ago player was last seen online
        var date = DateTime.ParseExact(fireStoreTimestamp.Replace("Timestamp: ", "").Trim(), "yyyy-MM-ddTHH:mm:ss.fffZ", null);
        var dateNow = DateTime.Now;
        var timeDiff = dateNow - date;

        string lastOnline = "Error...";

        if(timeDiff.Days > 0) {
            lastOnline = $"Last seen {timeDiff.Days} days ago";
        }
        else if(timeDiff.Hours > 0) {
            lastOnline = $"Last seen {timeDiff.Hours} hours ago";
        }
        else if (timeDiff.Minutes > 0) {
            lastOnline = $"Last seen {timeDiff.Minutes} minutes ago";
        }
        else {
            lastOnline = "Last seen just now";
        }

        return lastOnline;
    }
    #endregion
}