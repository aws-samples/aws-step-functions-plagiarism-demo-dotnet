import Vue from 'vue';
import App from './App.vue';

import Amplify, * as AmplifyModules from 'aws-amplify';
import { AmplifyPlugin } from 'aws-amplify-vue';

import awsconfig from '../awsconfig.json';

// Update our API Gateway config if variables are provided in a .env file.
// The .env file should be added to the root of the project.
if (process.env.VUE_APP_APIGW_ENDPOINT) {
   awsconfig.API.endpoints[0].endpoint = process.env.VUE_APP_APIGW_ENDPOINT;
}

Amplify.configure(awsconfig);

Vue.use(AmplifyPlugin, AmplifyModules);
Vue.config.productionTip = false;

new Vue({
  render: h => h(App),
}).$mount('#app');
