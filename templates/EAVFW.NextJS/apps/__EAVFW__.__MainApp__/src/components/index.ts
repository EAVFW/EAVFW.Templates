import { RegisterFeature } from "@eavfw/apps";
import "./Ribbons";
export * from "./CustomControlDemo";

RegisterFeature("WizardExpressionsProvider", (formValues: any) => {
    console.log("WizardExpressionsProvider", [formValues]);

    return {

    }
});

RegisterFeature("ExpressionsProviderAsync", (formValues: any, expression: string) => {
    console.log("ExpressionsProviderAsync", [formValues, expression]);
});