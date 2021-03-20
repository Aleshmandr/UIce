using UnityEngine;

namespace Juice
{
	public class TransformBinder : MonoBehaviour, IBinder<Vector3>
	{
		[SerializeField] private BindingInfo position = new BindingInfo(typeof(IReadOnlyObservableVariable<Vector3>));
		[SerializeField] private BindingInfo rotation = new BindingInfo(typeof(IReadOnlyObservableVariable<Quaternion>));

		private Transform transformCache;

		private VariableBinding<Vector3> positionBinding;
		private VariableBinding<Quaternion> rotationBinding;

		protected virtual void Awake()
		{
			transformCache = transform;

			positionBinding = new VariableBinding<Vector3>(position, this);
			positionBinding.Property.Changed += OnPositionChanged;

			rotationBinding = new VariableBinding<Quaternion>(rotation, this);
			rotationBinding.Property.Changed += OnRotationChanged;
		}

		protected virtual void OnEnable()
		{
			positionBinding.Bind();
			rotationBinding.Bind();
		}

		protected virtual void OnDisable()
		{
			positionBinding.Unbind();
			rotationBinding.Unbind();
		}

		private void OnPositionChanged(Vector3 newValue)
		{
			transformCache.position = newValue;
		}

		private void OnRotationChanged(Quaternion newValue)
		{
			transformCache.rotation = newValue;
		}
	}
}
