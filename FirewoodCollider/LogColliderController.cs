using MSCLoader;
using UnityEngine;

namespace FirewoodCollider
{
    public class LogColliderController : MonoBehaviour
    {
        public Collider[] PushColliders;
        public SettingsSliderInt BreakForceSlider;

        internal Collider logCollider;
        internal Collider logCloneColider;

        private void Start()
        {
            // Set value to avoid crashes
            if (PushColliders == null)
            {
                PushColliders = new Collider[0];
            }

            // The prefab is made of 2 parts and has 2 colliders.
            logCollider = gameObject.transform.parent.GetComponent<Collider>();
            logCloneColider = gameObject.transform.GetComponent<Collider>();

            ignoreColliders(PushColliders, true);

            // Log prefab was set to be indestructable. We ignored the collider 
            // and the log wont break by itself so reset value to default.
            gameObject.GetComponent<FixedJoint>().breakForce = BreakForceSlider == null ? 10000f : BreakForceSlider.GetValue();
        }

        private void OnJointBreak()
        {
            // The logs FSM script should break the log.
            // We just need to respect the collider again.
            ignoreColliders(PushColliders, false);
        }

        void ignoreColliders(Collider[] colliders, bool ignore)
        {
            for (var i = 0; i < colliders.Length; i++)
            {
                var collider = colliders[i];
                Physics.IgnoreCollision(logCloneColider, collider, ignore);
                Physics.IgnoreCollision(logCollider, collider, ignore);
            }
        }
    }
}