import React from 'react';
import { connect } from 'react-redux';
import { Link } from 'react-router';
import { RoutedState } from './reducer';
import Home from './home';
import Ad from './ad';
import { preventDefault } from './helpers';

function adKey() {
    return window.location.href.replace(/\?q=.*/, '');
}

function Layout(props: RoutedState) {
    return (
        <div>
            <nav className="navbar navbar-default navbar-fixed-top">
                <div className="container-fluid">
                    <div className="navbar-header">
                        <button type="button" className="navbar-toggle collapsed" data-toggle="collapse" data-target="#bs-example-navbar-collapse-1" aria-expanded="false">
                            <span className="sr-only">Toggle navigation</span>
                            <span className="icon-bar"></span>
                            <span className="icon-bar"></span>
                            <span className="icon-bar"></span>
                        </button>
                        <Link className="navbar-brand" to="/">NetStandard Type Search</Link>
                    </div>
                    <div className="collapse navbar-collapse" id="bs-example-navbar-collapse-1">
                        <ul className="nav navbar-nav navbar-right">
                            <li><a href="http://stephencleary.com/">by Stephen Cleary</a></li>
                        </ul>
                    </div>
                </div>
            </nav>
            <div className='container-fluid'>
                <div className='row'>
                    <div className='col-sm-5 col-md-4'>
                        <Ad key={adKey()} slot='7090994222' width={300} height={600}/>
                    </div>
                    <div className='col-sm-2 col-md-4'>
                        <Home {...props}/>
                    </div>
                    <div className='col-sm-5 col-md-4'>
                        <Ad key={adKey()} slot='1044460625' width={300} height={600}/>
                    </div>
                </div>
            </div>
        </div>
    );
}

export default connect(x => x)(Layout);