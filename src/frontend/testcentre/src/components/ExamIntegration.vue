<template>
  <div id="exam-integration">
    <div class="box">
      <div class="content">
        <h1 class="subtitle is-4">Test Step Functions Integration</h1>
        <div id="meta" class="field is-grouped is-grouped-multiline">
          <div class="control">
            <div class="tags has-addons">
              <span class="tag">Incident ID</span>
              <span class="tag is-link is-info">{{examData.IncidentId}}</span>
            </div>
          </div>
          <div class="control">
            <div class="tags has-addons">
              <span class="tag">Exam ID</span>
              <span class="tag is-link is-warning">{{examData.ExamId}}</span>
            </div>
          </div>
          <hr />
        </div>
        <p>
          This application expects two GET variables to be passed in the URL:
            <ul>
              <li><span class="is-family-monospace">IncidentId</span> - the Incident this attempt relates to.</li>
              <li><span class="is-family-monospace">ExamId</span> - the unique identifier for this particular attempt.</li>
            </ul>
        </p>
        <p>
          You can click the button below to pass some dummy variables, to check the integration is working.
        </p>
      </div>

      <div class="level">
        <div class="level-left">
          <div class="control level-item">
              <a class="button is-info" href="?IncidentId=foo123&ExamId=bar456">Test GET variables</a>
          </div>

          <div class="control level-item">
              <a @click="submitToStepFunctions" class="button is-black" href="?IncidentId=foo123&ExamId=bar456">Submit to Step Function Execution</a>
          </div>
        </div>
      </div>

      <!--
        Hidden notification messages which are shown depending on the outcome
        of a POST to our APIGW endpoint.
      -->

      <div  class="notification success exam-submitted is-primary">
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
      let uri = window.location.search.substring(1);
      let params = new URLSearchParams(uri);
      this.examData.IncidentId = params.get('IncidentId') || this.examData.IncidentId;
      this.examData.ExamId = params.get('ExamId') || this.examData.ExamId;
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
      let path = '/submitExam';
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
        this.submitErrorMessage = error;
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