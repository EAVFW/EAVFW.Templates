import "../styles/global.css";
import '../styles/craftjs.css';
import "@eavfw/apps/src/Layouts/RootLayout.scss";
import React, { Component, createContext } from 'react';
import { initializeIcons } from "@fluentui/font-icons-mdl2";
import { RouterBasedAppContextProvider } from "@eavfw/next";

import "../themes/default";
import "../components";

initializeIcons(/* optional base url */);

import { NextComponentType, NextPageContext } from "next";
import { NextRouter } from "next/dist/shared/lib/router/router";

import {
    EAVApp, ModelDrivenApp, useAppInfo, useModelDrivenApp, UserProvider,
    PageLayoutProps, ResolveFeature, PageLayout, RegisterFeature
} from "@eavfw/apps";
import manifest from "../manifest";


import { useContext } from "react";
import { useAppInsight } from "../components/AppInsights";

type AppProps = {
    pageProps: any;
    layoutProps: PageLayoutProps;
    Component: NextComponentType<NextPageContext, any, { app: ModelDrivenApp }>;
    err?: string;
    router: NextRouter,
    app: ModelDrivenApp,
    user: any
};



function getLayout(pageProps: any) {
    console.log("Getting layout: " + pageProps.layout);
    if (!pageProps.layout)
        return PageLayout;
    return ResolveFeature(pageProps.layout, false) ?? PageLayout;

}


function replaceLiteral(body: string, obj: any) {
    var iterLiteral = "\\[(.*?)\\]";
    let i = 0;
    var re = new RegExp(iterLiteral, "g");
    return body.replace(re, (s) => obj[s.substring(1, s.length - 1)]);
}

const MyAppLayout: React.FC<AppProps> = ({ Component: PageLayout, pageProps, err, router, layoutProps }) => {

    const rootKey = router.pathname + pageProps.layout;
    const pageKey = replaceLiteral(router.pathname, router.query);
    const ai = useAppInsight();

    ai.log("AppLayout Render: Path={Path} PathName={PathName}, RootKey={RootKey}, PageKey={PageKey} PageProps={@PageProps}",
        router.asPath, router.pathname, rootKey, pageKey, pageProps);

    const app = useModelDrivenApp();

    /**
     * App Info contains current App,Area, Entity, Record
     * */
    const appInfo = useAppInfo();

    /**
     * The EAVFW Layouts:
     *   - FormLayout : 
     *      Model Driven Grid Selected Provider, Ribbon Provider, Message Provider, 
     *      Progress Provider, TopBar, RibbonBar, MessageArea, Scrollable Pane, ProgressBar
     * */
    const Layout = getLayout(pageProps);

    /** 
     * <ThemeProvider theme={defaultTheme} {...props} id="web-container" />
     * @eavfw/apps/Layouts/RootLayout
     * */
    const RootLayout = ResolveFeature("RootLayout");


    /**
     * PageLayout is the component returned from /src/pages folder resolved from current route.
     * */

    return (

        <RootLayout {...pageProps} key={rootKey}>
            <Layout id="PageLayout" {...app._data} title={appInfo.title}>
                <PageLayout {...pageProps} app={app} key={pageKey} id="PageComponentId" />
            </Layout>
        </RootLayout>

    )
}

export const MyApp: React.FC<AppProps> = (props) => {

    const ai = useAppInsight();

    ai.log("App Render: AppProps={@AppProps}", props);

    return (
        <EAVApp manifest={manifest}>
            <RouterBasedAppContextProvider>
                <UserProvider authorize={props.pageProps.authorize} >

                    <MyAppLayout {...props} />

                </UserProvider>
            </RouterBasedAppContextProvider>
        </EAVApp>
    );
}
export default MyApp;
//export default withModelDrivenApp(MyApp);

//TODO does this make sense - https://vpilip.com/next-js-page-loading-indicator-improve-ux-of-next-js-app/
