using UnityEngine;

namespace CodeBase.Infrastructure.Service.InputService
{
    public class InputSystemService : IInputService
    {
        private readonly GameInput _gameInput;

        public InputSystemService()
        {
            _gameInput = new GameInput();
            _gameInput.Enable();
        }

        public  Vector2 Axis =>
            _gameInput.Gameplay.Movment.ReadValue<Vector2>();
        public  float Braking =>
            _gameInput.Gameplay.Brake.ReadValue<float>();
    }

}
