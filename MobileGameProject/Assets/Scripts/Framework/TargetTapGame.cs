using UnityEngine;
using UnityEngine.UI;

namespace MobileGameProject.Framework
{
    public class TargetTapGame : AMicrogameBehaviour
    {
        [SerializeField] private RectTransform _playArea;
        [SerializeField] private RectTransform _target;
        [SerializeField] private Text _progressText;
        [SerializeField, Min(1)] private int _tapsToWin = 5;

        private int _tapsRemaining;
    }

}