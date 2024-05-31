
import React from "react";
import { useRouter } from "next/router";
import { Controls, useModelDrivenApp } from "@eavfw/apps";


export default function DashboardPage() {

    const router = useRouter();
    const app = useModelDrivenApp();

    if (!router.query.dashboard)
        return <div>loading</div>

    const dashboard = app.getDashboard(router.query.dashboard as string);

    return dashboard?.control ? Controls[dashboard?.control]() : <div>NoDashboardConfigured</div>
}

const props = {
    layout: "FormLayout",
    authorize: true
};

// #!if EXPORTS_FOR_BUILD === 'SSG'
DashboardPage.getInitialProps = () => props;
// #!endif

// #!if EXPORTS_FOR_BUILD === 'SSR'
export const getServerSideProps = () => ({ props: props });
// #!endif
