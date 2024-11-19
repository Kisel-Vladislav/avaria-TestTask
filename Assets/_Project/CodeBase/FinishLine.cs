using System;
using UnityEngine;
namespace _Project.CodeBase
{
    public class FinishLine : MonoBehaviour
    {
        public event Action OnFineshed;

        private void OnTriggerEnter(Collider other) => OnFineshed?.Invoke();
    }
}
