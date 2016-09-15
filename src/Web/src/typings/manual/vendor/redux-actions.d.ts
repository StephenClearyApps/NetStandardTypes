declare namespace ReduxActionsFixed {
    interface Action {
        type: string;
        payload?: any;
        error?: boolean;
        meta?: any;
    }

    type Reducer<State> = (state: State, action: Action) => State;

    type ReducerMap<State> = {
        [actionType: string]: Reducer<State>
    };

    type HandleActions<State> = (reducerMap: ReducerMap<State>, initialState?: State) => Reducer<State>;
}
