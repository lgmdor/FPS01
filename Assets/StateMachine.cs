using System;
using System.Collections.Generic;

public class State {
	public List<Transition> transitions;
	public Action Update = () => {};
	public Action Start = () => {};
	public Action End = () => {};
	public string name;

	public State(string name): this(name, null) {}

	public State(string name, Action Update): this(name, Update, null) {}

	public State(string name, Action Update, Action Start): this(name, Update, Start, null) {}

	public State(string name, Action Update, Action Start, Action End) {
		transitions = new List<Transition>();

		this.name = name;
		this.Update = Update != null ? Update : () => {};
		this.Start = Start != null ? Start : () => {};
		this.End = End != null ? End : () => {};
	}

	public void AddTransition(State nextState, Condition condition) {
		transitions.Add(new Transition(nextState, condition));
	}
}

public struct Transition {
	public State nextState;
	public Condition condition;

	public Transition(State nextState, Condition condition) {
		this.nextState = nextState;
		this.condition = condition;
	}
}

public class StateMachine {
	public Dictionary<string, Condition> conditions;
	public Dictionary<string, State> states;
	// public Dictionary<State, Condition> transitions;
	public State currState;
	public State prevState;

	public StateMachine() {
		states = new Dictionary<string, State>();
		conditions = new Dictionary<string, Condition>();
		currState = null;
	}

	public State AddState(State state) {
		if(!states.ContainsKey(state.name)) {
			states[state.name] = state;

			return state;
		}
		else {
			throw new ArgumentException($"State '{state.name} already exists'");
		}
	}

	public void TryToChangeState() {
		foreach (Transition transition in currState.transitions) {
			if(transition.condition()) {
				ChangeState(transition.nextState);
			}
		}
	}

	private void ChangeState(State nextState) {
			prevState = currState;
			currState.End();
			currState = nextState;
			currState.Start();
	}

}

public delegate bool Condition();