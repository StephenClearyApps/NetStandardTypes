import React from 'react';

/* eslint-disable react/no-danger */

function showAds() {
    window.setTimeout(() => {
        try {
            ; //TODO ((window as any).adsbygoogle || []).push({});
        } catch (e) {
            // adsbygoogle will throw if there are no ads to load.
            return;
        }
    }, 200);
}

function adSenseHtml(slot: string, width: number, height: number) {
    const style = (width && height) ? `display:inline-block;width:${width}px;height:${height}px` : 'display:block';
    return { __html: `<ins class="adsbygoogle"
        style="${style}"
        data-ad-client="ca-pub-2749292939902134"
        data-ad-slot="${slot}"
        data-ad-format="auto"></ins>` };
}

interface AdProps {
    slot: string;
    width?: number;
    height?: number;
}

class Ad extends React.Component<AdProps, any> {
    componentDidMount() {
        showAds();
    }

    componentWillReceiveProps() {
        showAds();
    }

    render() {
        return (
            <div className='text-center'>
                <div className='center-block' dangerouslySetInnerHTML={adSenseHtml(this.props.slot, this.props.width, this.props.height)}/>
            </div>
        );
    }
}

export default Ad;