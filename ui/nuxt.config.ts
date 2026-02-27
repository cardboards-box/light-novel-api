// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
    compatibilityDate: '2025-07-15',
    devtools: { enabled: false },
    ssr: false,

    nitro: {
        preset: 'bun',
        typescript: {
            tsConfig: {
                compilerOptions: {
                    noUncheckedIndexedAccess: false
                }
            }
        }
    },

    app: {
        head: {
            link: [
                { rel: 'manifest', href: '/manifest.webmanifest' },
                { rel: 'preconnect', href: 'https://fonts.gstatic.com' },
                { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Roboto:wght@300;400;500&amp;display=swap' },
                { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Kolker+Brush&display=swap' },
                { rel: 'stylesheet', href: 'https://fonts.googleapis.com/icon?family=Material+Icons' },
                { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Material+Symbols+Outlined:opsz,wght,FILL,GRAD@20..48,100..700,0..1,-50..200' },
            ],
            meta: [
                { name: 'apple-mobile-web-app-capable', content: 'yes' },
                { name: 'viewport', content: 'initial-scale=1, width=device-width' },
                { name: 'theme-color', content: '#0c090c' },
            ]
        },
        pageTransition: { name: 'page', mode: 'out-in' }
    },

    runtimeConfig: {
        public: {
            apiUrl: process.env.API_URL || 'http://localhost:5133',
        }
    },

    css: [
        './node_modules/highlight.js/styles/vs2015.css',
        './styles/variables.scss',
        './styles/reset.scss',
        './styles/styles.scss',
        './styles/layout.scss',
        './styles/controls.scss',
    ],

    components: [
        '~/components/general',
        '~/components/general/tabs',
        '~/components/buttons',
        '~/components',
    ],

    modules: ['@nuxt/image'],
})
