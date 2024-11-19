using System.Linq;
using System.Text;
using UnityEngine;

namespace _Project.CodeBase.Ghost
{
    public enum RecordingType
    {
        Last = 0,
        Best = 1,
        Saved = 2
    }

    /// <summary>
    /// Класс для записи и воспроизведения анимации позиции и вращения объекта.
    /// Сохраняет данные в виде кривых анимации (AnimationCurve) и предоставляет
    /// методы для добавления, оценки и сериализации данных.
    /// </summary>
    public class Recording
    {
        private readonly AnimationCurve _posXCurve = new AnimationCurve();
        private readonly AnimationCurve _posYCurve = new AnimationCurve();
        private readonly AnimationCurve _posZCurve = new AnimationCurve();

        private readonly AnimationCurve _rotXCurve = new AnimationCurve();
        private readonly AnimationCurve _rotYCurve = new AnimationCurve();
        private readonly AnimationCurve _rotZCurve = new AnimationCurve();
        private readonly AnimationCurve _rotWCurve = new AnimationCurve();

        // Разделители для сериализации
        private const char DATA_DELIMITER = '|';
        private const char CURVE_DELIMITER = '\n';

        private readonly Transform _target;

        public float Duration { get; private set; }


        /// <summary>
        /// Конструктор для записи данных объекта.
        /// </summary>
        /// <param name="target">Объект, данные которого записываются.</param>
        public Recording(Transform target)
        {
            _target = target;
        }
        /// <summary>
        /// Конструктор для загрузки записи из строки данных.
        /// </summary>
        /// <param name="data">Сериализованные данные.</param>
        public Recording(string data)
        {
            _target = null; // Восстановление не требует ссылки на объект
            Deserialize(data);

            // Вычисляем длительность записи
            Duration = Mathf.Max(
                _posXCurve.keys.LastOrDefault().time,
                _posYCurve.keys.LastOrDefault().time,
                _posZCurve.keys.LastOrDefault().time
            );
        }

        /// <summary>
        /// Добавляет снимок состояния объекта на текущий момент времени.
        /// </summary>
        /// <param name="elapsed">Прошедшее время с начала записи.</param>
        public void AddSnapshot(float elapsed)
        {
            Duration = elapsed;

            var pos = _target.position;
            var rot = _target.rotation;

            // Обновляем кривые для позиции
            UpdateCurve(_posXCurve, elapsed, pos.x);
            UpdateCurve(_posYCurve, elapsed, pos.y);
            UpdateCurve(_posZCurve, elapsed, pos.z);

            // Обновляем кривые для вращения
            UpdateCurve(_rotXCurve, elapsed, rot.x);
            UpdateCurve(_rotYCurve, elapsed, rot.y);
            UpdateCurve(_rotZCurve, elapsed, rot.z);
            UpdateCurve(_rotWCurve, elapsed, rot.w);
        }
        /// <summary>
        /// Оценивает положение и вращение объекта в заданный момент времени.
        /// </summary>
        /// <param name="elapsed">Время, для которого требуется оценка.</param>
        /// <returns>Поза (положение и вращение) объекта.</returns>
        public Pose EvaluatePoint(float elapsed)
        {
            var position = new Vector3(
                _posXCurve.Evaluate(elapsed),
                _posYCurve.Evaluate(elapsed),
                _posZCurve.Evaluate(elapsed)
            );

            var rotation = new Quaternion(
                _rotXCurve.Evaluate(elapsed),
                _rotYCurve.Evaluate(elapsed),
                _rotZCurve.Evaluate(elapsed),
                _rotWCurve.Evaluate(elapsed)
            );

            return new Pose(position, rotation);
        }

        public string Serialize()
        {
            var builder = new StringBuilder();

            // Добавляем точки для всех кривых
            StringifyCurve(_posXCurve);
            StringifyCurve(_posYCurve);
            StringifyCurve(_posZCurve);
            StringifyCurve(_rotXCurve);
            StringifyCurve(_rotYCurve);
            StringifyCurve(_rotZCurve);
            StringifyCurve(_rotWCurve, false); // Последняя кривая без разделителя

            return builder.ToString();

            // Локальный метод для сериализации кривой
            void StringifyCurve(AnimationCurve curve, bool addDelimiter = true)
            {
                for (var i = 0; i < curve.length; i++)
                {
                    var keyframe = curve[i];
                    builder.Append($"{keyframe.time:F3},{keyframe.value:F2}");
                    if (i != curve.length - 1) builder.Append(DATA_DELIMITER);
                }

                if (addDelimiter) builder.Append(CURVE_DELIMITER);
            }
        }

        private void Deserialize(string data)
        {
            var components = data.Split(CURVE_DELIMITER);

            // Восстанавливаем кривые
            DeserializeCurve(_posXCurve, components[0]);
            DeserializeCurve(_posYCurve, components[1]);
            DeserializeCurve(_posZCurve, components[2]);
            DeserializeCurve(_rotXCurve, components[3]);
            DeserializeCurve(_rotYCurve, components[4]);
            DeserializeCurve(_rotZCurve, components[5]);
            DeserializeCurve(_rotWCurve, components[6]);

            // Локальный метод для восстановления кривой
            void DeserializeCurve(AnimationCurve curve, string curveData)
            {
                var keyframes = curveData.Split(DATA_DELIMITER);
                foreach (var keyframeData in keyframes)
                {
                    var split = keyframeData.Split(',');
                    var time = float.Parse(split[0]);
                    var value = float.Parse(split[1]);
                    curve.AddKey(new Keyframe(time, value));
                }
            }
        }
        private void UpdateCurve(AnimationCurve curve, float time, float value)
        {
            var count = curve.length;
            var keyframe = new Keyframe(time, value);

            // Оптимизация: заменяем последний ключ, если значения не изменились
            if (count > 1 &&
                Mathf.Approximately(value, curve.keys[count - 1].value))
            {
                curve.MoveKey(count - 1, keyframe);
            }
            else
            {
                curve.AddKey(keyframe);
            }
        }
    }


}
