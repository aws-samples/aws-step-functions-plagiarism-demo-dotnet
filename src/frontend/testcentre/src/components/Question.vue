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
export default {
  name: 'Question',
   data() {
    return {
      selected: '',
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
  methods: {
    questionAnswered() {
      this.$emit('record-answer', this.questionId, this.selected);
    }
  }
}
</script>
