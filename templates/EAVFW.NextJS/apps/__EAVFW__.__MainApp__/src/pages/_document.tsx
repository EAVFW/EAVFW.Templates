import * as React from "react";
import Document, { Html, Head, Main, NextScript, DocumentContext } from "next/document";
import { Stylesheet, InjectionMode } from "@fluentui/merge-styles";
import { resetIds } from "@fluentui/utilities";

// Do this in file scope to initialize the stylesheet before Fabric components are imported.
const stylesheet = Stylesheet.getInstance();

import { ServerStyleSheet } from 'styled-components';

// Set the config.
stylesheet.setConfig({
    injectionMode: InjectionMode.none,
    namespace: "server",
});

type DocumentType = {
    styleTags: any;
}

// Now set up the document, and just reset the stylesheet.
class MyDocument extends Document<DocumentType> {

    static async getInitialProps(ctx: DocumentContext) {
        stylesheet.reset();
        resetIds();

        console.log("ctx", ctx);
        const sheet = new ServerStyleSheet();

        const page = await ctx.renderPage((App) => (props: any) => {
            console.log("pageProps", props);

            return sheet.collectStyles(< App {...props} />)

            return < App {...props} />;
        });

        const styleTags2 = sheet.getStyleElement();
        const styleTags1 = stylesheet.getRules(true);

        console.log("page", page);

        return {
            ...page, styleTags: styleTags1
            , styleTags2
        };
    }

    render() {

        const useBlazor = "NEXT_PUBLIC_BLAZOR_NAMESPACE" in process.env;

        return (
            <Html>
                <base href={`${process.env.NEXT_PUBLIC_BASE_PATH ?? '/'}`} />
                <Head>
                    <script type="text/javascript" src={`${process.env['NEXT_PUBLIC_BASE_URL']??'/'}config/config.js`}></script>
                    <style
                        type="text/css"
                        dangerouslySetInnerHTML={{ __html: this.props.styleTags }}
                    />
                    {(this.props as any).styleTags2}

                   
                    
                </Head>
                <body >
                   
                    <Main />

                    {useBlazor &&
                        <script src="/_framework/blazor.webassembly.js" />
                    }

                    <NextScript />

                </body>
            </Html>
        );
    }
}

export default MyDocument
