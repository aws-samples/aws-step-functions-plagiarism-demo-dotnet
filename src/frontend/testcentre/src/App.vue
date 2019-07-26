<template>
  <div id="app">
    <Exam />
    <ExamIntegration />
  </div>
</template>

<script>
import Exam from './components/Exam.vue';
import ExamIntegration from './components/ExamIntegration.vue';

export default {
  name: 'app',
  components: {
    Exam,
    ExamIntegration,
  }
}
</script>

<style lang="scss">
  @import "./assets/custom.scss";
</style>
