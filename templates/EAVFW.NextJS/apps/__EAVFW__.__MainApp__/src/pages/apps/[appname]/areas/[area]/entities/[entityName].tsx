
import React, { useContext, useEffect, useState } from "react";
import { useRouter, NextRouter, } from 'next/router'
import { useAppInfo, useModelDrivenApp, useDefaultMainRibbonItems, ModelDrivenBodyViewer } from "@eavfw/apps";
 


export default function ListEntityRecordsPageWithhDefaultViewPage() {

    const router = useRouter();
    const app = useModelDrivenApp();
    const { currentAppName, currentAreaName } = useAppInfo();





    if (!router.query.area || !router.query.entityName)
        return <div>loading</div>

    const entity = app.getEntity(router.query.entityName as string);
    const selectedView = Object.keys(entity.views ?? {})[0];

    useDefaultMainRibbonItems(entity.views?.[selectedView]?.ribbon, router.push);
   
    return (
        <ModelDrivenBodyViewer 
            recordRouteGenerator={(record) => app.recordUrl({ appName:currentAppName, areaName:currentAreaName, entityName: record.entityName ?? entity.logicalName, recordId: record.id })}
            key={router.query.entityName as string}
            entityName={router.query.entityName as string}
            entity={entity}
            viewName={selectedView}
            locale={app.locale} />

    );
} 

const props = {
    layout: "FormLayout",
    authorize: true
};

// #!if EXPORTS_FOR_BUILD === 'SSG'
ListEntityRecordsPageWithhDefaultViewPage.getInitialProps = () => props;
// #!endif

// #!if EXPORTS_FOR_BUILD === 'SSR'
export const getServerSideProps = () => ({ props: props });
// #!endif

