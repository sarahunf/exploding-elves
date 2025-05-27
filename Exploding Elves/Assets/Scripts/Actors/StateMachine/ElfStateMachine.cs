using UnityEngine;
using Actors.Enum;

namespace Actors.StateMachine
{
    public class ElfStateMachine
    {
        private ElfState currentState;
        private readonly Elf elf;
        private float stateTimer;
        private bool isStateComplete;

        public ElfStateMachine(Elf elf)
        {
            this.elf = elf;
            currentState = ElfState.Spawning;
            stateTimer = 0f;
            isStateComplete = false;
        }

        public void Update()
        {
            if (isStateComplete) return;

            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0f)
            {
                CompleteCurrentState();
            }
        }

        public void SetState(ElfState newState, float duration = 0f)
        {
            currentState = newState;
            stateTimer = duration;
            isStateComplete = false;

            switch (newState)
            {
                case ElfState.Spawning:
                    elf.GetView().SetEmission(true, elf.GetConfig().body * 2f);
                    elf.GetView().SetScale(false);
                    break;
                case ElfState.Idle:
                    elf.GetView().SetEmission(false, Color.black);
                    elf.GetView().SetScale(true);
                    break;
                case ElfState.Replicating:
                    elf.GetView().SetEmission(true, elf.GetConfig().body * 2f);
                    elf.GetView().SetScale(false);
                    break;
                case ElfState.Exploding:
                    // Handled in Explode method
                    break;
            }
        }

        private void CompleteCurrentState()
        {
            isStateComplete = true;
            switch (currentState)
            {
                case ElfState.Spawning:
                case ElfState.Replicating:
                    SetState(ElfState.Idle);
                    break;
            }
        }

        public ElfState GetCurrentState() => currentState;
        public bool CanReplicate() => currentState == ElfState.Idle;
    }
} 