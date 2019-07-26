<template>
  <div class="exam">
    <form
      class="exam-form"
      @submit.prevent="examSubmit"
    >
    <div class="box">
      <h1 class="title">Exam Attempt</h1>
      <div class="field">
        <div id="exam-progress" class="is-flex">
          <div class="progress-bar">
            <progress class="progress is-primary" :value="questions_answered" :max="total_questions">15%</progress>
          </div>
          <div class="progress-text tags has-addons">
            <span class="tag">Progress</span>
            <span class="tag is-link is-warning">{{questions_answered}} / {{total_questions}}</span>
          </div>
        </div>
      </div>
      <!--
        As we are iterating over an array, the second value is index, rather than the
        third value, which would be index if we were iterating over an object.
      -->
      <div v-for="(question, index) in QuestionData.questions" v-bind:key="question.question_id">
        <Question
          v-on:record-answer="onRecordAnswer"
          v-bind:questionText="question.text"
          v-bind:questionNumber="index"
          v-bind:answers="question.answers"
          v-bind:questionId="question.question_id"
        />
      </div>

      <div class="field submit is-flex">
          <div class="control">
            <input class="button is-primary" type="submit" value="Submit Exam" />
          </div>
          <div id="score-display" class="control">
            <div class="tags are-medium has-addons">
              <span class="tag">Your Score</span>
              <span class="tag is-dark">{{score}}%</span>
            </div>
          </div>
      </div>
    </div>
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
      // Total number of questions in exam.
      total_questions: 0,
      // Number of questions answered.
      questions_answered: 0,
      // The user's exam score, out of 100.
      score: 0
    }
  },
  created() {
    this.total_questions = QuestionData.questions.length;
  },

  methods: {
    /**
     * Reacts when form is submitted. Calculates score and puts it back into the workflow.
     */
    examSubmit() {
      this.calculateScore();
    },
    /**
     * Reacts to a Question select event.
     */
    onRecordAnswer(questionId, answerGiven){
      // Update the progress bar if this question has not already been answered.
      if (!this.answers[questionId]) {
        this.questions_answered++;
      }
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