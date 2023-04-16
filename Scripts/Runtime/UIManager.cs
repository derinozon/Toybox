using UnityEngine;
using System.Collections.Generic;

namespace Toybox {

	public abstract class View : MonoBehaviour {
		public abstract void Initialize();

		public virtual void Hide() => gameObject.SetActive(false);

		public virtual void Show() => gameObject.SetActive(true);
	}

	public class UIManager : Singleton<UIManager> {


		[SerializeField] private View _startingView;

		[SerializeField] private View[] _views;

		private View _currentView;

		private readonly Stack<View> _history = new Stack<View>();

		protected override void Awake () {
			base.Awake();
		}

		public T GetView<T>() where T : View
		{
			for (int i = 0; i < _views.Length; i++)
			{
				if (_views[i] is T tView)
				{
					return tView;
				}
			}

			return null;
		}

		public void Show<T>(bool remember = true) where T : View
		{
			for (int i = 0; i < _views.Length; i++)
			{
				if (_views[i] is T)
				{
					if (_currentView != null)
					{
						if (remember)
						{
							_history.Push(_currentView);
						}

						_currentView.Hide();
					}

					_views[i].Show();

					_currentView = _views[i];
				}
			}
		}

		public void Show(View view, bool remember = true)
		{
			if (_currentView != null)
			{
				if (remember)
				{
					_history.Push(_currentView);
				}

				_currentView.Hide();
			}

			view.Show();

			_currentView = view;
		}

		public void ShowLast()
		{
			if (_history.Count != 0)
			{
				Show(_history.Pop(), false);
			}
		}

		private void Start()
		{
			for (int i = 0; i < _views.Length; i++)
			{
				_views[i].Initialize();

				_views[i].Hide();
			}

			if (_startingView != null)
			{
				Show(_startingView, true);
			}
		}
	}
}
