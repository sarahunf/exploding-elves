using UnityEngine;

namespace Actors.Pool
{
    public interface IPool
    {
        void ReturnToPool(GameObject obj);
    }
} 