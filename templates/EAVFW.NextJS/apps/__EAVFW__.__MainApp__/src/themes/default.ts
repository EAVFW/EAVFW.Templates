import { createTheme } from "@fluentui/react";
import { RegisterFeature } from "@eavfw/apps";
import { createV9Theme } from "@fluentui/react-migration-v8-v9";

const defaultTheme = createTheme({
    semanticColors: {
        actionLink: '#4a9c35'
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
        neutralPrimary: '#094080',
        neutralDark: '#14213c',
        black: '#0f182c',
        white: '#f8f3f0',
    }
});


const topBarTheme = createTheme({
    palette: {
        themePrimary: '#ffffff',
        themeLighterAlt: '#767676',
        themeLighter: '#a6a6a6',
        themeLight: '#c8c8c8',
        themeTertiary: '#d0d0d0',
        themeSecondary: '#dadada',
        themeDarkAlt: '#eaeaea',
        themeDark: '#f4f4f4',
        themeDarker: '#f8f8f8',
        neutralLighterAlt: '#203157',
        neutralLighter: '#25375d',
        neutralLight: '#2e4069',
        neutralQuaternaryAlt: '#33466f',
        neutralQuaternary: '#384c75',
        neutralTertiaryAlt: '#50638c',
        neutralTertiary: '#c8c8c8',
        neutralSecondary: '#d0d0d0',
        neutralPrimaryAlt: '#dadada',
        neutralPrimary: '#ffffff',
        neutralDark: '#f4f4f4',
        black: '#f8f8f8',
        white: '#094080',
    }
});


export default defaultTheme;

RegisterFeature("defaultTheme", defaultTheme);
RegisterFeature("topBarTheme", topBarTheme);
RegisterFeature("defaultV2Theme", createV9Theme(defaultTheme));
RegisterFeature("topBarV2Theme", createV9Theme(topBarTheme));