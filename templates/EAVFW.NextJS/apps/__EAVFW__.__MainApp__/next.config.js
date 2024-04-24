const { env } = require("process");
const path = require("path");
//const MiniCssExtractPlugin = require("mini-css-extract-plugin");
//https://github.com/vercel/next.js/issues/10497
const withTM = require('next-transpile-modules')((env.NEXT_TRANSPILE_MODULES||'').split(' ')); // pass the modules you would like to see transpiled
//console.log(withTM);

//console.log(env);
module.exports = withTM({
    output: env.NODE_ENV === "production" ? 'export' : undefined,
    distDir: env.NODE_ENV === "production" ? 'wwwroot' : undefined,
    trailingSlash: true,
    webpack: (config, { defaultLoaders }) => {
        console.log(env.NODE_ENV === "production" ? 'SSG' : 'SSR');

        config.resolve.alias.react = path.resolve(__dirname, '../../node_modules/react');
        config.resolve.alias['react-dom'] = path.resolve(__dirname, '../../node_modules/react-dom');
       // config.resolve.alias['@fluentui/react'] = path.resolve(__dirname, '../../node_modules/@fluentui/react');
        config.resolve.alias['@fluentui/merge-styles'] = path.resolve(__dirname, '../../node_modules/@fluentui/merge-styles');

 

        //config.module.rules.push({
        //    test: /\.(ts)x?$/, // Just `tsx?` file only
        //    use: [
        //        // options.defaultLoaders.babel, I don't think it's necessary to have this loader too
        //        {
        //            loader: "ts-loader",
        //            options: {
        //                transpileOnly: true,
        //                experimentalWatchApi: true,
        //                onlyCompileBundledFiles: true,
        //            },
        //        },
        //    ],
        //});

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
})
