<template>
  <div id="exam-integration">
    <a href="?IncidentId=foo123&ExamId=bar456">Test GET vars</a>
    <div class="form-group">
        <input type="text" id="IncidentId" v-model="examData.IncidentId" />
        <input type="text" id="ExamId" v-model="examData.ExamId" />
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
          IncidentId: '',
          // Unique identifier for exam attempt.
          ExamId: '',
        }
       }
      },
      // If present, get the primary keys, then set our hidden form value.
      // Keys can get passed as a GET variables,
      // in the form ?IncidentId=foo&ExamId=bar
      created: function() {
          let uri = window.location.search.substring(1);
          let params = new URLSearchParams(uri);
          this.examData.IncidentId = params.get('IncidentId');
          this.examData.ExamId = params.get('ExamId');
      }
}

</script>