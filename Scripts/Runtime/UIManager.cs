using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Toybox {

	public abstract class View : MonoBehaviour {
		private bool _visible;
		public abstract void Init(UIManager manager);
		public virtual void SetVisibility(bool visible) {
			_visible = visible;
			gameObject.SetActive(visible);
		}
		public bool IsVisible => _visible;
	}

	public class UIManager : Singleton<UIManager> {
		
		#region PublicFields
		
		public delegate void UpdateDelegate();
		public event UpdateDelegate OnUpdate;
		
		#endregion

		#region PrivateFields
		
		[SerializeField] View _startingView;
		[SerializeField] List<View> _views;
		View _currentView;
		readonly Stack<View> _history = new Stack<View>();
		
		#endregion

		#region PrivateMethods
		
		protected override void Awake () => base.Awake();

		void Start () {
			if (_views.Count == 0) {
				_views = FindObjectsOfType<View>(true).ToList();
			}
			_views.ForEach(view => {
				view?.Init(this);
				view?.SetVisibility(false);
			});
			_startingView?.SetVisibility(true);
		}

		void Update () {
			OnUpdate?.Invoke();
		}

		#endregion

		#region PublicMethods

		public void AddView (View view) => _views.Add(view);

		public T GetView<T>() where T : View => _views.OfType<T>().FirstOrDefault();

		public void Show<T>(bool remember = true) where T : View {
			foreach (var view in _views.OfType<T>()) {
				if (_currentView != null && remember) {
					_history.Push(_currentView);
				}

				_currentView?.SetVisibility(false);
				view.SetVisibility(true);
				_currentView = view;
			}
		}

		public void Show(View view, bool remember = true) {
			if (_currentView != null) {
				if (remember) {
					_history.Push(_currentView);
				}

				_currentView.SetVisibility(false);
			}

			view.SetVisibility(true);
			_currentView = view;
		}

		public void ShowLast () {
			if (_history.Count != 0) {
				Show(_history.Pop(), false);
			}
		}

		#endregion
	}
}