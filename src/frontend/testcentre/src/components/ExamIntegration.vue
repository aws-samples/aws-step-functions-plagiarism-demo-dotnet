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
    </div>
  </div>
</template>

<script>
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
  }
}
</script>