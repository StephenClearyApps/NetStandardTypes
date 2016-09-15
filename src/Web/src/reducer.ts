import { IDispatch } from 'redux';
import { handleActions } from 'redux-actions';
import { spread } from './helpers';
import { ActionTypes } from './action-types';
import { Location } from 'history';

export interface State {
}

export interface RoutedState extends State {
    location: Location;
    dispatch: IDispatch;
}

const defaultState: State = {
};

export const reducer = (handleActions as ReduxActionsFixed.HandleActions<State>)({
}, defaultState);
