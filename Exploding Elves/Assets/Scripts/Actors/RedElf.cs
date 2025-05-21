using Actors.Enum;
namespace Actors
{
    public class RedElf : Elf
    {
        private void Awake()
        {
            entityType = EntityType.RedElf;
        }
    }
}