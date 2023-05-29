using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


namespace Gabe.FriendSystem {

    public class FriendSystemTests {
        // A Test behaves as an ordinary method
        [Test]
        public void FriendSystemTestsSimplePasses() {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator SearchPlayerTest() {
            GameObject gameObj = new();
            var search = gameObj.AddComponent<SearchFriendsController>();

            //search.SearchForFriends();

            yield return new WaitForSeconds(2f);
        }
    }
}