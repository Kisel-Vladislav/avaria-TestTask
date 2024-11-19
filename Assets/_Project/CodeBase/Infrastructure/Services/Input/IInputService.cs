using UnityEngine;

namespace CodeBase.Infrastructure.Service.InputService
{
    public interface IInputService
    {
        Vector2 Axis { get; }
        float Braking { get; }
    }
}
