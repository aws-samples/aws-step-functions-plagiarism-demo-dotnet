<template>
  <div className="question">
      <label className="label">{{questionText}}
     <select
        required
        v-model="selected"
        @change="questionAnswered"
      >
        <option disabled value="">Please select one</option>
        <option
          v-for="(text, value) in answers"
          v-bind:value="value"
          v-bind:key="value"
        >
          {{ text }}
        </option>
      </select>
      <span>Selected: {{ selected }}</span>
      </label>
    </div>
</template>

<script>
export default {
  name: 'Question',
  data: function() {
    return {
        selected: ''
    }
  },
  props: {
    questionId: String,
    questionText: String,
    answers: Object,
  },
  methods: {
    questionAnswered() {
      this.$emit('record-answer', this.questionId, this.selected);
    }
  }
}
</script>
