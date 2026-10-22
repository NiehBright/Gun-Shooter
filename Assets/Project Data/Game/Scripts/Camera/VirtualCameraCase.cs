
using UnityEngine;

namespace Watermelon
{
    [System.Serializable]
    public class VirtualCameraCase
    {
        [SerializeField] CameraType cameraType;
        public CameraType CameraType => cameraType;

#pragma warning disable 0618
        [SerializeField] Unity.Cinemachine.CinemachineVirtualCamera virtualCamera;
        public Unity.Cinemachine.CinemachineVirtualCamera VirtualCamera => virtualCamera;

        private Unity.Cinemachine.CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
        public Unity.Cinemachine.CinemachineBasicMultiChannelPerlin CinemachineBasicMultiChannelPerlin => cinemachineBasicMultiChannelPerlin;
#pragma warning restore 0618

        private TweenCase shakeTweenCase;

        public void Initialise()
        {
            cinemachineBasicMultiChannelPerlin = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        public void Shake(float fadeInTime, float fadeOutTime, float duration, float gain)
        {
            shakeTweenCase.KillActive();

            gain *= 2;

            shakeTweenCase = Tween.DoFloat(0.0f, gain, fadeInTime, (float fadeInValue) =>
            {
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = fadeInValue;
            }).OnComplete(delegate
            {
                shakeTweenCase = Tween.DelayedCall(duration, delegate
                {
                    shakeTweenCase = Tween.DoFloat(gain, 0.0f, fadeOutTime, (float fadeOutValue) =>
                    {
                        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = fadeOutValue;
                    });
                });
            });
        }
    }
}
