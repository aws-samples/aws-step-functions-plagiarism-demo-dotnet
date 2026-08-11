import coreWebVitals from 'eslint-config-next/core-web-vitals';

export default [
    ...coreWebVitals,
    {
        ignores: ['.next/**', 'out/**', 'node_modules/**'],
    },
];
