using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Ica.Utils
{
    public static unsafe class NativeCollectionExtensions
    {
        public static void InsertAtBeginning<T>(this ref NativeList<T> list, T element) where T : unmanaged
        {
            list.Add(new T());
            UnsafeUtility.MemMove(list.GetUnsafeList()->Ptr + 1, list.GetUnsafeList()->Ptr, sizeof(T) * (list.Length - 1));
            list[0] = element;
        }

        public static void InsertAtBeginning<T>(this ref UnsafeList<T> list, T element) where T : unmanaged
        {
            list.Add(new T());
            UnsafeUtility.MemMove(list.Ptr + 1, list.Ptr, sizeof(T) * (list.Length - 1));
            list[0] = element;
        }

        public static void Insert<T>(this ref NativeList<T> list, int index, T item) where T : unmanaged
        {
            list.Add(item);
            
            var destination = list.GetUnsafeList()->Ptr + index + 1;
            var source = list.GetUnsafeList()->Ptr + index;
            long size = sizeof(T) * (list.Length - index - 1);
            
            UnsafeUtility.MemMove(destination, source, size);
            
            list[index] = item;
        }
    }
}