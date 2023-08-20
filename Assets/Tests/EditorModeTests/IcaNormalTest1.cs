using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;


namespace Tests.EditorModeTests
{
    public class IcaNormalTest1
    {

        [NUnit.Framework.Test]
        public void IcaNormalTest1SimplePasses()
        {
            // Use the Assert class to test conditions.
            Assert.IsTrue(1==2);
        }

        // A UnityTest behaves like a coroutine in PlayMode
        // and allows you to yield null to skip a frame in EditMode
        [UnityEngine.TestTools.UnityTest]
        public System.Collections.IEnumerator IcaNormalTest1WithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // yield to skip a frame
            yield return null;
        }
        
        
 
    }
}