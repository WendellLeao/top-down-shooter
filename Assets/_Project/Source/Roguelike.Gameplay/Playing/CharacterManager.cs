using Leaosoft.Services;
using Leaosoft.Events;
using UnityEngine;
using Leaosoft;
using Roguelike.Gameplay.Maps;

namespace Roguelike.Gameplay.Playing
{
    public sealed class CharacterManager : Manager
    {
        [SerializeField]
        private Character _characterPrefab;
        [SerializeField]
        private Transform _spawnPoint;
        
        private IEventService _eventService;
        private Character _character;

        protected override void OnInitialize()
        {
            base.OnInitialize();

            _eventService = ServiceLocator.GetService<IEventService>();
            
            _eventService.AddEventListener<CharacterCollideDoorEvent>(HandleCharacterCollideDoor);
            
            SpawnCharacter();
        }

        protected override void OnDispose()
        {
            base.OnDispose();
            
            _eventService.RemoveEventListener<CharacterCollideDoorEvent>(HandleCharacterCollideDoor);
            
            _character.Stop();
        }

        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);
            
            _character.Tick(deltaTime);
        }

        protected override void OnFixedTick(float fixedDeltaTime)
        {
            base.OnFixedTick(fixedDeltaTime);

            _character.FixedTick(fixedDeltaTime);
        }
        
        private void HandleCharacterCollideDoor(GameEvent gameEvent)
        {
            if (gameEvent is CharacterCollideDoorEvent)
            {
                DestroyCharacter();

                SpawnCharacter();
            }
        }
        
        private void SpawnCharacter()
        {
            _character = Instantiate(_characterPrefab, transform);

            _character.transform.position = _spawnPoint.position;

            _character.HealthController.OnDead += HandleCharacterDead;
            
            _character.Begin();
            
            _eventService.DispatchEvent(new CharacterSpawnedEvent(_character));
        }

        private void HandleCharacterDead()
        {
            DestroyCharacter();
            
            SpawnCharacter();
        }

        private void DestroyCharacter()
        {
            _character.HealthController.OnDead -= HandleCharacterDead;
            
            _character.Stop();

            // TODO: implement pooling
            Destroy(_character.gameObject);
        }
    }
}
