using System.Collections;
using UnityEngine;

namespace _Project.CodeBase.Infrastructure.Services
{
    public interface ICoroutineRunner
    {
        Coroutine StartCoroutine(IEnumerator coroutine);
    }
}
