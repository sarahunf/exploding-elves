using Actors.Enum;
using Actors.Factory.Interface;
using Actors.Interface;
using UnityEngine;

namespace Actors.Factory
{
    public class ElfFactory : MonoBehaviour, IEntityFactory
    {
        //TODO: replace this for a more scalable approach
        [SerializeField] private Elf blackElfPrefab;
        [SerializeField] private Elf redElfPrefab;
        [SerializeField] private Elf whiteElfPrefab;
        [SerializeField] private Elf blueElfPrefab;
    
        public IEntity CreateEntity(EntityType entityType)
        {
            switch (entityType)
            {
                case EntityType.BlackElf:
                    return Instantiate(blackElfPrefab);
                case EntityType.RedElf:
                    return Instantiate(redElfPrefab);
                case EntityType.WhiteElf:
                    return Instantiate(whiteElfPrefab);
                case EntityType.BlueElf:
                    return Instantiate(blueElfPrefab);
                default:
                    throw new System.ArgumentException($"Unknown entity type: {entityType}");
            }
        }
    }
}