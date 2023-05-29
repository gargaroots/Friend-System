using Firebase.Firestore;
using System.Collections.Generic;

public struct ChatDataStructure {

    public DocumentReference DocRef;
    public List<Dictionary<string, object>> ListOfMessages;
    public List<DocumentReference> ListOfParticipantsDocRefs;
}

