using UnityEngine;

namespace CodeBase.Infrastructure.Service.InputService
{
    public class StandardInputService : InputService
    {
        public override Vector2 Axis =>
            UnityAxis();
        public override float Braking =>
            UnityBrake();

        private Vector2 UnityAxis() =>
            new Vector2(Input.GetAxis(HORIZONTAL), Input.GetAxis(VERTICAL));
        private float UnityBrake() =>
            Input.GetAxis("Jump");
    }
}
