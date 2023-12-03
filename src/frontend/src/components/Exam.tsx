'use client'

import Image from 'next/image'
import { useState } from 'react'
import QuestionData from './QuestionData.json'
import Question from './Question'

type ExamProps = {
    examSubmitted: boolean;
    setScore: (score: number) => void;
    score: number;
}

type QuestionType = {
    text: string;
    answers: object;
    correct_answer: string;
    question_id: string;
}

export default function Exam({ examSubmitted, setScore, score }: ExamProps) {
    const [questionsAnswered, setQuestionsAnswered] = useState(0);
    const totalQuestions: number = QuestionData.questions.length
    const [answers, setAnswers] = useState<Map<string, string>>(new Map<string, string>());
    const [isExamDisabled, setIsExamDisabled] = useState<boolean>(examSubmitted);

    /**
     * when a form is submitted. Calculates score and puts it back into the workflow.
     */
    function submitExam() {
        const score = calculateScore();
        // Let other components know what the score is.
        setScore(score)
    }

    /**
      * Calculate the user's exam score and save it to local data.
      */
    function calculateScore() {
        // First, collate all the correct answers in one place.
        let correctAnswers = new Map<string, string>();
        QuestionData.questions.forEach((question: QuestionType) => {
            correctAnswers.set(question.question_id, question.correct_answer);
        });

        // Then, iterate through the students answers to see how they did.
        const answerMap = new Map(Object.entries(answers));
        // Keep a running total of how many answers were answered correctly.
        let correctCount = 0;
        answerMap.forEach((answerGiven, questionId) => {
            if (correctAnswers.get(questionId) === answerGiven) {
                correctCount++;
            }
        });
        // Record a score out of 100.
        return correctCount / answerMap.size * 100;
    }

    /**
   * Reacts to a Question select event.
   */
    function recordAnswer(questionId: string, answerGiven: string) {
        // Update the progress bar if this question has not already been answered.
        if (!answers.get(questionId)) {
            setQuestionsAnswered(questionsAnswered + 1);
        }
        // Record a student answer for later comparison.
        answers.set(questionId, answerGiven);
    }



    return (
        <div className="exam">
            <form className="exam-form" >
                <div className="box">
                    <h1 className="title">Exam Attempt</h1>
                    <div className="field">
                        <div id="exam-progress" className="is-flex">
                            <div className="progress-bar">
                                <progress className="progress is-primary" value={questionsAnswered} max={totalQuestions}>15%</progress>
                            </div>
                            <div className="progress-text tags has-addons">
                                <span className="tag">Progress</span>
                                <span className="tag is-link is-warning">{questionsAnswered} / {totalQuestions}</span>
                            </div>
                        </div>
                    </div>

                    <div>
                        {QuestionData.questions.map((question: QuestionType, index) => (
                            <Question
                                questionNumber={index}
                                recordAnswer={recordAnswer}
                                questionText={question.text}
                                answers={question.answers}
                                questionId={question.question_id}
                                disabled={isExamDisabled}
                            />
                        ))
                        }
                    </div>

                    <div className="field submit is-flex">
                        <div className="control">
                            <input disabled={isExamDisabled} className="button is-primary" type="submit" value="Submit Exam" />
                        </div>
                        <div id="score-display" className="control">
                            <div className="tags are-medium has-addons">
                                <span className="tag" onClick={() => submitExam()}>Your Score</span>
                                <span className="tag is-dark">{score}%</span>
                            </div>
                        </div>
                    </div>
                </div>
            </form>
        </div>
    );
}
