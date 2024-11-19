using UnityEngine;

namespace CodeBase.Infrastructure.Service.InputService
{
    public abstract class InputService : IInputService
    {
        protected const string HORIZONTAL = "Horizontal";
        protected const string VERTICAL = "Vertical";

        public abstract Vector2 Axis { get; }
        public abstract float Braking { get; }
    }
}