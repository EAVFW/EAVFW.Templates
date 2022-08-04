
import React, { useContext, useEffect, useState } from "react";

import { useRouter } from "next/router";
 

import { useMemo } from "react";

import { ModelDrivenEntityViewer, ResolveFeature, RouterTabProvider, useDefaultMainRibbonItems, useFormChangeHandler, useModelDrivenApp } from "@eavfw/apps";


 

 
export default function FormPage() {

    const app = useModelDrivenApp();
    const router = useRouter();
    const defaultConfig = ResolveFeature("formsConfig");



    if (!router.query.area || !router.query.entityName || !router.query.recordId)
        return <div>page loading</div>

    const entity = app.getEntity(router.query.entityName as string);
    const formName = router.query.formname as string;
    const recordId = router.query.recordId as string;

    const { onChangeCallback, record, isLoading, extraErrors } = useFormChangeHandler(entity, recordId);
    console.log("recordState", record, isLoading);

    useDefaultMainRibbonItems({ ...entity.forms?.[formName]?.ribbon, delete: { visible: false } }, router.push);
 
    const related = useMemo(() => app.getRelated(entity.logicalName), [entity.logicalName]);

    if (isLoading) return <div>loading</div>;

     
    return <RouterTabProvider>
        <ModelDrivenEntityViewer related={related} onChange={onChangeCallback} record={record} formName={formName} entityName={router.query.entityName as string} {...defaultConfig} {...app.getFormsConfig()} entity={entity} locale={app.locale} extraErrors={extraErrors} />
    </RouterTabProvider>


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
