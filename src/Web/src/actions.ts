import { bindActionCreators, IDispatch } from 'redux';
import { fetchJson } from './helpers';
import { ActionTypes } from './action-types';
import { store } from './store';

const actionCreators = {
}

export const Actions = bindActionCreators(actionCreators as any, store.dispatch) as typeof actionCreators;