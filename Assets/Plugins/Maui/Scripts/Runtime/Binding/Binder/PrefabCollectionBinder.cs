﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace Maui
{
	public class PrefabCollectionBinder : CollectionBinder<object>
	{
		public IReadOnlyList<CollectionItemViewModelComponent> CurrentItems => currentItems;
		
		[SerializeField] private List<CollectionItemViewModelComponent> prefabs;

		private Transform container;
		private List<CollectionItemViewModelComponent> currentItems;
		private Dictionary<Type, CollectionItemViewModelComponent> prefabResolutionCache;

		protected override void Awake()
		{
			base.Awake();

			container = transform;
			currentItems = new List<CollectionItemViewModelComponent>();
			prefabResolutionCache = new Dictionary<Type, CollectionItemViewModelComponent>();
		}

		protected override void OnCollectionReset()
		{
			ClearItems();
		}

		protected override void OnCollectionCountChanged(int oldCount, int newCount)
		{
			// Nothing to do here
		}

		protected override void OnCollectionItemAdded(int index, object value)
		{
			InsertItem(index, value);
		}

		protected override void OnCollectionItemMoved(int oldIndex, int newIndex, object value)
		{
			MoveItem(oldIndex, newIndex);
		}

		protected override void OnCollectionItemRemoved(int index, object value)
		{
			RemoveItem(index);
		}

		protected override void OnCollectionItemReplaced(int index, object oldValue, object newValue)
		{
			if (GetBestPrefabFromCache(oldValue) == GetBestPrefabFromCache(newValue))
			{
				SetItemValue(index, newValue);
			}
			else
			{
				RemoveItem(index);
				InsertItem(index, newValue);
			}
		}
		
		protected virtual CollectionItemViewModelComponent SpawnItem(CollectionItemViewModelComponent prefab, Transform itemParent)
		{
			return Instantiate(prefab, itemParent, false);
		}

		protected virtual void DisposeItem(CollectionItemViewModelComponent item)
		{
			Destroy(item.gameObject);
		}

		private void ClearItems()
		{
			for (int i = currentItems.Count - 1; i >= 0; i--)
			{
				RemoveItem(i);
			}
		}

		private void RemoveItem(int index)
		{
			CollectionItemViewModelComponent item = currentItems[index];
			currentItems.RemoveAt(index);
			DisposeItem(item);
		}

		private void InsertItem(int index, object value)
		{
			CollectionItemViewModelComponent bestPrefab = GetBestPrefabFromCache(value);
			CollectionItemViewModelComponent newItem = SpawnItem(bestPrefab, container);
			currentItems.Insert(index, newItem);
			newItem.transform.SetSiblingIndex(index);
			SetItemValue(index, value);
		}

		private void SetItemValue(int index, object value)
		{
			currentItems[index].SetData(value);
		}

		private void MoveItem(int oldIndex, int newIndex)
		{
			CollectionItemViewModelComponent item = currentItems[oldIndex];
			currentItems.RemoveAt(oldIndex);
			currentItems.Insert(newIndex, item);
			item.transform.SetSiblingIndex(newIndex);
		}

		private CollectionItemViewModelComponent GetBestPrefabFromCache(object value)
		{
			Type valueType = value.GetType();

			if (prefabResolutionCache.TryGetValue(valueType, out var result) == false)
			{
				result = FindBestPrefab(valueType);

				if (result != null)
				{
					prefabResolutionCache[valueType] = result;
				}
			}

			return result;
		}

		private CollectionItemViewModelComponent FindBestPrefab(Type valueType)
		{
			CollectionItemViewModelComponent result = null;
			int bestDepth = -1;

			foreach (CollectionItemViewModelComponent prefab in prefabs)
			{
				Type injectionType = prefab.InjectionType;
				Type viewModelType;

				if (injectionType != null
				    && (viewModelType = GetViewModelType(injectionType)) != null
				    && viewModelType.GenericTypeArguments[0].IsAssignableFrom(valueType))
				{
					Type dataType = viewModelType.GenericTypeArguments[0];
					Type baseType = dataType.BaseType;
					int depth = 0;

					while (baseType != null)
					{
						depth++;
						baseType = baseType.BaseType;
					}

					if (depth > bestDepth)
					{
						bestDepth = depth;
						result = prefab;
					}
				}
			}

			return result;
		}

		private Type GetViewModelType(Type runtimeType)
		{
			Type result = null;

			Type genericType = null;
			
			if (runtimeType.IsGenericType)
			{
				genericType = runtimeType.GetGenericTypeDefinition();
			}

			if (genericType != null && genericType == typeof(BindableViewModel<>))
			{
				result = runtimeType;
			}
			else if (runtimeType.BaseType != null)
			{
				result = GetViewModelType(runtimeType.BaseType);
			}

			return result;
		}
	}
}