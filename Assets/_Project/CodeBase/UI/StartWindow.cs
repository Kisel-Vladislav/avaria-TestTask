using System;
using UnityEngine;
using UnityEngine.UI;

namespace CodeBase.UI
{
    public class StartWindow : MonoBehaviour
    {
        [SerializeField] private Button startButton;

        public event Action OnStartButtonClick;
        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonPressed);
        }
        private void OnStartButtonPressed()
        {
            OnStartButtonClick?.Invoke();
            gameObject.SetActive(false);
        }

        public void Show() =>
            gameObject.SetActive(true);
    }
}