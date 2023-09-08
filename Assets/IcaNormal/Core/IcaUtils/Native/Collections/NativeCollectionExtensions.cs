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
    }
}