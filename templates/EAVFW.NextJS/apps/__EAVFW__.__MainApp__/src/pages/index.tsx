import React, { useContext } from "react";

import Link from "next/link";


import { DefaultEffects, Stack } from "@fluentui/react";
import { useModelDrivenApp, useUserProfile } from "@eavfw/apps";
import {
    filterRoles
} from "@eavfw/apps";


export type IndexPageProps = {};

 

export default function Home(props: IndexPageProps) {
    console.group("Component: Home");
    console.log(props);
   
    console.groupEnd();

    const user = useUserProfile();
    const app = useModelDrivenApp();
    console.log(user);
    return (
         
            <Stack verticalFill>
                <h1 style={{ textDecoration: "underline solid" }}>Apps</h1>

                <Stack horizontal gap={8} style={{ marginTop: 16 }}>
                {app.getApps()
                    .filter(([appKey, appv]) => (!appv?.roles) || filterRoles(appv?.roles, user))
                    .map(([appKey,appv]) => (
                        <div key={appKey} style={{
                            width: 200,
                            height: 200,
                            margin: 8,
                            padding:8,
                            background: "white",
                            justifyContent: "center",
                            alignItems: "center",
                            boxShadow: DefaultEffects.elevation4
                        }}>
                            <Link href={`/apps/${appKey}`} >
                                {appKey}
                            </Link>
                        </div>
                    ))}
                </Stack>

            </Stack>
        
    );
}

// add getStaticProps() function
export function getStaticProps() {

    return {
        props: {
            layout: "AppPickerLayout",
            authorize: true
        }
    }
}
