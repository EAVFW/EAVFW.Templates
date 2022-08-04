import { JSONSchema7 } from "json-schema";

declare module 'json-schema' {
    export interface JSONSchema7 {
        enumNames?: string[];
        "x-control"?: string;
        "x-description"?: string;
        'x-widget-props'?: {
            attributeName?: string,
            entityName?: string,
            fieldName?: string,
            formName?: string
        }
    }
}


//export type RJSFError = {
//    name: string;
//    message: string;
//    params: { multipleOf: number };
//    property: string;
//    stack: string;
//    schemaPath: string;
//}


