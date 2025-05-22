using UnityEngine;

namespace Actors.Interface
{
    public interface IPool
    {
        GameObject Get();
        void ReturnToPool(GameObject obj);
    }
} 