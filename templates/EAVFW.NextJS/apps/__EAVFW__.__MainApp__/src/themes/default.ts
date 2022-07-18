import { createTheme } from "@fluentui/react";
import { RegisterFeature } from "@eavfw/apps";

const defaultTheme = createTheme({
    semanticColors: {
        actionLink:'#4a9c35'
    },
    palette: {
        themePrimary: '#4a9c35',
        themeLighterAlt: '#030602',
        themeLighter: '#0c1908',
        themeLight: '#162f10',
        themeTertiary: '#2c5d20',
        themeSecondary: '#41892f',
        themeDarkAlt: '#57a543',
        themeDark: '#6bb359',
        themeDarker: '#8cc77e',
        neutralLighterAlt: '#f1ece9',
        neutralLighter: '#ede8e6',
        neutralLight: '#e3dfdc',
        neutralQuaternaryAlt: '#d3d0cd',
        neutralQuaternary: '#cac6c4',
        neutralTertiaryAlt: '#c2bebc',
        neutralTertiary: '#a2afca',
        neutralSecondary: '#5a6d95',
        neutralPrimaryAlt: '#2a3c64',
        neutralPrimary: '#1b2c50',
        neutralDark: '#14213c',
        black: '#0f182c',
        white: '#f8f3f0',
    }
});
 

export default defaultTheme;



RegisterFeature("defaultTheme", defaultTheme);
RegisterFeature("topBarTheme", defaultTheme);