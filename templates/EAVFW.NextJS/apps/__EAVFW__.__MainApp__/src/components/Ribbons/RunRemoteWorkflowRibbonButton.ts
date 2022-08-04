import { RegistereRibbonButton, useAppInfo, useModelDrivenApp, useRibbon } from "@eavfw/apps";


RegistereRibbonButton("RUN_REMOTE_WORKFLOW", ({ key, ...props }) => {
    const { registerButton, events } = useRibbon();
    const { currentEntityName, currentRecordId } = useAppInfo();


    const app = useModelDrivenApp();

    registerButton({
        key: key,
        text: props.text ?? "Run Workflow",
        iconProps: props.iconProps ?? { iconName: 'Send' },
        title: props.title ?? props.text ?? "Run Workflow",
        disabled: false,
        onClick: () => {

            //@ts-ignore
            let rsp = fetch(`${process.env.NEXT_PUBLIC_API_BASE_URL}/entities/${app.getEntity(currentEntityName).collectionSchemaName}/records/${currentRecordId}/workflows/${props.workflowName}/runs`, {
                method: "POST",
                body: JSON.stringify({}),
                credentials: "include"
            });
        }
        //workflow: props.workflow
        // onClick: onClick
    });

});