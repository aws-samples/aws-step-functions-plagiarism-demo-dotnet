<template>
  <div id="app">
    <Exam />
    <ExamIntegration />
  </div>
</template>

<script>
import Exam from './components/Exam.vue';
import ExamIntegration from './components/ExamIntegration.vue';
import Question from './components/Question.vue';

export default {
  name: 'app',
  components: {
    Exam,
    ExamIntegration,
  }
}
</script>

<style>
#app {
  font-family: 'Avenir', Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
  margin-top: 60px;
}
</style>
