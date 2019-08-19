<template>
  <div id="exam-integration">
    <div class="box">
      <div class="content">
        <h1 class="subtitle is-4">Test Step Functions Integration</h1>
        <div id="meta" class="field is-grouped is-grouped-multiline">
          <div class="control">
            <div class="tags has-addons">
              <span class="tag">Incident ID</span>
              <span class="tag is-info">{{examData.IncidentId}}</span>
            </div>
          </div>
          <div class="control">
            <div class="tags has-addons">
              <span class="tag">Exam ID</span>
              <span class="tag is-warning">{{examData.ExamId}}</span>
            </div>
          </div>
          <div class="control">
            <div class="tags has-addons">
              <span class="tag">Task Token</span>
              <span id="task-token-preview" class="tag is-danger">{{examData.TaskToken}}</span>
            </div>
          </div>
          <hr />
        </div>
        <p>
          This application expects three GET variables to be passed in the URL:
            <ul>
              <li><span class="is-family-monospace">IncidentId</span> - the Incident this attempt relates to.</li>
              <li><span class="is-family-monospace">ExamId</span> - the unique identifier for this particular attempt.</li>
              <li><span class="is-family-monospace">TaskToken</span> - a callback task token for this Step Function execution.</li>
            </ul>
        </p>
        <p>
          You can click the button below to pass some dummy variables, to check the integration is working.
        </p>
      </div>

      <div class="level">
        <div class="level-left">
          <div class="control level-item">
              <a class="button is-info" href="?IncidentId=foo123&ExamId=bar456&TaskToken=AAAAKgAAAAIAAAAAAAAAASWo0UxfuXhTgRdqc1rySg1PZc/6rKbRHisQGn/pi89D8fIVujH6v3RS2DnWLIyPkgfJnI0H+WkRFoz8wXhfEgVvz6xRHEPURX2oi/eMINBFbGev89Kydex7Q3fi0mELPrD/gyfKsKwwsPPjXRPj1G4sDXwP3tZYdKGD/f1GfR04IIUB1uAKHSu6zTeQglzm+6cTbxAcGqcBgg6aIcUCY2PoA0Rk+mLzu/Y7+rY0FwQQCl0Npb8R8VLzEC2guM67oDFd9pKWbhEDkB700wNVQvpYAr6bvHFlbpVtXwoD16T8rZ0c802nvCEl7MvW/t+zxBSxbidGWsvXhF172iWpWcPnsEPUBjrcDglf/6MgRxzNIc4VXmRRp/GYBrEnYU1qf0qkeOQb0Fuq30nxtOvRcYp+WwFOMNXDpezhgu4LGzYT2J69+0mljvRRGNMXt0hG52fysjL53j4IMxZF/Hqy3ZW+DlkbFW38ePZufuBwsuT2oUbc1IR3+W5pZZ0OAuJLWDjHQRAsDVEaVQyjsHMynm13wFYPUvFXUorORLX9Y8fI8Fphp71ZDMPvbtqAhfc4Z25x08abQXSMKoXsmwNAaQssvyT42qXO79UnlfbsgALavU6Q2oLgUSmAX0Ou9lhgIVYhHfZDvX9lGTnlRmFdhz4">
              Test GET variables</a>
          </div>

          <div class="control level-item">
              <a @click="submitToStepFunctions" class="button is-black" href="#">Submit to Step Function Execution</a>
          </div>
        </div>
      </div>

      <!--
        Hidden notification messages which are shown depending on the outcome
        of a POST to our APIGW endpoint.
      -->

      <div v-if='submitSuccessful' class="notification success exam-submitted is-primary">
        <button v-on:click="deleteNotification" class="delete"></button>
        Your exam has been submitted. Your score was <strong>{{examData.Score}}%</strong> Your assessor will be in touch to let you
        know what the next steps are.
      </div>

      <div v-if='submitFailed' class="notification failure submission-failed is-error">
        <button v-on:click="deleteNotification" class="delete"></button>
        <p>There was an error when submitting your exam:</p>
        <br />
        <pre>{{submitErrorMessage}}</pre>
      </div>
      </div>
  </div>
</template>


<script>
// Use the event bus to respond when a score is recorded from the exam.
import ExamEventBus from './ExamEventBus';

export default {
  data() {
    return {
      examData: {
        // Exam score (out of 100).
        Score: '',
        // Unique identifier for plagiarism incident.
        IncidentId: 'Not supplied',
        // Unique identifier for exam attempt.
        ExamId: 'Not supplied',
        // Task Token unique to the current Step Functions execution.
        TaskToken: 'Not supplied'
      },
      // Set to true if a POST to our API is successful.
      submitSuccessful: false,
      // Set to true if a POST to our API fails.
      submitFailed: false,
      submitErrorMessage: ''
    }
  },
  // If present, get the primary keys, then set our hidden form value.
  // Keys can get passed as a GET variables,
  // in the form ?IncidentId=foo&ExamId=bar
  created: function() {
      let uri = window.location.search;
      let params = new URLSearchParams(uri);
      this.examData.IncidentId = params.get('IncidentId') || this.examData.IncidentId;
      this.examData.ExamId = params.get('ExamId') || this.examData.ExamId;
      // UrlSearchParams uses parsing rules which may alter the token;
      // get the raw value instead.
      let url = new URL(window.location.href);
      const regex = /TaskToken=(.*)&/gm;
      let taskToken = regex.exec(url.search);
      if (taskToken.length >= 2) {
        this.examData.TaskToken = taskToken[1];
      }
  },

  mounted: function() {
    // Register a listener to record a score when an exam is submitted.
    // @see Exam.vue::examSubmitted().
    ExamEventBus.$on('examSubmitted', score => {
      this.examData.Score = score;
    });
  },

  methods: {
    submitToStepFunctions: function(event) {
      event.preventDefault();
      // Post our response back to Step Functions to continue the flow.
      let apiName = 'PlagiarismStepFunctionsDemo';
      let path = '/exam';
      let myInit = {
        body: this.examData
      };
      this.$Amplify.API.post(apiName, path, myInit).then(response => {
        this.submitSuccessful = true;
        // Let the exam component know that this exam has been sent back.
        ExamEventBus.$emit('examPostedSuccessfully');

      }).catch(error => {
        // Something went wrong.
        this.submitFailed = true;
        // Hack to get endpoint string.
        let endpointTried = this.$Amplify.API._options.endpoints[0].endpoint + path;
        this.submitErrorMessage = error + ` (tried ${endpointTried})`;
      });
    },
    deleteNotification: function(event) {
      // Allow failed attempts to close the notification and try again.
      if (event.target.parentNode.classList.contains('failure')) {
        this.submitFailed = false;
      }
      // Allow successful submissions to close the notification.
      if (event.target.parentNode.classList.contains('success')) {
        this.submitSucessful = false;
      }
    }
  }
}
</script>