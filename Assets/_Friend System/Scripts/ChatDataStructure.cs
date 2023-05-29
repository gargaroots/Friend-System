using Firebase.Firestore;
using System.Collections.Generic;

namespace Gabe.FriendSystem {

    public struct ChatDataStructure {

        public DocumentReference DocRef;
        public List<Dictionary<string, object>> ListOfMessages;
        public List<DocumentReference> ListOfParticipantsDocRefs;
    }
}