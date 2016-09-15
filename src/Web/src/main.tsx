import 'babel-polyfill';
import 'bluebird';
import 'whatwg-fetch';
import React from 'react';
import { render } from 'react-dom';
import { Provider } from 'react-redux';
import { Router, IndexRoute, Route, browserHistory } from 'react-router';
import Layout from './layout';
import { store } from './store';
import { Actions } from './actions';

window.onload = () => {
    render((
        <Provider store={store}>
            <Router history={browserHistory}>
                <Route path='/'>
                    <IndexRoute component={Layout}/>
                </Route>
            </Router>
        </Provider>
    ), document.getElementById("app"));
};
