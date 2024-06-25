const { env } = require("process");
const path = require("path");
//const MiniCssExtractPlugin = require("mini-css-extract-plugin");
//https://github.com/vercel/next.js/issues/10497
const withTM = require('next-transpile-modules')((env.NEXT_TRANSPILE_MODULES||'').split(' ')); // pass the modules you would like to see transpiled
//console.log(withTM);

//console.log(env);
module.exports = {
    basePath: env.BUILD_BASE_PATH === '/' ? '' : env.BUILD_BASE_PATH,
    output: env.NODE_ENV === "production" ? 'export' : undefined,
    distDir: env.NODE_ENV === "production" ? 'wwwroot' : undefined,
    trailingSlash: true,
    env: {
        APPLICATION_INSIGHTS_NO_STATSBEAT: "1",
        ENV_DISABLE_STATSBEAT: "1",
        NEXT_PUBLIC_BASE_PATH: env.BUILD_BASE_PATH
    },
    compress: false,
    productionBrowserSourceMaps: true,
    webpack: (config, { defaultLoaders }) => {
        console.log(env.NODE_ENV === "production" ? 'SSG' : 'SSR');

        config.resolve.alias.react = path.resolve(__dirname, '../../node_modules/react');
        config.resolve.alias['react-dom'] = path.resolve(__dirname, '../../node_modules/react-dom');


        config.resolve.alias['@fluentui/react'] = path.resolve(__dirname, '../../node_modules/@fluentui/react');
        config.resolve.alias['@fluentui/react-components'] = path.resolve(__dirname, '../../node_modules/@fluentui/react-components');
        config.resolve.alias['@fluentui/merge-styles'] = path.resolve(__dirname, '../../node_modules/@fluentui/merge-styles');
        config.resolve.alias['@rjsf/fluentui-rc'] = path.resolve(__dirname, '../../node_modules/@rjsf/fluentui-rc');


        config.resolve.alias['@eavfw/forms'] = path.resolve(__dirname, '../../node_modules/@eavfw/forms');
        config.resolve.alias['@eavfw/manifest'] = path.resolve(__dirname, '../../node_modules/@eavfw/manifest');
        config.resolve.alias['@eavfw/apps'] = path.resolve(__dirname, '../../node_modules/@eavfw/apps');
        config.resolve.alias['@eavfw/hooks'] = path.resolve(__dirname, '../../node_modules/@eavfw/hooks');
        config.resolve.alias['@eavfw/expressions'] = path.resolve(__dirname, '../../node_modules/@eavfw/expressions');
        config.resolve.alias['@eavfw/designer'] = path.resolve(__dirname, '../../node_modules/@eavfw/designer');
        config.resolve.alias['@eavfw/designer-nodes'] = path.resolve(__dirname, '../../node_modules/@eavfw/designer-nodes');
        config.resolve.alias['@eavfw/designer-core'] = path.resolve(__dirname, '../../node_modules/@eavfw/designer-core');
        config.resolve.alias['@eavfw/utils'] = path.resolve(__dirname, '../../node_modules/@eavfw/utils');


        config.resolve.alias['@craftjs/core'] = path.resolve(__dirname, '../../node_modules/@craftjs/core');
        config.resolve.alias['@rjsf/core'] = path.resolve(__dirname, '../../node_modules/@rjsf/core');
        config.resolve.alias['@rjsf/utils'] = path.resolve(__dirname, '../../node_modules/@rjsf/utils');

        config.module.rules.push({
            test: /\.svg$/,
            use: ["@svgr/webpack"]
        });

        config.module.rules.push({
            test: /\.html$/,
            loader: "html-loader",
            options: {
                esModule: true,
                sources: false,
                minimize: {
                    removeComments: false,
                    collapseWhitespace: false
                }
            }
        });
        //config.module.rules.push({
        //    test: /\.css$/i,
        //    loader: "css-loader",
        //    options: { url: false }
        //});
        //config.module.rules.push({
        //    test: /\.scss$/,
        //    use: [
        //        // Creates `style` nodes from JS strings
        //        "style-loader",
        //        // Translates CSS into CommonJS
        //        "css-loader",
        //        // Compiles Sass to CSS
        //        "sass-loader",
        //    ]
        // });
        //config.plugins.push(new MiniCssExtractPlugin({
        //    filename: "[name].css",
        //    chunkFilename: "[id].css"
        //}));

        const preprocessLoader = {
            test: /\.tsx$/,
            use: [
                {
                    loader: 'webpack-preprocessor-loader',
                    options: {
                        debug: true,
                        params: {
                            EXPORTS_FOR_BUILD: env.NODE_ENV === "production" ? 'SSG' : 'SSR', // or "SSG", to be set via environment variables.
                        },
                        verbose: true
                    },
                },
            ],
        };
        config.module.rules.push(preprocessLoader);


        return config;
    },
}
