
import React, { useCallback, useContext, useEffect, useMemo, useRef, useState } from "react";


import { NextRouter, useRouter } from "next/router";
  
import {
    ModelDrivenApp, FormsConfig, useModelDrivenApp,
    useAppInfo, useFormChangeHandler, useDefaultMainRibbonItems,
    RouterTabProvider,
    ModelDrivenEntityViewer,
    ResolveFeature
} from "@eavfw/apps";
import { EntityDefinition, getNavigationProperty, isLookup } from "@eavfw/manifest";

 


export default function CreateNewRecordPage() {

    const app = useModelDrivenApp();
    const router = useRouter();

    const defaultConfig = ResolveFeature("formsConfig");


    if (!router.query.area || !router.query.entityName)
        return <div>loading</div>

    const entity = app.getEntity(router.query.entityName as string);
    const formName = router.query.formname as string;
    const attributes = Object.values(entity.attributes).map(a => a.logicalName);
    // const [record, setRecord] = useState(Object.fromEntries(Object.keys(router.query).filter(logicalName => attributes.indexOf(logicalName) !== -1).map(k => [k, router.query[k]])));
    const entityName = router.query.entityName as string;

    const { onChangeCallback, record, extraErrors } = useFormChangeHandler(entity, undefined, Object.fromEntries(Object.keys(router.query).filter(logicalName => attributes.indexOf(logicalName) !== -1).map(k => [k, router.query[k]])));

   // useRibbonEffect(entity, formName);

    useDefaultMainRibbonItems({ ...entity.forms?.[formName]?.ribbon, delete: { visible:false } }, router.push);
    

    const related = useMemo(() => app.getRelated(entity.logicalName), [entity.logicalName]);

    //TODO - this is formKey from manifest and not name
    return (
        <RouterTabProvider>
            <ModelDrivenEntityViewer related={related} onChange={onChangeCallback} record={record} formName={formName} entityName={entityName} {...defaultConfig} {...app.getFormsConfig()} entity={entity} locale={app.locale} extraErrors={extraErrors} />
        </RouterTabProvider>
    )
}


const props = {
    layout: "FormLayout",
    authorize: true
};
// #!if EXPORTS_FOR_BUILD === 'SSG'
CreateNewRecordPage.getInitialProps = () => props;
// #!endif

// #!if EXPORTS_FOR_BUILD === 'SSR'
export const getServerSideProps = () => ({ props: props });
// #!endif
