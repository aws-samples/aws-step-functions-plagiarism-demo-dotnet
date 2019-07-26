import Vue from 'vue';
import App from './App.vue';

import Amplify, * as AmplifyModules from 'aws-amplify';
import { AmplifyPlugin } from 'aws-amplify-vue';

Amplify.configure({
  aws_project_region: "ap-southeast-2",
  API: {
    endpoints: [
      {
        name: "PlagiarismStepFunctionsDemo",
        endpoint: "https://1234567890-abcdefgh.amazonaws.com"
      }
    ]
  }
});

Vue.use(AmplifyModules, AmplifyPlugin);

Vue.config.productionTip = false;

new Vue({
  render: h => h(App),
}).$mount('#app');
