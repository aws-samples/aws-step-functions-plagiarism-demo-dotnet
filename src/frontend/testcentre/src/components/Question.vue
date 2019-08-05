<template>
  <div class="field box question">
    <label className="label">
      <strong> Question {{questionNumber + 1}}:</strong>
      <em> {{questionText}}</em>
      </label>
    <div class="control">
      <div class="select is-fullwidth">
        <select
          required
          :disabled="exam_disabled == 1"
          @change="questionAnswered"
          v-model="selected"
        >
          <option disabled selected value="">Please select one</option>
          <option
            v-for="(text, value) in answers"
            v-bind:value="value"
            v-bind:key="value"
          >
           {{text}}
          </option>
        </select>
      </div>
    </div>
    <p class="help">Choose the best option.</p>
  </div>
</template>

<script>
import ExamEventBus from './ExamEventBus';

export default {
  name: 'Question',
   data() {
    return {
      selected: '',
      exam_disabled: false
    }
  },
  props: {
    // Unique identifier for question.
    questionId: String,
    // Question sequence number (index in array of questions).
    questionNumber: Number,
    // Text of questions.
    questionText: String,
    // An Object map of answers for the question.
    answers: Object,
  },
  mounted() {
    // Disable the form if this exam has been sent back to StepFunctions.
    ExamEventBus.$on('examPostedSuccessfully', () => {
      this.exam_disabled = true;
    });
  },
  methods: {
    questionAnswered() {
      this.$emit('record-answer', this.questionId, this.selected);
    }
  }
}
</script>
