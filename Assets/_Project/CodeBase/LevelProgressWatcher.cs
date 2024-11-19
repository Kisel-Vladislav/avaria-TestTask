using _Project.CodeBase;
using _Project.CodeBase.Ghost;
using Ashsvp;
using CodeBase.UI;
using UnityEngine;
using Zenject;

namespace CodeBase
{
    public class LevelProgressWatcher : MonoBehaviour
    {
        [SerializeField] private GameObject _ghostPrefab;
        [SerializeField] private FinishLine _finishLine;
        [SerializeField] private StartWindow _startWindow;
        [SerializeField] private SimcadeVehicleController _vehicleController;
        [SerializeField] private Transform _carInitialPoint;

        private ReplaySystem _replayService;

        [Inject]
        public void Construct(ReplaySystem replayService, SimcadeVehicleController vehicleController)
        {
            _replayService = replayService;
            _vehicleController = vehicleController;
        }

        private void Start()
        {
            _vehicleController.CanDrive = false;

            _startWindow.OnStartButtonClick += StartLevel;
            _finishLine.OnFineshed += RestartLevel;
        }
        private void OnDestroy()
        {
            _startWindow.OnStartButtonClick -= StartLevel;
            _finishLine.OnFineshed -= RestartLevel;
        }

        private void StartLevel()
        {
            _vehicleController.CanDrive = true;
            StartReplayRecording();
            SpawnAndPlayGhost();
        }
        private void RestartLevel()
        {
            _replayService.FinishRun();
            _startWindow.Show();
            _vehicleController.CanDrive = false;
            _vehicleController.ResetVehiclePosition(_carInitialPoint);
        }
        private void SpawnAndPlayGhost()
        {
            var ghost = Instantiate(_ghostPrefab, _carInitialPoint.position, _carInitialPoint.rotation);
            _replayService.PlayRecording(RecordingType.Last, ghost);
        }
        private void StartReplayRecording()
        {
            var target = _vehicleController.GetComponentInChildren<GhostRecord>().gameObject.transform;
            _replayService.StartRun(target);
        }
    }
}