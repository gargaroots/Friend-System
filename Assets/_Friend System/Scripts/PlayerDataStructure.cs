using Firebase.Firestore;
using System.Collections.Generic;

public struct PlayerDataStructure{

    public string Name;
    public string Nickname;
    public bool IsOnline;
    public string LastSeen;
    public DocumentReference DocRef;

    public List<DocumentReference> ListOfBlockedPlayersDocRefs;
    public List<DocumentReference> ListOfFriendsDocRefs;
    public List<DocumentReference> ListOfChatsDocRefs;
    
    public List<DocumentReference> ListOfIncomingFriendRequestsDocRefs;
    public List<DocumentReference> ListOfOutgoingFriendRequestsDocRefs;

    public List<PlayerDataStructure> ListOfFriends;
    public List<PlayerDataStructure> ListOfBlocked;
    public List<PlayerDataStructure> ListOfIncomingFriendRequests;
    public List<PlayerDataStructure> ListOfOutgoingFriendRequests;

    public List<ChatDataStructure> ListOfChats;
}
