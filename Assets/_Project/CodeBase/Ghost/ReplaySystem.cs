using _Project.CodeBase.Infrastructure.Services;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace _Project.CodeBase.Ghost
{
    public class ReplaySystem : IInitializable
    {
        private readonly Dictionary<RecordingType, Recording> _runs = new Dictionary<RecordingType, Recording>();
        private readonly WaitForFixedUpdate _wait = new WaitForFixedUpdate();

        private Recording _currentRun;
        private float _elapsedRecordingTime;
        private int _snapshotEveryNFrames;
        private int _frameCount;
        private float _maxRecordingTimeLimit;

        private Recording _currentReplay;
        private GameObject _ghostObj;
        private bool _destroyOnComplete;
        private float _replaySmoothedTime;

        private readonly float _smoothFactor = 0.1f;

        private readonly ICoroutineRunner _coroutineRunner;

        public ReplaySystem(ICoroutineRunner runner)
        {
            _coroutineRunner = runner;
        }
        public void Initialize()
        {
            _coroutineRunner.StartCoroutine(FixedUpdate());
            _coroutineRunner.StartCoroutine(Update());
        }

        /// <summary>
        /// Begin recording a run
        /// </summary>
        /// <param name="target">The transform you wish to record</param>
        /// <param name="snapshotEveryNFrames">The accuracy of the recording. Smaller number == higher file size</param>
        /// <param name="maxRecordingTimeLimit">Stop recording beyond this time</param>
        public void StartRun(Transform target, int snapshotEveryNFrames = 2, float maxRecordingTimeLimit = 60)
        {
            _currentRun = new Recording(target);

            _elapsedRecordingTime = 0;

            _snapshotEveryNFrames = Mathf.Max(1, snapshotEveryNFrames);
            _frameCount = 0;

            _maxRecordingTimeLimit = maxRecordingTimeLimit;
        }

        /// <summary>
        /// Complete the current recording
        /// </summary>
        /// <param name="save">If we want to save this run. Use false for restarts</param>
        /// <returns>Whether this run was the fastest so far</returns>
        public bool FinishRun(bool save = true)
        {
            if (_currentRun == null)
                return false;

            if (!save)
            {
                _currentRun = null;
                return false;
            }

            _runs[RecordingType.Last] = _currentRun;
            _currentRun = null;

            if (!GetRun(RecordingType.Best, out var best) || _runs[RecordingType.Last].Duration <= best.Duration)
            {
                _runs[RecordingType.Best] = _runs[RecordingType.Last];
                return true;
            }

            return false;
        }

        public void SetSavedRun(Recording run) => _runs[RecordingType.Saved] = run;
        public bool GetRun(RecordingType type, out Recording run) => _runs.TryGetValue(type, out run);

        /// <summary>
        /// Begin playing a recording
        /// </summary>
        /// <param name="type">The type of recording you wish to play</param>
        /// <param name="ghostObj">The visual representation of the ghost. Must be pre-instantiated (this allows customization)</param>
        /// <param name="destroyOnCompletion">Whether or not to automatically destroy the ghost object when the run completes</param>
        public void PlayRecording(RecordingType type, GameObject ghostObj, bool destroyOnCompletion = true)
        {
            if (_ghostObj != null) Object.Destroy(_ghostObj);

            if (!GetRun(type, out _currentReplay))
            {
                Object.Destroy(ghostObj);
                return;
            }

            _replaySmoothedTime = 0;
            _destroyOnComplete = destroyOnCompletion;

            if (_currentReplay != null) 
                _ghostObj = ghostObj;
            else if (_destroyOnComplete) 
                Object.Destroy(_ghostObj);
        }

        /// <summary>
        /// Stop the replay. Should be called when the player finishes the run before the ghost
        /// </summary>
        public void StopReplay()
        {
            if (_ghostObj != null) Object.Destroy(_ghostObj);
            _currentReplay = null;
        }

        private IEnumerator FixedUpdate()
        {
            while (true)
            {
                yield return _wait;
                AddSnapshot();
                _elapsedRecordingTime += Time.smoothDeltaTime;
            }
        }
        private IEnumerator Update()
        {
            while (true)
            {
                yield return null;
                _replaySmoothedTime += Time.smoothDeltaTime;
                UpdateReplay();
            }
        }
        private void UpdateReplay()
        {
            if (_currentReplay == null) return;

            // Evaluate the point at the current time
            var pose = _currentReplay.EvaluatePoint(_replaySmoothedTime);

            // Smooth the position and rotation for better visual effect
            _ghostObj.transform.position = Vector3.Lerp(_ghostObj.transform.position, pose.position, _smoothFactor);
            _ghostObj.transform.rotation = Quaternion.Slerp(_ghostObj.transform.rotation, pose.rotation, _smoothFactor);

            // Destroy the replay when done
            if (_replaySmoothedTime > _currentReplay.Duration)
            {
                _currentReplay = null;
                if (_destroyOnComplete) Object.Destroy(_ghostObj);
            }
        }
        private void AddSnapshot()
        {
            if (_currentRun == null) return;

            // Capture frame, taking into account the frame skip
            if (_frameCount++ % _snapshotEveryNFrames == 0)
                _currentRun.AddSnapshot(_elapsedRecordingTime);

            // End a run over the limit
            if (_currentRun.Duration >= _maxRecordingTimeLimit) 
                FinishRun(false);
        }
    }
}

