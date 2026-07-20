using UnityEngine;

namespace Garbage
{
    public abstract class GarbageBase : MonoBehaviour
    {

        [SerializeField] private Rigidbody _rigidbody = null;

        public Rigidbody Rigidbody => _rigidbody;

        private void Awake()
        {
            if (_rigidbody == null)
            {
                _rigidbody = GetComponent<Rigidbody>();
            }

            if (!tag.Contains(GarbageConstant.GARBAGE_TAG))
            {
                tag = GarbageConstant.GARBAGE_TAG;
            }
        }
    }
}
