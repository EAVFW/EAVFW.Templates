import "../styles/global.css";
import '../styles/craftjs.css';
import "@eavfw/apps/src/Layouts/RootLayout.scss";
import "../components";
import React, { Component, createContext } from 'react';
import { initializeIcons } from "@fluentui/font-icons-mdl2";
import { RouterBasedAppContextProvider } from "@eavfw/next";
import defaultTheme from "../themes/default";
import topBarTheme from "../themes/default";

initializeIcons(/* optional base url */);

import { NextComponentType, NextPageContext } from "next";
import { NextRouter } from "next/dist/shared/lib/router/router";

import {
    EAVApp, ModelDrivenApp, useAppInfo, useModelDrivenApp, UserProvider,
    PageLayoutProps, ResolveFeature, PageLayout, RegisterFeature
} from "@eavfw/apps";
import manifest from "../manifest";
import { ThemeProvider } from "@fluentui/react";

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

//export const RootLayout: React.FC = (props: any) => {
//    console.log(props);


//    if (props.layout === "EmptyLayout")
//        return props.children;

    

//    return <ThemeProvider theme={defaultTheme} {...props} id="web-container" />
//}



const MyAppLayout: React.FC<AppProps> = ({ Component, pageProps, err, router, layoutProps }) => {
    console.log("App " + router.pathname + " " + pageProps.layout);


    const app = useModelDrivenApp();
    const appInfo = useAppInfo();

    const Layout = getLayout(pageProps);
    const RootLayout = ResolveFeature("RootLayout");

    return (
        <RootLayout {...pageProps} theme={defaultTheme} key="AppLayout" id="AppLayoutId">
            <Layout id="PageLayout" {...app._data} key="PageLayout" title={appInfo.title}>
                <Component {...pageProps} app={app} key="PageComponent" id="PageComponentId" />
            </Layout>
        </RootLayout>
    )
}

export const MyApp: React.FC<AppProps> = (props) => {


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


RegisterFeature("defaultTheme", defaultTheme);
RegisterFeature("topBarTheme", topBarTheme);