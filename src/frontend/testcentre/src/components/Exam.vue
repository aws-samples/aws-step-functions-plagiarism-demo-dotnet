<template>
  <div class="exam">
    <h1>AWS Step Functions Plagiarism Exam</h1>
    <form
      class="exam-form"
      @submit.prevent="onSubmit"
    >
      <li v-for="question in QuestionData.questions" v-bind:key="question.question_id">
        <Question
          v-on:record-answer="onRecordAnswer"
          v-bind:questionText="question.text"
          v-bind:answers="question.answers"
          v-bind:questionId="question.question_id"
        />
      </li>
      <input type="submit" value="Submit" />
      <p><em>Your score:</em> {{score}}</p>
    </form>
  </div>
</template>

<script>
import QuestionData from './questionData.json';
import Question from './Question.vue';
export default {
  name: "Exam",
  components: {
      Question
  },
  data() {
    return {
      // Static question data loaded from a config file.
      QuestionData: QuestionData,
      // A map of the answers the user has given. Updated by downstream
      // components.
      answers: {},
      // The user's exam score, out of 100.
      score: 0
    }
  },
  created() {
    console.log(QuestionData.questions, 'questionData.questions');
  },

  methods: {
    onSubmit(event) {
      this.calculateScore();
    },
    /**
     * Reacts to a Question select event.
     */
    onRecordAnswer(questionId, answerGiven){
      // Record a student answer for later comparison.
      this.answers[questionId] = answerGiven;
    },
    /**
     * Calculate the user's exam score and save it to local data.
     */
    calculateScore() {
      // First, collate all the correct answers in one place.
      const correctAnswers = this.correctAnswers();
      // Then, iterate through the students answers to see how they did.
      const answerMap = new Map(Object.entries(this.answers));
      // Keep a running total of how many answers were answered correctly.
      let correctCount = 0;
      answerMap.forEach((answerGiven, questionId) => {
        if (correctAnswers[questionId] == answerGiven) {
            correctCount++;
        }
      });

      // Record a score out of 100.
      this.score = correctCount / answerMap.size * 100;
    },
    /**
     * Returns an Object, keys are questionIDs and values are correct answers.
     */
    correctAnswers() {
      // We use a map to make it easier to iterate over the questions.
      const questionsMap = new Map(Object.entries(this.QuestionData.questions));
      let correctAnswers = {};

      questionsMap.forEach((question) => {
        correctAnswers[question.question_id] = question.correct_answer;
      });
      return correctAnswers;
    }

  }

}
</script>