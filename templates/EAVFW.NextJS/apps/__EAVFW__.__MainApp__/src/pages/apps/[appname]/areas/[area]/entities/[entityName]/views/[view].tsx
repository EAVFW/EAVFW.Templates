
import React, { useContext } from "react";
import { useRouter } from "next/router";
import { ICommandBarItemProps } from "@fluentui/react";
import {
    useAppInfo, useDefaultMainRibbonItems, useModelDrivenApp, ModelDrivenBodyViewer
} from "@eavfw/apps";

 

export default function FormPage() {

    const router = useRouter();
    const app = useModelDrivenApp();

    if (!router.query.area || !router.query.entityName)
        return <div>loading</div>

    const entity = app.getEntity(router.query.entityName as string);

   

    const selectedView = router.query.view as string;;

    useDefaultMainRibbonItems(entity.views?.[selectedView]?.ribbon, router.push);
    
    const { currentAppName, currentAreaName } = useAppInfo();
     
    return (
        <ModelDrivenBodyViewer
            recordRouteGenerator={(record) => app.recordUrl({ appName:currentAppName,areaName: currentAreaName, entityName: record.entityName ?? entity.logicalName, recordId: record.id })}
            key={router.query.entityName as string}
            entityName={router.query.entityName as string}
            entity={entity}
            viewName={selectedView} showViewSelector={false}
            locale={app.locale} />

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
