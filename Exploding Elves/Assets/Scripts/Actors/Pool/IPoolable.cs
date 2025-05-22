namespace Actors.Pool
{
    public interface IPoolable
    {
        void SetPool(EntityPool pool);
        void ReturnToPool();
    }
}