import { useModelDrivenApp, useUserProfile } from "@eavfw/apps";
import { IDropdownOption } from "@fluentui/react";
import { useRouter } from "next/dist/client/router";
import React, { useEffect, useState } from "react";


export default function Home() {

    const app = useModelDrivenApp();
    const router = useRouter();
    const [selectedArea, setselectedArea] = useState<string>();
    const user = useUserProfile();

  
   
    const sitemap = app.sitemap;
    useEffect(() => {
        try {
         
            const areas = Object.keys(sitemap.areas).filter(area => {
                console.log("area to filter");
                console.log(sitemap.areas[area]);
                console.log(user);

                if (!user)
                    return true;

                let noRoleInfoDefined = true;
                for (let g of Object.keys(sitemap.areas[area])) {
                    let group = sitemap.areas[area][g];
                    for (let e of Object.keys(group)) {
                        let roles = group[e].roles;

                        console.log(roles);
                        if (roles) {
                            noRoleInfoDefined = false;
                            if (roles?.allowed?.filter(role => user.role.filter((r: string) => role === r).length > 0)?.length ?? 0 > 0) {
                                return true;
                            }
                        }
                    }
                }

                return noRoleInfoDefined;
            }).map(area => ({ key: area, text: area, id: area } as IDropdownOption));
             

            setselectedArea(router.query.area as string ?? areas[0]?.key);
        } finally {
            console.groupEnd();
        }

    }, [router.query.area, user]);

    const dashboard = Object.values(sitemap.dashboards[selectedArea!] ?? {})[0];

    if (!dashboard?.url)
        return <div>No Dashboard Configured</div>

    return (
        <iframe style={{ width:'100%', height:'100%' }} src={dashboard.url} />
    );
}



const props = {
    layout: "PageLayout",
    authorize:true
};

// #!if EXPORTS_FOR_BUILD === 'SSG'
Home.getInitialProps = () => props;
// #!endif

// #!if EXPORTS_FOR_BUILD === 'SSR'
export const getServerSideProps = () => ({ props: props });
// #!endif
