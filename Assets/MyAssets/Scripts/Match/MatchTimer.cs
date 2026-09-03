using UnityEngine;

namespace Assets.MyAssets.Scripts.Match
{
    /// <summary>
    /// 자석볼을 놓은 뒤 결과가 확정될 때까지 기다리는 시간을 재는 타이머.
    /// MonoBehaviour가 아닌 순수 C# 클래스라 프레임 진행은 소유자(GameDirector)가 Tick으로 넣어준다.
    /// </summary>
    public sealed class MatchTimer
    {
        private float remainingTime;

        /// <summary>UI에 표시할 남은 시간. 음수로 내려가지 않는다.</summary>
        public float DisplayTime => Mathf.Max(remainingTime, 0f);

        public bool IsFinished => remainingTime <= 0f;

        public void Begin(float duration)
        {
            remainingTime = duration;
        }

        public void Tick(float deltaTime)
        {
            remainingTime -= deltaTime;
        }

        /// <summary>
        /// 자석볼끼리 달라붙는 등 결과가 아직 확정되지 않았을 때 대기 시간을 늘린다.
        /// </summary>
        public void ExtendTime(float seconds)
        {
            remainingTime += seconds;
        }
    }
}
