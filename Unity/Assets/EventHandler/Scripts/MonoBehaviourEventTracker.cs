using System;
using System.Collections.Generic;
using UnityEngine;
using CommonCode;

namespace CommonUnity.Events
{
	public class MonoBehaviourEventTracker : MonoBehaviour
	{
		private Dictionary<MonoBehaviourEvent, List<Action>> m_Actions = new Dictionary<MonoBehaviourEvent, List<Action>>();
		private Dictionary<ColliderEvent, List<Action<object>>> m_CollisionActions = new Dictionary<ColliderEvent, List<Action<object>>>();

		public void Add(MonoBehaviourEvent mbe, Action action)
		{
			if (!m_Actions.ContainsKey(mbe))
			{
				m_Actions.Add(mbe, new List<Action>());
			}

			m_Actions[mbe].Add(action);
		}

		public void Add(ColliderEvent ce, Action<object> action)
		{
			if (!m_CollisionActions.ContainsKey(ce))
			{
				m_CollisionActions.Add(ce, new List<Action<object>>());
			}

			m_CollisionActions[ce].Add(action);
		}

		void Awake()
		{
			ExecuteEvent(MonoBehaviourEvent.Awake);
		}

		void Start()
		{
			ExecuteEvent(MonoBehaviourEvent.Start);
		}

		void Update()
		{
			ExecuteEvent(MonoBehaviourEvent.Update);
		}

		void LateUpdate()
		{
			ExecuteEvent(MonoBehaviourEvent.LateUpdate);
		}

		void OnCollisionEnter(Collision collision)
		{
			ExecuteEvent(ColliderEvent.CollisionEnter, collision);
		}

		void OnCollisionExit(Collision collision)
		{
			ExecuteEvent(ColliderEvent.CollisionExit, collision);
		}

		void OnCollisionStay(Collision collision)
		{
			ExecuteEvent(ColliderEvent.CollisionStay, collision);
		}

		void OnTriggerEnter(Collider other)
		{
			ExecuteEvent(ColliderEvent.TriggerEnter, other);
		}

		void OnTriggerExit(Collider other)
		{
			ExecuteEvent(ColliderEvent.TriggerExit, other);
		}

		void OnTriggerStay(Collider other)
		{
			ExecuteEvent(ColliderEvent.TriggerStay, other);
		}

		private void ExecuteEvent(MonoBehaviourEvent mbe)
		{
			if (m_Actions.ContainsKey(mbe))
			{
				m_Actions[mbe].ForEach((a) => { a(); });
			}
		}

		private void ExecuteEvent(ColliderEvent ce, object arg)
		{
			if (m_CollisionActions.ContainsKey(ce))
			{
				m_CollisionActions[ce].ForEach((a) => { a(arg); });
			}
		}
	}
}