namespace Actors.Interface
{
    public interface IPoolable
    {
        void SetPool(IPool pool);
        void ReturnToPool();
    }
} 