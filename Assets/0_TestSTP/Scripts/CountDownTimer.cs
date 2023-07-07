using System.Collections.Generic;
using MEC;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace TestSTP {
    public class CountDownTimer : MonoBehaviour {
        [SerializeField] private GameObject _transMain;
        [SerializeField] private TMP_Text _txtTimer;
        [SerializeField] private float _remainingTime;
        [SerializeField] private float _tempTime;
        [SerializeField] private Slider _slider;
        [SerializeField] private Image _sliderFill;

        private float _maxValue;

        [Header("Settings")] [SerializeField] private float _timeoutDuration = 5f;
        private UnityAction _timeoutAction;

        private string _sentEventTag = "SentEvent";
        private bool _startCountDown;

        private void Update() {
            if (!_startCountDown) return;

            if (_tempTime <= 0) {
                _startCountDown = false;
                Timing.RunCoroutine(IEWaitForSendEvent(), _sentEventTag);
            } else {
                _tempTime -= Time.deltaTime;
                _txtTimer.text = Utils.SecondsToHourForLand(_tempTime);
                if (_slider != null)
                {
                    var percent = _tempTime / _maxValue;
                    _slider.value = percent;
                    if(percent > 0.6f)
                        _sliderFill.color = Color.green;
                    if(percent < 0.6f && percent > 0.3f)
                        _sliderFill.color = Color.yellow;
                    if(percent < 0.3f)
                        _sliderFill.color = Color.red;
                }
            }
        }

        public void StartNewCountdownTimer(string id, float remainingSeconds, UnityAction timeoutAction, float max = 0f) {
            _sentEventTag = id;
            _remainingTime = remainingSeconds;
            _timeoutAction = timeoutAction;
            if (max > 0f) _maxValue = max;
            if (_remainingTime > 0) {
                gameObject.SetActive(true);
                _txtTimer.text = Utils.SecondsToHourForLand(_remainingTime);
                StartCountDown();
                _transMain.SetActive(true);
            } else {
                Hide();
                Debug.Log("_duration = 0 | no need countime");
            }
        }

        private void StartCountDown() {
            _tempTime = _remainingTime;
            _startCountDown = true;
        }

        #region Event timer

        private IEnumerator<float> IEWaitForSendEvent() {
            Debug.Log("Start IEWaitForSendEvent");
            yield return Timing.WaitForSeconds(_timeoutDuration);
            _timeoutAction?.Invoke();
        }

        #endregion

        private void KillTimers() {
            Timing.KillCoroutines(_sentEventTag);
        }

        public void Hide() {
            KillTimers();
            _timeoutAction = null;
            gameObject.SetActive(false);
        }

        public void SetExpired(UnityAction timeoutAction)
        {
            _txtTimer.text = "Expired";
            _slider.value = 0f;
            _sliderFill.color = Color.red;
            _timeoutAction = timeoutAction;
            Timing.RunCoroutine(IEWaitForSendEvent(), _sentEventTag);
        }
    }
}