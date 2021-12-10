const {env} = require("process");

//https://github.com/vercel/next.js/issues/10497
const withTM = require('next-transpile-modules')((env.NEXT_TRANSPILE_MODULES || '').split(' ')); // pass the modules you would like to see transpiled


module.exports = withTM({

    trailingSlash: true,
    webpack: (config, {defaultLoaders}) => {
        console.log(env.NODE_ENV === "production" ? 'SSG' : 'SSR');

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
