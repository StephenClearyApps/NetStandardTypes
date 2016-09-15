declare module 'react-disqus-thread' {
    import React from 'react';

    export interface ReactDisqusThreadProps {
        shortname: string;
        identifier?: string;
        title?: string;
        url?: string;
        category_id?: string;
    }

    class ReactDisqusThread extends React.Component<ReactDisqusThreadProps, any> {
    }

    export default ReactDisqusThread;
}