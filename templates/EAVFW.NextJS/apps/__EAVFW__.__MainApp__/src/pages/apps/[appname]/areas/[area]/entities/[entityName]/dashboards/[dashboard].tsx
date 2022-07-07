
import React, { useContext, useEffect, useState } from "react";
 
import { useRouter } from "next/router";
 
import dynamic from 'next/dynamic'
import { EmbedProps } from 'powerbi-client-react';

import { useModelDrivenApp } from "@eavfw/apps";
import { useSWRFetch } from "@eavfw/manifest";

const PowerBIEmbed = dynamic<EmbedProps>(() => import('powerbi-client-react').then(m => m.PowerBIEmbed), { ssr: false });

export type FormsConfig = {

    /**
     * The entity viewer will only show the FormPicker when there are more than one form. This can be overridden here
     */
    alwaysShowFormSelector?: boolean;
}

const defaultConfig: FormsConfig = { // Omit<ModelDrivenEntityViewerProps, keyof (WithRouterProps & WithAppProps)> = {
    alwaysShowFormSelector: false
}


export default function FormPage() {

    const router = useRouter();
    const app = useModelDrivenApp();

    if (!router.query.area || !router.query.entityName)
        return <div>loading</div>

    const entity = app.getEntity(router.query.entityName as string);

    const { data: embedParams, isLoading } = useSWRFetch(`/pbiembed/workspaces/490e8e7d-2cf6-4d83-85e6-cc93d30435a0/reports/c1215150-604f-4353-bfd6-a7f4b4121a13`);

    const selectedView = router.query.dashboard as string;;
    const [embedconfig, setocnfig] = useState<any>();
    useEffect(() => {
        if (!isLoading) {
            dynamicallyImportPackage()
        }
    }, [isLoading])
    let dynamicallyImportPackage = async () => {
        const models = await import('powerbi-client').then(k => k.models);

        setocnfig({
            type: 'report',   // Supported types: report, dashboard, tile, visual and qna
           // id: '<id>',
            embedUrl: embedParams.EmbedReport[0].EmbedUrl,
            accessToken: embedParams.EmbedToken.Token,
            tokenType: models.TokenType.Embed,
            settings: {
                panes: {
                    filters: {
                        expanded: false,
                        visible: false
                    }
                },
                background: models.BackgroundType.Transparent,
            }
        });
        // you can now use the package in here
    }

    if (!embedconfig)
        return <div>loading</div>;
  
    //Question : Should the home items retrival be in the page to make model driven grid viewer simpler?
    //Answer: Currently the abstraction of pages/items seems simpler to be tighly coubled with the gridviewer.

    return (
        <PowerBIEmbed
            embedConfig={embedconfig}

            eventHandlers={
                new Map([
                    ['loaded', function () { console.log('Report loaded'); }],
                    ['rendered', function () { console.log('Report rendered'); }],
                    ['error', function (event: any) { console.log(event.detail); }]
                ])
            }

            cssClassName={"report-style-class"}

            getEmbeddedComponent={(embeddedReport) => {
              //  this.report = embeddedReport as Report;
            }}
        />

    );
}
 

const props = {
    layout: "FormLayout",
    authorize: true
};

// #!if EXPORTS_FOR_BUILD === 'SSG'
FormPage.getInitialProps = () => props;
// #!endif

// #!if EXPORTS_FOR_BUILD === 'SSR'
export const getServerSideProps = () => ({ props: props });
// #!endif
