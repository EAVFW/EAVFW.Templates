import { createContext, useContext } from "react";

function replaceLiteral(body: string, args: any[]) {
    var iterLiteral = "{(.*?)}";
    let i = 0;
    var re = new RegExp(iterLiteral, "g");
    return body.replace(re, (s) => s.startsWith("{@") ? JSON.stringify(args[i++]) : args[i++]);
}

const insights_context = createContext({

    log: (str: string, ...arg: any[]) => {
       
        // #!if ENVIRONMENT !== 'production'
        console.log(replaceLiteral(str, arg), arg);
        // #!endif
    }
});

export const useAppInsight =()=> useContext(insights_context);