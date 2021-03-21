﻿using System.Collections;
using EFT.Trainer.Configuration;
using EFT.UI;
using UnityEngine;

namespace EFT.Trainer.Features
{
	public abstract class CachableMonoBehaviour<T> : MonoBehaviour, IEnableable
	{
		[ConfigurationProperty]
		public abstract float CacheTimeInSec { get; set; }

		[ConfigurationProperty]
		public abstract bool Enabled { get; set; }

		private T _data;
		private bool _refreshing = false;

#if DEBUG_PERFORMANCE
		private readonly System.Diagnostics.Stopwatch _stopwatch = new();
#endif

		private void Start()
		{
			StartCoroutine(RefreshDataScheduler());
		}

		protected static void AddConsoleLog(string log, string from = "scheduler")
		{
			if (PreloaderUI.Instantiated)
				PreloaderUI.Instance.Console.AddLog(log, from);
		}

		private IEnumerator RefreshDataScheduler()
		{
			if (Enabled)
			{
				try
				{
					_refreshing = true;

#if DEBUG_PERFORMANCE
					_stopwatch.Restart();
					AddConsoleLog($"Refreshing {GetType().Name}...");
#endif

					_data = RefreshData();
				}
				finally
				{
					_refreshing = false;

#if DEBUG_PERFORMANCE
					_stopwatch.Stop();
#endif

				}
			}

#if DEBUG_PERFORMANCE
			AddConsoleLog($"Refreshed in {_stopwatch.ElapsedMilliseconds}ms...");
#endif

			yield return new WaitForSeconds(CacheTimeInSec);
			StartCoroutine(RefreshDataScheduler());
		}

		private void Update()
		{
			if (!Enabled || _refreshing) 
				return;

			if (_data != null)
				ProcessData(_data);
		}

		private void OnGUI()
		{
			if (!Enabled || _refreshing) 
				return;

			if (_data != null)
				ProcessDataOnGUI(_data);
		}

		public abstract T RefreshData();
		public virtual void ProcessData(T data) { }
		public virtual void ProcessDataOnGUI(T data) { }
	}
}
