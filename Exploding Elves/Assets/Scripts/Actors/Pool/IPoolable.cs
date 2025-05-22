using UnityEngine;

namespace Actors.Pool
{
    public interface IPoolable
    {
        void SetPool(IPool pool);
        void ReturnToPool();
    }
}