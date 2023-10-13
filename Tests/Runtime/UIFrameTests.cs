﻿using System.Collections;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace Uice.Tests
{
	public class UIFrameTests
	{
		private static readonly string PanelAPath = "Assets/Tests/Runtime/Prefabs/Panel A.prefab";

		private UIFrame uiFrame;

		[SetUp]
		public void Setup()
		{
			uiFrame = UIFrameUtility.CreateDefaultUIFrame();
		}

		[TearDown]
		public void TearDown()
		{
			Object.Destroy(uiFrame.gameObject);
			uiFrame = null;
		}

		[UnityTest]
		public IEnumerator _00_UIFrameObjectIsCreated()
		{
			Assert.NotNull(uiFrame);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _01_MainCanvasIsPresent()
		{
			Assert.NotNull(uiFrame.MainCanvas);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _02_CameraIsPresent()
		{
			Assert.NotNull(uiFrame.UICamera);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _03_EventSystemIsPresent()
		{
			EventSystem eventSystem = uiFrame.GetComponentInChildren<EventSystem>();

			Assert.NotNull(eventSystem);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _04_WindowLayerIsPresent()
		{
			WindowLayer windowLayer = uiFrame.GetComponentInChildren<WindowLayer>();

			Assert.NotNull(windowLayer);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _05_PanelLayerIsPresent()
		{
			PanelLayer panelLayer = uiFrame.GetComponentInChildren<PanelLayer>();

			Assert.NotNull(panelLayer);
			yield return null;
		}

		[UnityTest]
		public IEnumerator _06_PanelIsRegistered()
		{
			uiFrame.Initialize();
			PanelA panelAPrefab = AssetDatabase.LoadAssetAtPath<PanelA>(PanelAPath);
			PanelA panelInstance = Object.Instantiate(panelAPrefab);

			uiFrame.RegisterView(panelInstance);

			Assert.IsTrue(uiFrame.IsViewRegistered<PanelA>());
			yield return null;
		}

		[UnityTest]
		public IEnumerator _07_PanelIsRegisteredThenDisposed()
		{
			uiFrame.Initialize();
			PanelA panelAPrefab = AssetDatabase.LoadAssetAtPath<PanelA>(PanelAPath);
			PanelA panelInstance = Object.Instantiate(panelAPrefab);

			uiFrame.RegisterView(panelInstance);
			uiFrame.UnregisterView(panelInstance);

			Assert.IsFalse(uiFrame.IsViewRegistered<PanelA>());
			yield return null;
		}
	}
}
