using System.Collections;
using NUnit.Framework;
using Unity.Collections;
using UnityEditor;
using UnityEngine.TestTools;

namespace Ica.Utils.Tests
{
    public class NativeCollectionExtensionsTests
    {
        [Test]
        public void InsertAtBeginning()
        {
            var toInsert = 98765;
            var listOriginal = new NativeList<int>(1, Allocator.Temp) { 0, 1, 2, 3, 4 };
            var listModified = new NativeList<int>(1, Allocator.Temp) { 0, 1, 2, 3, 4 };

            listModified.InsertAtBeginning(toInsert);

            Assert.AreEqual(listModified[0], toInsert);
            for (int i = 0; i < listOriginal.Length; i++)
            {
                Assert.AreEqual(listOriginal[i], listModified[i + 1]);
            }
        }
        
        [Test]
        public void InsertAtBeginningToEmptyList()
        {

            var list = new NativeList<int>( Allocator.Temp) ;
            list.InsertAtBeginning(3);
            list.InsertAtBeginning(2);
            list.InsertAtBeginning(1);
            list.InsertAtBeginning(0);

            Assert.AreEqual(list.Length, 4);
            for (int i = 0; i < list.Length; i++)
            {
                Assert.AreEqual(list[i], i);
            }
        }
    }
}