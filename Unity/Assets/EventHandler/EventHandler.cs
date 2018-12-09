using CommonCS.Exceptions;
using CommonCS.IEnumerables;
using CommonCS.Objects.Reflection.Events;
using CommonCS.Objects.Reflection.Fields;
using CommonCS.Objects.Reflection.Methods;
using CommonCS.Types;
using CommonUnity.Deprecated;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;
using CommonCode;

namespace CommonUnity.Events
{
	////////////////////////////////////////
	//
	//	EventHandler
	//
	////////////////////////////////////////

	public class EventHandler : CustomBehaviour
	{
		/*
			Public properties
		*/

		public Event[] events { get { return _Events.ToArray(); } }

		/*
			Inspector fields
		*/

#pragma warning disable 649

		[SerializeField]
		private List<Event> _Events;

#pragma warning restore 649

		/*
			Private fields
		*/

		private List<Event> m_InternalEvents;
		
		/*
			Callback methods
		*/

		/// <summary>
		/// Awake
		/// </summary>
		void Awake()
		{
			m_InternalEvents = new List<Event>(_Events);

			for (int i = 0; i < m_InternalEvents.Count; i++)
			{
				Event ce = m_InternalEvents[i];

				// remove if disabled
				if (!ce.enabled)
				{
					m_InternalEvents.RemoveAt(i--);
					continue;
				}
				
				// attempt to initialise event
				bool success = ce.Initialise(Execute);
				
				// if initialisation succeeded, remove from list
				if (success)
				{
					m_InternalEvents.RemoveAt(i--);
				}
			}
		}

		void Update()
		{
			foreach (var ce in m_InternalEvents)
			{
				// events are guaranteed to be keycode triggered at update time
				if (ce.keyFunc(ce.keyCode))
				{
					ce.ExecuteActions();
				}
			}
		}
	}

	////////////////////////////////////////
	//
	//	Enums
	//
	////////////////////////////////////////

	public enum Trigger				{ KeyPress, Collision, Time, UnityEvent, CSharpEvent, MonoBehaviourEvent, Pipe };
	public enum ColliderEvent		{ CollisionEnter, CollisionExit, CollisionStay, TriggerEnter, TriggerExit, TriggerStay };
	public enum ArgumentType		{ None, Bool, Float, Int, String, Object };
	public enum MethodType			{ StandardMethod, StartCoroutine, StopCoroutine };
	public enum KeyPressType		{ Down, Up, Hold };

	////////////////////////////////////////
	//
	//	HandlerAction
	//
	////////////////////////////////////////

	[Serializable]
	public class HandlerAction
	{
		/*
			Drawer fields
		*/
		
		///////////////////////////
		public int	componentIndex,
					methodIndex;

		public bool isIEnumerator;
		///////////////////////////

		/*
			Public fields :(
		*/

		// action
		public GameObject target;
		public Component component;
		public string method;
		public ArgumentType argumentType;
		public MethodType methodType;

		// different types of args
		public bool argBool;
		public float argFloat;
		public int argInt;
		public string argString;
		public UnityEngine.Object argObject;

		// use provided argument
		public bool useArg;

		/*
			Private fields
		*/

		private List<Action<object>> m_PipedActions = new List<Action<object>>();

		/*
			Private readonly fields
		*/

		private readonly Type[] k_ArgumentTypes = { null, typeof(bool), typeof(float), typeof(int), typeof(string), typeof(UnityEngine.Object) };

		/*
			Public methods
		*/

		/// <summary>
		/// Execute action with no arguments
		/// </summary>
		public void Execute()
		{
			Execute(null);
		}

		/// <summary>
		/// Execute action possibly using generic argument
		/// </summary>
		public void GenericExecute<T>(T t)
		{
			Execute(t);
		}

		/// <summary>
		/// Execute action possibly using argument
		/// </summary>
		public void Execute(object newArg)
		{
			// get correct argument
			object arg = null;

			// get function argument type
			Type type = k_ArgumentTypes[(int)argumentType];
			if (type != null)
			{
				// is it the same type as the event?
				bool sameType = false;
				if (newArg != null)
				{
					sameType = newArg.GetType().IsOrInheritsFrom(type);
				}

				// decide which argument to use
				arg = type.GetDefaultValue();
				if (useArg)
				{
					switch (argumentType)
					{
						case ArgumentType.Bool: arg = argBool; break;
						case ArgumentType.Float: arg = argFloat; break;
						case ArgumentType.Int: arg = argInt; break;
						case ArgumentType.Object: arg = argObject; break;
						case ArgumentType.String: arg = argString; break;
					}
				}
				else if (sameType)
				{
					arg = newArg;
				}
			}

			object result = null;
			
			// invoke method
			MonoBehaviour monoBehaviour = null;
			switch (methodType)
			{
				// start method normally
				case MethodType.StandardMethod:
					switch (argumentType)
					{
						case ArgumentType.None: result = component.InvokeMethod(methodIndex);	break;
						default:				result = component.InvokeMethod(methodIndex, arg);	break;
					}
					break;

				// start method as a coroutine
				case MethodType.StartCoroutine:
					monoBehaviour = component as MonoBehaviour;
					if (monoBehaviour == null)
					{
						throw new InvalidCastException("That component was not a MonoBehaviour and thus cannot start coroutines");
					}

					switch (argumentType)
					{
						case ArgumentType.None: monoBehaviour.StartCoroutine(method);		break;
						default:				monoBehaviour.StartCoroutine(method, arg);	break;
					}
					break;

				// stop coroutine
				case MethodType.StopCoroutine:
					monoBehaviour = component as MonoBehaviour;
					if (monoBehaviour == null)
					{
						throw new InvalidCastException("That component was not a MonoBehaviour and thus cannot stop coroutines");
					}

					monoBehaviour.StopCoroutine(method);
					break;
			}

			// call piped actions with result
			m_PipedActions.ForEach((action) =>
			{
				action(result);
			});
		}

		/// <summary>
		/// Subscribe to named csharp event on component object
		/// </summary>
		public void SubscribeToCSharpEvent(Component component, string eventName)
		{
			Type eventType = component.GetEventParameterType(eventName, 1);
			if (eventType == null)
			{
				component.AddEventHandler(eventName, Execute, this);
			}
			else
			{
				MethodInfo methodInfo = GetType().GetMethod("GenericExecute");
				MethodInfo generic = methodInfo.MakeGenericMethod(eventType);

				component.AddEventHandler(eventName, generic, this);
			}
		}

		/// <summary>
		/// Add piped action
		/// </summary>
		public void AddPipedAction(Action<object> action)
		{
			m_PipedActions.Add(action);
		}
	}

	////////////////////////////////////////
	//
	//	Event
	//
	////////////////////////////////////////

	[Serializable]
	public class Event
	{
		/*
			Drawer fields
		*/

		///////////////////////////
		public bool fold;
		public bool triggerFold;
		public bool resultFold;
		public bool actionsFold;
		public int unityIndex;
		public int unityEventIndex;
		public int csharpIndex;
		public int csharpEventIndex;
		public int pipeEventIndex;
		public int pipeActionIndex;
		///////////////////////////

		/*
			Public fields :( 
		*/

		// name and trigger type
		public string name;
		public Trigger trigger;
		public bool enabled;

		// keypress
		public KeyCode keyCode;
		public KeyPressType keyPressType;
		public Func<KeyCode, bool> keyFunc;

		// collision
		public Collider collider;
		public ColliderEvent colliderEvent;

		// time
		public float time;

		// unityevent
		public GameObject unityEventTarget;
		public Component unityEventComponent;
		public string unityEventName;

		// csharpevent
		public GameObject csharpEventTarget;
		public Component csharpEventComponent;
		public string csharpEventName;

		// monobehaviourevent
		public GameObject monoBehaviourTarget;
		public MonoBehaviourEvent monoBehaviourEvent;

		// piping
		public EventHandler pipeTarget;
		public string pipeEvent;
		public int pipeAction;

		// actions
		public HandlerAction[] actions;

		// additional actions
		public Action additionalActions;
		public Action<bool> additionalActionsW_Bool;
		public Action<float> additionalActionsW_Float;
		public Action<int> additionalActionsW_Int;
		public Action<string> additionalActionsW_String;
		public Action<UnityEngine.Object> additionalActionsW_Object;

		/*
			Public methods 
		*/

		/// <summary>
		/// Execute actions with no arguments
		/// </summary>
		public void ExecuteActions()
		{
			//actions.ForEach((ca) => { ca.Execute(); });

			foreach (var action in actions)
			{
				action.Execute();
			}
		}

		/// <summary>
		/// Execute actions with an argument
		/// </summary>
		public void ExecuteActionsW_Arg(object arg)
		{
			//actions.ForEach((ca) => { ca.Execute(arg); });
			
			foreach (var action in actions)
			{
				action.Execute(arg);
			}
		}

		/// <summary>
		/// Initialise actions that don't need to be alive at update time
		/// </summary>
		public bool Initialise(Action<Action, float> execute)
		{
			switch (trigger)
			{
				case Trigger.Collision:				AddCollisionEventHandlers();		return true;
				case Trigger.CSharpEvent:			AddCSharpEventHandlers();			return true;
				case Trigger.MonoBehaviourEvent:	AddMonoBehaviourEventHandlers();	return true;
				case Trigger.Pipe:					PipeActions();						return true;
				case Trigger.UnityEvent:			AddUnityEventHandlers();			return true;
				case Trigger.Time:					execute(ExecuteActions, time);		return true;
				case Trigger.KeyPress:
					switch (keyPressType)
					{
						case KeyPressType.Down: keyFunc = Input.GetKeyDown; break;
						case KeyPressType.Hold: keyFunc = Input.GetKey;		break;
						case KeyPressType.Up:	keyFunc = Input.GetKeyUp;	break;
					}
					break;
			}

			return false;
		}

		/*
			Private methods
		*/

		/// <summary>
		/// Pipe actions to their target
		/// </summary>
		private void PipeActions()
		{
			try
			{
				var controllerEvent = pipeTarget.events.GetFirst((ce) =>
				{
					return ce.name == pipeEvent;
				});

				var controllerAction = controllerEvent.actions[pipeAction];

				controllerAction.AddPipedAction(ExecuteActionsW_Arg);
			}
			catch (ElementNotFoundException)
			{
				throw new ElementNotFoundException("Controller did not contain an event with that name.");
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException("Controller event did not contain " + pipeAction.ToString() + "actions.");
			}
		}
		
		/// <summary>
		/// Add handlers to collision event
		/// </summary>
		private void AddCollisionEventHandlers()
		{
			var collisionTracker = collider.GetComponent<MonoBehaviourEventTracker>();
			if (collisionTracker == null)
			{
				collisionTracker = collider.gameObject.AddComponent<MonoBehaviourEventTracker>();
			}

			collisionTracker.Add(colliderEvent, ExecuteActionsW_Arg);
		}

		/// <summary>
		/// Add handlers to mono behaviour event
		/// </summary>
		private void AddMonoBehaviourEventHandlers()
		{
			var tracker = monoBehaviourTarget.GetComponent<MonoBehaviourEventTracker>();
			if (tracker == null)
			{
				tracker = monoBehaviourTarget.AddComponent<MonoBehaviourEventTracker>();
			}

			tracker.Add(monoBehaviourEvent, ExecuteActions);
		}

		/// <summary>
		/// Add handlers to csharp event
		/// </summary>
		private void AddCSharpEventHandlers()
		{
			foreach (var ca in actions)
			{
				ca.SubscribeToCSharpEvent(
					csharpEventComponent, 
					csharpEventName
				);
			}
		}

		/// <summary>
		/// Add handlers to unity event
		/// </summary>
		private void AddUnityEventHandlers()
		{
			foreach (var ca in actions)
			{
				UnityEvent ue = unityEventComponent.GetFieldValueOrNull<UnityEvent>(unityEventName);
				if (ue == null)
				{
					bool success = AddUnityEventHandlers<bool>(ca);
					
					if (!success)
						success = AddUnityEventHandlers<float>(ca);

					if (!success)
						success = AddUnityEventHandlers<int>(ca);

					if (!success)
						success = AddUnityEventHandlers<string>(ca);

					if (!success)
						AddUnityEventHandlers<UnityEngine.Object>(ca);
				}
				else
				{
					ue.AddListener(new UnityAction(ca.Execute));
				}
			}
		}

		/// <summary>
		/// Add generic handlers to generic unity event
		/// </summary>
		private bool AddUnityEventHandlers<T>(HandlerAction action)
		{
			UnityEvent<T> uet = unityEventComponent.GetFieldValueOrNull<UnityEvent<T>>(unityEventName);

			if (uet == null)
			{
				return false;
			}

			uet.AddListener(new UnityAction<T>(action.GenericExecute));

			return true;
		}
	}
}