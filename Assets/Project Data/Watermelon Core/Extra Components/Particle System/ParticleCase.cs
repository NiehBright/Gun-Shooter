using UnityEngine;

namespace Watermelon
{
    public class ParticleCase
    {
        private ParticleSystem particleSystem;
        public ParticleSystem ParticleSystem => particleSystem;

        private float disableTime = -1;

        private bool forceDisable;

        private Particle particle;
        private ParticleBehaviour particleBehaviour;

        public ParticleCase(Particle particle, bool isDelayed)
        {
            this.particle = particle;

            // Create object
            GameObject particleObject = particle.ParticlePool.GetPooledObject();
            particleObject.SetActive(true);

            // Get particle component
            particleSystem = particleObject.GetComponent<ParticleSystem>();

            if(!isDelayed)
            {
                particleSystem.Play();
            }
            else
            {
                particleSystem.Stop();
            }

            if (particle.SpecialBehaviour)
            {
                particleBehaviour = particleObject.GetComponent<ParticleBehaviour>();
                particleBehaviour.OnParticleActivated();
            }
        }

        public ParticleCase SetPosition(Vector3 position)
        {
            particleSystem.transform.position = position;

            return this;
        }

        public ParticleCase SetScale(Vector3 scale)
        {
            particleSystem.transform.localScale = scale;

            return this;
        }

        public ParticleCase SetRotation(Quaternion rotation)
        {
            particleSystem.transform.localRotation = rotation;

            return this;
        }

        public ParticleCase SetDuration(float duration)
        {
            disableTime = Time.time + duration;

            return this;
        }

        public ParticleCase SetTarget(Transform followTarget, Vector3 localPosition)
        {
            particleSystem.transform.SetParent(followTarget);
            particleSystem.transform.localPosition = localPosition;

            return this;
        }

        public void OnDisable()
        {
            Transform container = (particle != null && particle.ParticlePool != null && particle.ParticlePool.ObjectsContainer != null)
                ? particle.ParticlePool.ObjectsContainer 
                : PoolManager.ObjectsContainerTransform;

            if (particleSystem != null)
            {
                particleSystem.transform.SetParent(container);
                particleSystem.Stop();
                particleSystem.gameObject.SetActive(false);
            }

            if (particle != null && particle.SpecialBehaviour)
                particleBehaviour.OnParticleDisabled();
        }

        public void ForceDisable(ParticleSystemStopBehavior stopBehavior = ParticleSystemStopBehavior.StopEmitting)
        {
            forceDisable = true;
            Transform container = (particle != null && particle.ParticlePool != null && particle.ParticlePool.ObjectsContainer != null)
                ? particle.ParticlePool.ObjectsContainer 
                : PoolManager.ObjectsContainerTransform;

            if (particleSystem != null)
            {
                particleSystem.transform.SetParent(container);
                particleSystem.Stop(true, stopBehavior);
            }
        }

        public bool IsForceDisabledRequired()
        {
            if (forceDisable)
                return true;

            if (disableTime != -1 && Time.time > disableTime)
                return true;

            return false;
        }
    }
}