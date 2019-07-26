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
      <div class="control">
          <a class="button is-info" href="?IncidentId=foo123&ExamId=bar456">Test GET variables</a>
      </div>
      <div class="control">
          <a @click="submitToStepFunctions" class="button is-info" href="?IncidentId=foo123&ExamId=bar456">Submit to Step Function Execution</a>
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
      }
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
        console.log(response);
      }).catch(error => {
        console.log(error.response);
      });
      console.log(this.examData);
    }
  }
}
</script>